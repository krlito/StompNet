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
using System.Linq;
using System.Net.Mime;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using StompNet;
using StompNet.Models;
using StompNet.Models.Frames;

namespace Stomp.Net.Examples
{

    partial class Program
    {
        /// <summary>
        /// This example shows the basic usage of the high level API of the library.
        /// 
        /// Four messages are going to be sent to a queue and they will be read from the same queue.
        /// 
        /// High level API features.
        /// - High level interfaces to ease the flow of the application: IStompConnector,
        ///   IStompConnection, IStompMessage, IStompTransaction and IObserver&lt;IStompMessage&gt;.
        /// - Built upon an observer pattern. It can be used for reactive application approaches.
        /// - Message receipt confirmations and sequence number generation for frames are 
        ///   automatically handled by the library.
        /// - Thread-safety.
        /// - You would rarely have to interact with STOMP frames directly.
        ///
        /// </summary>
        public static async Task ExampleConnector()
        {
            //A connection to the TCP server socket is created.
            using (TcpClient tcpClient = new TcpClient(serverHostname, serverPort))

            // IStompConnector is an IDisposable. A Stomp12Connector parametrized using the constructor.
            // Stomp12Connector receives as parameters: a stream, the name of the virtual host, 
            // a username (optional) and a password (optional).
            // Stomp12Connector may also receive a parameter 'retryTimeout' (TimeSpan, by default: 30 seconds)
            // which is the time before a command is re-send if it does not receive a response.
            using (IStompConnector stompConnector = new Stomp12Connector(tcpClient.GetStream(), virtualHost, login, passcode))
            {
                //Connect to the STOMP service.
                IStompConnection connection = await stompConnector.ConnectAsync(heartbeat: new Heartbeat(30000, 30000));

                // Send a couple of messages with string content.
                for (int i = 1; i <= 2; i++)
                    await connection.SendAsync(
                        aQueueName,                 // Queue/Topic to send messages to.
                        messageContent + " #" + i,  // The message content. String (by default UTF-8 encoding is used).
                        false,                      // Optional - Does a receipt confirmation is used in SEND command?. Default: false.
                        null,                       // Optional - Collection of key-value pairs to include as non-standard headers.
                        CancellationToken.None);    // Optional - A CancellationToken.

                // Subscribe to receive messages.
                // Calling 'SubscribeAsync' returns an IDisposable which can be called to unsubscribe.
                // It is not mandatory to dispose the subscription manually. It will be disposed when
                // the stompConnector is disposed or if an exception in the stream occurs.
                IDisposable subscription = 
                    await connection.SubscribeAsync(
                        new ExampleObserver(),          // An observer. A class implementing IObserver<IStompMessage>.
                        aQueueName,                     // Queue/Topic to observe from.
                        StompAckValues.AckAutoValue,    // Optional - Ack mode: "auto", "client", "client-individual". Default: "auto".
                        false,                          // Optional - Does a receipt confirmation is used in SUBSCRIBE command?. Default: false.
                        null,                           // Optional - Collection of key-value pairs to include as non-standard headers.
                        CancellationToken.None);        // Optional - A CancellationToken.

                // Messages with byte content can also be send.
                byte[] aByteMessageContent = new byte [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                // Send two more messages using SendAsync byte version.
                for (int i = 0; i < 2; i++ )
                    await connection.SendAsync(
                        aQueueName,                         // Queue/Topic to send messages to.
                        aByteMessageContent,                // The message content.
                        MediaTypeNames.Application.Octet,   // Message content type.
                        true,                               // Optional - Does a receipt confirmation is used in SEND command?. Default: false.
                                                            // In this case, a confirmation receipt will be awaited before the task is completed.
                        null,                               // Optional - Collection of key-value pairs to include as non-standard headers.
                        CancellationToken.None);            // Optional - A CancellationToken.

                // Wait some time for the messages to be received.
                await Task.Delay(500);

                // Unsubscribe.
                subscription.Dispose();

                // Disconnect.
                await connection.DisconnectAsync();
            }
        }

        /// <summary>
        /// Class observer of STOMP incoming messages.
        /// </summary>
        class ExampleObserver : IObserver<IStompMessage>
        {
            private string _name;

            public ExampleObserver(string name = null)
            {
                _name = name ?? "OBSERVER";
            }

            /// <summary>
            /// Each message from the subscription is processed on this method.
            /// </summary>
            public void OnNext(IStompMessage message)
            {
                Console.WriteLine("RECEIVED ON '{0}'", _name);
                
                if(message.ContentType == MediaTypeNames.Application.Octet)
                    Console.WriteLine("Message Content (HEX): " + string.Join(" ", message.Content.Select(b => b.ToString("X2"))));
                else
                    Console.WriteLine("Message Content: " + message.GetContentAsString());
                
                Console.WriteLine();

                // If the message is acknowledgeable then acknowledge.
                if (message.IsAcknowledgeable)
                    message.Acknowledge(); // This command may receive as parameters: a receipt flag, extra-headers and/or a cancellation token.
                                           // There is also message.AcknowledgeNegative();
            }

            /// <summary>
            /// Any error on the input stream will come through this method.
            /// If an ERROR frame is received, this method will also be called 
            /// with a ErrorFrameException as parameter.
            /// 
            /// If this method is invoked, no more messages will be received in
            /// this subscriptions.
            /// </summary>
            public void OnError(Exception error)
            {
                Console.WriteLine("EXCEPTION ON '{0}'", _name);
                Console.WriteLine(error.Message);
                Console.WriteLine();
            }

            /// <summary>
            /// This method is invoked when unsubscribing.
            /// 
            /// If this method is invoked, no more messages will be received in
            /// this subscriptions.
            /// </summary>
            public void OnCompleted()
            {
                Console.WriteLine("OBSERVING ON '{0}' IS COMPLETED.", _name);
                Console.WriteLine();
            }
        }
    }
}
