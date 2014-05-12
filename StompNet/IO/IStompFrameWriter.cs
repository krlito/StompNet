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
using System.Threading;
using System.Threading.Tasks;
using StompNet.Models.Frames;

namespace StompNet.IO
{
	/// <summary>
	/// Contract of a Frame Writer.
	/// </summary>
    public interface IStompFrameWriter : IDisposable
    {
        /// <summary>
        /// Version of the STOMP protocol.
        /// </summary>
	    string ProtocolVersion { get; }

        /// <summary>
        /// Write a frame.
        /// </summary>
        /// <param name="frame">Frame to be written.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the write operation.</returns>
        Task WriteAsync(Frame frame, CancellationToken cancellationToken);
    }
    
    /// <summary>
    /// So, you do not like using CancellationTokens :D. Use this extension!
    /// </summary>
    public static class FrameWriterExtensions
    {
    	public static Task WriteAsync(this IStompFrameWriter writer, Frame frame)
    	{
    		return writer.WriteAsync(frame, CancellationToken.None);
    	}
    }
}