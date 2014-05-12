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
using StompNet.IO;
using StompNet.Models;
using StompNet.Models.Frames;

namespace Stomp.Net.Examples
{
    partial class Program
    {
        /// <summary>
        /// Example to demonstrate STOMP transactions using the StompNet low level API.
        /// 
        /// In this example, a single message is going to be sent to a queue and it will be read from 
        /// the same queue.
        /// </summary>
        public static async Task ExampleWriterAndReader()
        {
            using (TcpClient tcpClient = new TcpClient(serverHostname, serverPort))

            // Create a frame writer and frame reader. These classes are NOT thread safe.
            // Stomp12FrameWriter and Stomp12FrameReader are NOT thread safety. They could 
            // wrapped into a StompSerialFrameWriter and a StompSerialFrameReader for
            // serializing operations (thread-safe).
            using (IStompFrameWriter writer = new Stomp12FrameWriter(tcpClient.GetStream()))
            using (IStompFrameReader reader = new Stomp12FrameReader(tcpClient.GetStream()))
            {
                Frame inFrame;

                //---------------------------------
                // Write CONNECT.
                //
                await writer.WriteConnectAsync(virtualHost, login, passcode);
                
                // Read CONNECTED.
                // Keep reading while receiving Heartbeats.
                do
                {
                    inFrame = await reader.ReadFrameAsync();
                } while (inFrame.Command == StompCommands.Heartbeat);

                //Verify a CONNECTED frame was received.
                if(!AssertExpectedCommandFrame(inFrame, StompCommands.Connected))
                    return;

                Console.WriteLine("Connected");


                //---------------------------------
                // Write SEND command with receipt.
                //
                await writer.WriteSendAsync(
                    aQueueName,
                    messageContent + (" (WITH RECEIPT)."),
                    "myreceiptid-123");

                // Read RECEIPT.
                // Keep reading while receiving Heartbeats.
                do
                {
                    inFrame = await reader.ReadFrameAsync();
                } while (inFrame.Command == StompCommands.Heartbeat);

                //Verify a RECEIPT frame was received.
                if (!AssertExpectedCommandFrame(inFrame, StompCommands.Receipt))
                    return;

                // Process incoming RECEIPT.
                // Using the incoming frame, Interpret will create a new instance of a sub-class 
                // of Frame depending on the Command. If the frame is a HEARTBEAT, the same frame 
                // will be returned. Possible sub-classes: ConnectedFrame, ErrorFrame, MessageFrame,
                // ReceiptFrame.
                // Interpret throws exception if the incoming frame is malformed (not standard).
                ReceiptFrame rptFrame = StompInterpreter.Interpret(inFrame) as ReceiptFrame;

                if (rptFrame.ReceiptId != "myreceiptid-123")
                {
                    Console.WriteLine("ERROR: Unexpected receipt " + rptFrame.ReceiptId + ".");
                    return;
                    
                }
                
                Console.WriteLine("Received matching receipt.");

                //---------------------------------
                // Write SUBSCRIBE.
                //
                string subscriptionId = new Random().Next().ToString();
                await writer.WriteSubscribeAsync(aQueueName, subscriptionId, ack: StompAckValues.AckAutoValue);

                // Read MESSAGE.
                // Keep reading while receiving Heartbeats.
                do
                {
                    inFrame = await reader.ReadFrameAsync();
                } while (inFrame.Command == StompCommands.Heartbeat);

                //Verify a MESSAGE frame was received.
                if(!AssertExpectedCommandFrame(inFrame, StompCommands.Message))
                    return;

                // Process incoming RECEIPT.
                MessageFrame msgFrame = StompInterpreter.Interpret(inFrame) as MessageFrame;
                Console.WriteLine("Received Message:");
                Console.WriteLine();
                Console.WriteLine("Destination: " + msgFrame.Destination);
                Console.WriteLine("ContentType: " + msgFrame.ContentType);
                Console.WriteLine("ContentLength: " + msgFrame.ContentLength);
                Console.WriteLine("Content:" + msgFrame.GetBodyAsString());
                Console.WriteLine();

                // Write DISCONNECT.
                await writer.WriteDisconnectAsync();
                Console.WriteLine("Disconnected.");
            }
        }

        static bool AssertExpectedCommandFrame(Frame frame, string expectedFrameCommand)
        {
            // If error
            if (frame.Command == StompCommands.Error)
            {
                // Using the incoming frame, Interpret will create a new instance of a sub-class 
                // of Frame depending on the Command. If the frame is a HEARTBEAT, the same frame 
                // will be returned. Possible sub-classes: ConnectedFrame, ErrorFrame, MessageFrame,
                // ReceiptFrame.
                // Interpret throws exception if the incoming frame is malformed (not standard).
                ErrorFrame errFrame = StompInterpreter.Interpret(frame) as ErrorFrame;

                Console.WriteLine("ERROR RESPONSE");
                Console.WriteLine("Message : " + errFrame.Message);
                
                return false;
            }

            if (frame.Command != expectedFrameCommand)
            {
                Console.WriteLine("UNEXPECTED FRAME.");
                Console.WriteLine(frame.ToString());
                
                return false;
            }

            return true;
        }
    }
}
