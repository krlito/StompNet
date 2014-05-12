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
        /// This is another example of the basic usage of the high level API of the library.
        /// 
        /// Multiple messages are going to be sent to a topic and a queue. After that,
        /// multiple observers will receive the messages.
        /// </summary>
        /// <returns></returns>
        public static async Task ExampleConnectorAnother()
        {
            using (TcpClient tcpClient = new TcpClient(serverHostname, serverPort))
            using (IStompConnector stompConnector = new Stomp12Connector(tcpClient.GetStream(), virtualHost, login, passcode))
            {
                IStompConnection connection = await stompConnector.ConnectAsync();
                //
                // QUEUE CASE
                //
                Console.WriteLine("TWO OBSERVERS ON A QUEUE");
                Console.WriteLine("------------------------");
                Console.WriteLine();
                
                // Subscribe two observers to one queue.
                await connection.SubscribeAsync(new ExampleObserver("QUEUE OBSERVER 1"), aQueueName, StompAckValues.AckClientIndividualValue, true);
                await connection.SubscribeAsync(new ExampleObserver("QUEUE OBSERVER 2"), aQueueName, StompAckValues.AckClientIndividualValue, true);

                // Send three messages.
                for (int i = 1; i <= 3; i++)
                    // As you see the receipt flag is true, so SendAsync will wait for a receipt confirmation.
                    await connection.SendAsync(aQueueName, messageContent + " #" + i, true);

                // Wait some time for the messages to be received.
                await Task.Delay(500);

                //
                // TOPIC CASE
                //
                Console.WriteLine("TWO OBSERVERS ON A TOPIC");
                Console.WriteLine("------------------------");
                Console.WriteLine();

                // Subscribe two observers to one topic.
                await connection.SubscribeAsync(new ExampleObserver("TOPIC OBSERVER 1"), aTopicName, StompAckValues.AckClientIndividualValue, true);
                await connection.SubscribeAsync(new ExampleObserver("TOPIC OBSERVER 2"), aTopicName, StompAckValues.AckClientIndividualValue, true);

                // Send three messages. As you see the receipt flag is true.
                for (int i = 1; i <= 3; i++)
                    // As you see the receipt flag is true, so SendAsync will wait for a receipt confirmation.
                    await connection.SendAsync(aTopicName, messageContent + " #" + i, true);

                // Wait some time for the messages to be received.
                await Task.Delay(500);

                // Disconnect.
                await connection.DisconnectAsync();
            }

            // This delay is for console output formatting purposes.
            // It makes sure the OnCompleted methods of the observers are invoked before 
            // showing the main menu again. In other words, when this is removed, completion 
            // messages of the observer may appear after the main menu has been showed.
            await Task.Delay(500);
        }

        ///// <summary>
        ///// Class observer of STOMP incoming messages.
        ///// </summary>
        //class ExampleObserver : IObserver<IStompMessage>
        //{
        //    private string _name;

        //    public ExampleObserver(string name = null)
        //    {
        //        _name = name ?? "OBSERVER";
        //    }

        //    /// <summary>
        //    /// Each message from the subscription is processed on this method.
        //    /// </summary>
        //    public void OnNext(IStompMessage message)
        //    {
        //        Console.WriteLine("RECEIVED ON '{0}'", _name);
                
        //        if(message.ContentType == MediaTypeNames.Application.Octet)
        //            Console.WriteLine("Message Content (HEX): " + string.Join(" ", message.Content.Select(b => b.ToString("X2"))));
        //        else
        //            Console.WriteLine("Message Content: " + message.GetContentAsString());
                
        //        Console.WriteLine();

        //        // If the message is acknowledgeable then acknowledge.
        //        if (message.IsAcknowledgeable)
        //            message.Acknowledge(); //This command may receive as parameters: a receipt flag, extra-headers and/or a cancellation token.
        //    }

        //    /// <summary>
        //    /// Any error on the input stream or will come through this method.
        //    /// If an ERROR frame is received, this method will also be called 
        //    /// with a ErrorFrameException as parameter.
        //    /// 
        //    /// If this method is invoked, no more messages will be received in
        //    /// this subscriptions.
        //    /// </summary>
        //    public void OnError(Exception error)
        //    {
        //        Console.WriteLine("EXCEPTION ON '{0}'", _name);
        //        Console.WriteLine(error.Message);
        //        Console.WriteLine();
        //    }

        //    /// <summary>
        //    /// This method is invoked when unsubscribing.
        //    /// 
        //    /// If this method is invoked, no more messages will be received in
        //    /// this subscriptions.
        //    /// </summary>
        //    public void OnCompleted()
        //    {
        //        Console.WriteLine("OBSERVING ON '{0}' IS COMPLETED.", _name);
        //        Console.WriteLine();
        //    }
        //}
    }
}
