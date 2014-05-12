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
 
using System.Collections.Generic;

namespace StompNet.Models.Frames
{
    /// <summary>
    /// Class representing a STOMP CONNECTED frame.
    /// </summary>
    public class ConnectedFrame : Frame
    {
        public string Version { get; private set; }

        public string Session { get; private set; }

        public string Server { get; private set; }

        public Heartbeat Heartbeat { get; private set; }
        
        public ConnectedFrame(IEnumerable<KeyValuePair<string, string>> headers) 
            : base (StompCommands.Connected, headers)
        {
            int missingHeaders = 4;

            foreach (var header in Headers)
            {
                if (Version == null && header.Key == StompHeaders.Version)
                {
                    Version = header.Value;
                    missingHeaders--;
                }
                else if (Session == null && header.Key == StompHeaders.Session)
                {
                    Session = header.Value;
                    missingHeaders--;
                }
                else if (Server == null && header.Key == StompHeaders.Server)
                {
                    Server = header.Value;
                    missingHeaders--;
                }
                else if (Heartbeat == null && header.Key == StompHeaders.Heartbeat)
                {
                    Heartbeat = Heartbeat.GetHeartbeat(header.Value);
                    missingHeaders--;
                }

                if (missingHeaders == 0) break;
            }

            if (missingHeaders != 0)
            {
                if (string.IsNullOrEmpty(Version))
                {
                    Version = "1.0";
                }
            }
        }
    }
}
