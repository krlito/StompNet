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
    /// Class representing a STOMP ERROR frame.
    /// </summary>
    public class ErrorFrame : Frame
    {
        public string ReceiptId { get; private set; }

        public string Message { get; private set; }

        public string ContentType { get; private set; }
        
        public int ContentLength { get; private set; }

        internal ErrorFrame(IEnumerable<KeyValuePair<string, string>> headers, byte[] body = null) 
            : base (StompCommands.Error, headers, body)
        {
            int missingHeaders = 3;

            foreach (var header in Headers)
            {
                if (ReceiptId == null && header.Key == StompHeaders.ReceiptId)
                {
                    ReceiptId = header.Value;
                    missingHeaders--;
                }
                else if (Message == null && header.Key == StompHeaders.Message)
                {
                    Message = header.Value;
                    missingHeaders--;
                }
                else if (ContentType == null && header.Key == StompHeaders.ContentType)
                {
                    ContentType = header.Value;
                    missingHeaders--;
                }

                if (missingHeaders == 0) break;
            }

            ContentLength = BodyArray.Length;
        }
    }
}
