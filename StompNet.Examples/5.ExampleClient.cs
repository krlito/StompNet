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
using StompNet.Helpers;
using StompNet.IO;
using StompNet.Models;
using StompNet.Models.Frames;

namespace Stomp.Net.Examples
{
    partial class Program
    {
        /// <summary>
        /// This example shows the basic usage of the mid level API of the library using IStompClient interface.
        /// 
        /// This API as the high level one is based on a Observer pattern. But IStompClient does NOT have high
        /// level classes nor it manages receipts automatically. Using this part of the library allows you to
        /// have direct access to STOMP frames.
        /// 
        /// In this example, a couple of messages are going to be sent to a queue and they will be read from 
        /// the same queue.
        /// </summary>
        public static async Task ExampleClient()
        {
            using (TcpClient tcpClient = new TcpClient(serverHostname, serverPort))
            //In this example we are creating a Stomp12Client, NOT a Stomp12Connector.
            using (IStompClient stompClient = new Stomp12Client(tcpClient.GetStream()))
            {
                // After building the client, at least one subscription to (ALL) incoming FRAMES should be made.
                // Do NOT confuse STOMP subscriptions in the high level API to these FRAME subscriptions.
                // Observers can be subscribed in two ways:
                // 1. Using SubscribeEx extension method which receives three delegate/Actions: OnNext, OnError
                // and OnCompleted.
                IDisposable subscription1 = 
                    stompClient.SubscribeEx(
                        (frame) => //This is an instance of a Frame class, not an IStompMessage.
                            {
                                Console.WriteLine("RECEIVED COMMAND: " + frame.Command);
                                Console.WriteLine();
                            }
                    );

                // 2. Implement an IObserver<Frame> and pass an instance as an argument.
                IDisposable subscription2 = 
                    stompClient.Subscribe(new MessageFrameOnlyObserver());

                // NOTE ABOUT SUBSCRIBE: Order of observer invocation is not guaranteed.

                // Start receiving messages.
                stompClient.Start();

                // Now you can write STOMP commands on the stream.

                // CONNECT Command
                await stompClient.WriteConnectAsync(virtualHost, login, passcode);

                // SUBSCRIBE Command
                string subscriptionId = stompClient.GetNextReceiptId();
                await stompClient.WriteSubscribeAsync(
                    aQueueName, 
                    stompClient.GetNextSubscriptionId(), 
                    subscriptionId);

                // SEND Command
                await stompClient.WriteSendAsync(
                    aQueueName,
                    messageContent + " (WITHOUT RECEIPT).");


                // SEND Command
                await stompClient.WriteSendAsync(
                    aQueueName, 
                    messageContent + (" (WITH RECEIPT)."), 
                    stompClient.GetNextReceiptId());
                    //Notice a receipt id has to be created manually and a RECEIPT command should be expected in the observers.

                // Wait some time for the messages to be received.
                await Task.Delay(500);

                // UNSUBSCRIBE Command
                await stompClient.WriteUnsubscribeAsync(subscriptionId);

                // DISCONNECT Command
                await stompClient.WriteDisconnectAsync();
            }
        }
    }

    /// <summary>
    /// This class will receive all kind of Frames, but it will only process message frames.
    /// </summary>
    class MessageFrameOnlyObserver : IObserver<Frame>
    {
        public void OnNext(Frame frame)
        {
            //If frame command is MESSAGE.
            if (frame.Command == StompCommands.Message)
            {
                // Using the incoming frame, Interpret will create a new instance of a sub-class 
                // of Frame depending on the Command. If the frame is a HEARTBEAT, the same frame 
                // will be returned. Possible sub-classes: ConnectedFrame, ErrorFrame, MessageFrame,
                // ReceiptFrame.
                // Interpret throws exception if the incoming frame is malformed (not standard).
                MessageFrame message = StompInterpreter.Interpret(frame) as MessageFrame;


                //Print MESSAGE frame data.
                Console.WriteLine("MESSAGE FRAME");
                Console.WriteLine("Destination: " + message.Destination);
                Console.WriteLine("ContentType: " + message.ContentType);
                Console.WriteLine("ContentLength: " + message.ContentLength);
                Console.WriteLine("Content:" + message.GetBodyAsString());
                Console.WriteLine();
            }
        }

        // IMPORTANT: ERROR frames will come through this method.
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
