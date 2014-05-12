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

namespace StompNet.Models.Frames
{
    /// <summary>
    /// Class representing a Heartbeat.
    /// </summary>
    public class Heartbeat
    {
        public readonly static Heartbeat NoHeartbeat = new Heartbeat(0, 0);

        public int Outgoing { get; private set; }
        public int Incoming { get; private set; }
        public string RawHeartbeat { get; private set; }

        public Heartbeat(int outgoing, int incoming)
        {
            Outgoing = outgoing;
            Incoming = incoming;
            RawHeartbeat = outgoing + "," + incoming;
        }

        public static Heartbeat GetHeartbeat(string heartbeat)
        {
            if (string.IsNullOrEmpty(heartbeat))
                return NoHeartbeat;

            string[] heartBeatParts = heartbeat.Split(',');

            if (heartBeatParts.Length != 2)
                throw new FormatException("A heart-beat header MUST contain two positive integers separated by a comma.");

            int outgoing, incoming;
            if (!int.TryParse(heartBeatParts[0], out outgoing) || outgoing < 0 || !int.TryParse(heartBeatParts[1], out incoming) || incoming < 0)
                throw new FormatException("A heart-beat header MUST contain two positive integers separated by a comma.");

            if(outgoing == 0 && incoming == 0)
                return NoHeartbeat;

            return new Heartbeat(outgoing, incoming);
        }
    }
}
