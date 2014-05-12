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
using StompNet.Models.Frames;

namespace StompNet.Exceptions
{
	/// <summary>
	/// Exception to be used when an STOMP ERROR frame is received. 
	/// </summary>
    public class ErrorFrameException : Exception
    {
        public ErrorFrame ErrorFrame { get; private set; }
        
        public ErrorFrameException(ErrorFrame errorFrame)
            : base(errorFrame != null ? errorFrame.Message : null)
        {
            if(errorFrame == null)
                throw new ArgumentNullException("errorFrame");
            ErrorFrame = errorFrame;
        }
    }
}
