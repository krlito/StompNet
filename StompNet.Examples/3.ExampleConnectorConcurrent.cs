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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using StompNet;
using StompNet.Models;

namespace Stomp.Net.Examples
{
    partial class Program
    {
        /// <summary>
        /// This example will demonstrate the thread-safety of IStompConnector
        /// </summary>
        /// <returns></returns>
        public static async Task ExampleConnectorConcurrent()
        {
            using (TcpClient tcpClient = new TcpClient(serverHostname, serverPort))
            using (IStompConnector stompConnector = new Stomp12Connector(tcpClient.GetStream(), virtualHost, login, passcode))
            {
                IStompConnection connection = await stompConnector.ConnectAsync();
                
                // Create threads to send messages.
                ICollection<Thread> threads = new Collection<Thread>();
                for (int i = 0; i < 10; i++)
                {
                    Thread t = new Thread(
                        async () =>
                            {
                                for(int j = 0; j < 100; j++)
                                    await connection.SendAsync(aQueueName, messageContent);
                            });
                    threads.Add(t);
                }

                // Start threads
                foreach(Thread t in threads)
                    t.Start();

                // Subscribe
                IDisposable subscription = 
                    await connection.SubscribeAsync(
                    new CounterObserver(), // An observer that count messages and print the count on completed.
                    aQueueName, 
                    StompAckValues.AckClientIndividualValue); // Messages must be acked before the broker discards them.

                Console.WriteLine("Please wait a few seconds...");
                Console.WriteLine();

                // Wait for the threads to finish.
                foreach (Thread t in threads)
                    t.Join();

                // Wait for a little longer for messages to arrive.
                await Task.Delay(3000);

                // Unsubscribe.
                // After this invocation, observers's OnCompleted is going to be called.
                // For this example, a count of 1000 should be shown unless there was 
                // previous data in the queue.
                subscription.Dispose();

                // Disconnect.
                await connection.DisconnectAsync();
            }
        }

        /// <summary>
        /// An observable that counts the number of messages received.
        /// When unsubscribing, this will print the count.
        /// </summary>
        class CounterObserver : IObserver<IStompMessage>
        {
            private int _count = 0;
            
            public void OnNext(IStompMessage message)
            {
                _count++;

                if (message.IsAcknowledgeable)
                    message.Acknowledge(true);
            }

            public void OnError(Exception error)
            {
                Console.WriteLine("EXCEPTION!" );
                Console.WriteLine(error.Message);
                Console.WriteLine();
            }

            public void OnCompleted()
            {
                Console.WriteLine();
                Console.WriteLine("{0} MESSAGES WERE OBSERVED.", _count);
            }
        }
    }
}
