/*******************************************************************************
 *
 *  Copyright (c) 2014 Carlos Campo <carlos@campo.com.co>
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 * 
 *******************************************************************************/

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using StompNet;
using StompNet.Models;

namespace Stomp.Net.Examples
{
    partial class Program
    {
        /// <summary>
        /// Example to demonstrate STOMP transactions using the StompNet high level API.
        /// 
        /// BEWARE: APACHE APOLLO DOES NOT HANDLE VERY MELL TRANSACTION WITH RECEIPT CONFIRMATIONS.
        /// </summary>
        public static async Task ExampleConnectorTransaction()
        {
            using (TcpClient tcpClient = new TcpClient(serverHostname, serverPort))
            using (IStompConnector stompConnector = new Stomp12Connector(tcpClient.GetStream(), virtualHost, login, passcode))
            {
                IStompConnection connection = await stompConnector.ConnectAsync();

                //
                // TRANSACTION 1
                // This transaction send two messages to a queue.
                //
                Console.WriteLine("TRANSACTION #1");
                
                IStompTransaction tr1 = await connection.BeginTransactionAsync();
                await tr1.SendAsync(aQueueName, messageContent + " #1");
                await tr1.SendAsync(aQueueName, messageContent + " #2");
                await tr1.CommitAsync();
                Console.WriteLine("Messages sent to queue: " + aQueueName + ".");
                Console.WriteLine();

                //
                // TRANSACTION 2
                // Transactionally receive messages from a queue and send rhem to another queue.
                //
                Console.WriteLine("TRANSACTION #2");
                
                IAsyncDisposable sub2 = 
                    await connection.SubscribeAsync(
                    new TransactionalObserver(connection, "#2"), //The transaction is done inside the observer.
                    aQueueName,
                    StompAckValues.AckClientIndividualValue, 
                    true);

                // Wait some time for the messages to be received and processed.
                await Task.Delay(500);
                await sub2.DisposeAsync();

                Console.WriteLine("Messages processed from " + aQueueName + " and sent to queue " + anotherQueueName + ".");
                Console.WriteLine();

                //
                // TRANSACTION 3
                // Transactionally receive messages from another queue and send them back to the original queue.
                // At the last moment, ABORT.
                //
                Console.WriteLine("TRANSACTION #3");
                
                IStompTransaction tr3 = await connection.BeginTransactionAsync();

                // Creating a subscription from a transaction guarantees that acknowledgements will make part 
                // of the transaction.
                IAsyncDisposable sub3 = 
                    await tr3.SubscribeAsync(
                    new ProcessorObserver("#3", tr3), //All processing inside the observer is automatically made part of the transaction.
                    anotherQueueName, 
                    StompAckValues.AckClientIndividualValue);

                // Wait some time for the messages to be received and processed.
                await Task.Delay(500);

                // In transaction cases, subscription SHOULD be disposed before finishing the transaction.
                // Otherwise, messages will keep being received and new (transacted) acknowledgements will 
                // be unknown for the message broker and may cause errors.
                await sub3.DisposeAsync();

                //Abort.
                await tr3.AbortAsync();

                Console.WriteLine("Nothing should have happened because this transaction was aborted.");
                Console.WriteLine();

                //
                // TRANSACTION 4
                // Transactionally receive messages from another queue and send them back to the original queue.
                // At the last moment, COMMIT.
                //
                Console.WriteLine("TRANSACTION #4");
                
                IStompTransaction tr4 = await connection.BeginTransactionAsync();
                IAsyncDisposable sub4 = await tr4.SubscribeAsync(
                    new ProcessorObserver("#4", tr4), //All processing inside the observer is automatically made part of the transaction.
                    anotherQueueName, 
                    StompAckValues.AckClientIndividualValue);

                // Wait some time for the messages to be received and processed.
                await Task.Delay(500);

                // In transaction cases, subscription SHOULD be disposed before finishing the transaction.
                // Otherwise, messages will keep being received and new (transacted) acknowledgements will 
                // be unknown for the message broker and may cause errors.
                await sub4.DisposeAsync();

                // Commit.
                await tr4.CommitAsync();

                Console.WriteLine("Messages processed from " + anotherQueueName + " and sent to queue " + aQueueName + ".");
                Console.WriteLine();

                //
                // QUEUE DUMP
                // Transactionally receive messages from another queue and send them back to the original queue.
                // At the last moment, COMMIT.
                //
                Console.WriteLine("QUEUE " + aQueueName.ToUpper() +" DUMP");
                Console.WriteLine("Show the messages in their final destination.");
                Console.WriteLine();
                IAsyncDisposable sub5 = 
                    await connection.SubscribeAsync(
                    new ExampleObserver("QUEUE " + aQueueName + " DUMP"), 
                    aQueueName, 
                    StompAckValues.AckClientIndividualValue);
                await Task.Delay(500);
                await sub4.DisposeAsync();

                // Disconnect.
                await connection.DisconnectAsync();
            }
        }

        /// <summary>
        /// Transactional Observer.
        /// 
        /// This observer creates a transaction for each message it receives.
        /// The transaction has actions: i) acknowledge the message, ii) move
        /// the message to another queue.
        /// </summary>
        class TransactionalObserver : IObserver<IStompMessage>
        {
            private IStompConnection _connection;
            private string _name;

            public TransactionalObserver(IStompConnection connection, string name)
            {
                _connection = connection;
                _name = name;
            }

            public void OnNext(IStompMessage message)
            {
                Task.Run(
                    async () =>
                        {
                            // Create a transaction.
                            IStompTransaction transaction = await _connection.BeginTransactionAsync();
                            
                            // NOTICE acknowledge receives the transaction ID to make the acknowlegment part of the transaction.
                            // BEWARE: useReceipt is false because Apache Apollo does not handle transactions and receipts in 
                            // a good manner when used at the same time.
                            await message.Acknowledge(false, transaction.Id);

                            // Send the content of the message to another queue.
                            await transaction.SendAsync(
                                anotherQueueName, 
                                message.GetContentAsString() + " TRANSACTED BY " + _name);

                            // Commit
                            await transaction.CommitAsync();
                        })
                    .Wait();
            }

            public void OnError(Exception error)
            {
                Console.WriteLine("EXCEPTION!");
                Console.WriteLine(error.Message);
                Console.WriteLine();
            }

            public void OnCompleted()
            {
                // Do nothing.
            }
        }

        /// <summary>
        /// Observer that takes a message and sends it to a queue 
        /// which name is in the parameter aQueueName.
        /// 
        /// Notice this observer does not know about the transaction
        /// because it was subscribed using the transaction instance.
        /// So, the messages it acknowledges or sends are automatically
        /// made part of the transaction.
        /// </summary>
        class ProcessorObserver : IObserver<IStompMessage>
        {
            private string _name;
            private IStompTransaction _transaction;

            public ProcessorObserver(string name, IStompTransaction transaction)
            {
                _name = name;
                _transaction = transaction;
            }

            public void OnNext(IStompMessage value)
            {
                Task.Run(
                    async () =>
                        {
                            await _transaction.SendAsync(
                                aQueueName, 
                                value.GetContentAsString() + " PROCESSED BY " + _name);
                            
                            if (value.IsAcknowledgeable)
                                await value.Acknowledge();
                        })
                    .Wait();
            }

            public void OnError(Exception error)
            {
                Console.WriteLine("EXCEPTION!");
                Console.WriteLine(error.Message);
                Console.WriteLine();
            }

            public void OnCompleted()
            {
                // Do nothing.
            }
        }
    }
}
