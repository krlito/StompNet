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
using StompNet.Models.Frames;

namespace StompNet.Models
{
    /// <summary>
    /// Class to transform frames into an instance of a 'subclass' matching its command.
    /// </summary>
    public static class StompInterpreter
    {
        /// <summary>
        /// Transform a frame into a 'subclass' matching its command.
        /// These subclasses are: ReceiptFrame, MessageFrame, ErrorFrame, ConnectedFrame.
        /// </summary>
        /// <param name="frame">Frame to be interpreted.</param>
        /// <returns>A new frame which class matches its command type.</returns>
        public static Frame Interpret(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException("frame");

            switch (frame.Command)
            {
                case StompCommands.Heartbeat:
                    return frame;
                case StompCommands.Receipt:
                    if (frame.BodyArray != Frame.EmptyBody)
                        throw new InvalidDataException("Receipt frame MUST NOT have a body.");
                    return new ReceiptFrame(frame.Headers);
                case StompCommands.Message:
                    return new MessageFrame(frame.Headers, frame.BodyArray);
                case StompCommands.Error:
                    return new ErrorFrame(frame.Headers, frame.BodyArray);
                case StompCommands.Connected:
                    if (frame.BodyArray != Frame.EmptyBody)
                        throw new InvalidDataException("Connected frame MUST NOT have a body.");
                    return new ConnectedFrame(frame.Headers);
                default:
                    throw new InvalidDataException(string.Format("'{0}' is not a valid STOMP server command.", frame.Command));
            }
        }
    }
}
