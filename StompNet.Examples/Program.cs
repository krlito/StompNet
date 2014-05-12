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
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Stomp.Net.Examples
{
    /// <summary>
    /// This is the entry point to the project contains a set of examples on how to use StompNet.
    /// 
    /// You can test these using a clean installation of [Apache Apollo][http://activemq.apache.org/apollo/].
    /// </summary>
    partial class Program
    {
        //Parameters to configure the examples.
        private const string serverHostname = "localhost";
        private const int    serverPort = 61613;
        private const string virtualHost = "localhost";
        private const string login = "admin";
        private const string passcode = "password";
        private const string aQueueName = "/queue/stompNetExampleQueueONE";
        private const string anotherQueueName = "/queue/stompNetExampleQueueTWO";
        private const string aTopicName = "/topic/stompNetExampleTopic";
        private const string messageContent = "Hola Mundo!";

        static void Main()
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            bool exampling = true;

            while (exampling)
            {
                WriteTitle("StompNet Examples");
                Console.WriteLine("1. Send/receive MESSAGES using hi-level API (IStompConnector)");
                Console.WriteLine("2. Another send/receive MESSAGES example using hi-level API (IStompConnector)");
                Console.WriteLine("3. Concurrent send/receive using hi-level API (IStompConnector)");
                Console.WriteLine("4. Transactions using hi-level API (IStompConnector)");
                Console.WriteLine("5. Send/receive COMMANDS using mid-level API (IStompClient)");
                Console.WriteLine("6. Send/receive COMMANDS using lo-level API (IStompFrame(Writer|Reader))");
                Console.WriteLine("7. Exit");
                Console.Write("Select operation: ");
                int op;
                int.TryParse(Console.ReadLine(), out op);

                Console.WriteLine();
                Console.WriteLine();
                try
                {
                    switch (op)
                    {
                        case 1:
                            await ExampleConnector();
                            break;
                        case 2:
                            await ExampleConnectorAnother();
                            break;
                        case 3:
                            await ExampleConnectorConcurrent();
                            break;
                        case 4:
                            await ExampleConnectorTransaction();
                            break;
                        case 5:
                            await ExampleClient();
                            break;
                        case 6:
                            await ExampleWriterAndReader();
                            break;
                        case 7:
                            exampling = false;
                            break;
                        default:
                            WriteTitle("Invalid Selection");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is SocketException)
                        WriteTitle("Verify network connection, configuration parameters and broker status.");
                    else
                        throw;
                }
                
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        //Helper to write titles.
        static void WriteTitle(string str)
        {
            Console.WriteLine(new string('-', str.Length + 4));
            Console.WriteLine("- " + str + " -");
            Console.WriteLine(new string('-', str.Length + 4));
        }
    }
}
