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
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using StompNet;

namespace Stomp.Net.Examples
{
    partial class Program
    {
        /// <summary>
        /// Example used in the README.md file.
        /// </summary>
        static async Task ReadmeExample()
        {
            // Establish a TCP connection with the STOMP service.
            using (TcpClient tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync("localhost", 61613);

                //Create a connector.
                using (IStompConnector stompConnector =
                    new Stomp12Connector(
                        tcpClient.GetStream(),
                        "localhost", // Virtual host name.
                        "admin",
                        "password"))
                {
                    // Create a connection.
                    IStompConnection connection = await stompConnector.ConnectAsync();

                    // Send a message.
                    await connection.SendAsync("/queue/example", "Anybody there!?");

                    // Send two messages using a transaction.
                    IStompTransaction transaction = await connection.BeginTransactionAsync();
                    await transaction.SendAsync("/queue/example", "Hi!");
                    await transaction.SendAsync("/queue/example", "My name is StompNet");
                    await transaction.CommitAsync();

                    // Receive messages back.
                    // Message handling is made by the ConsoleWriterObserver instance.
                    await transaction.SubscribeAsync(
                        new ConsoleWriterObserver(),
                        "/queue/example");

                    // Wait for messages to be received.
                    await Task.Delay(250);

                    // Disconnect.
                    await connection.DisconnectAsync();
                }
            }
        }

        class ConsoleWriterObserver : IObserver<IStompMessage>
        {
            public void OnNext(IStompMessage message)
            {
                Console.WriteLine("MESSAGE: " + message.GetContentAsString());

                if (message.IsAcknowledgeable)
                    message.Acknowledge();
            }

            // ERROR frames come through here.
            public void OnError(Exception error)
            {
                Console.WriteLine("ERROR: " + error.Message);
            }

            public void OnCompleted()
            {
                Console.WriteLine("UNSUBSCRIBED!");
            }
        }
    }
}
