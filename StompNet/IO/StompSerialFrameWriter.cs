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

using System.Threading;
using System.Threading.Tasks;
using StompNet.Helpers;
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// Wrapper around an IStompFrameWriter for thread-safety. 
    /// STOMP frames will be written in a sequential/serial FIFO manner.
    /// </summary>
    public class StompSerialFrameWriter : IStompFrameWriter
    {
        private readonly IStompFrameWriter _writer;
        private readonly ITaskExecuter _serialTaskExecuter;

        public string ProtocolVersion
        {
            get { return _writer.ProtocolVersion; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="writer">Frame writer to be wrapped.</param>
        public StompSerialFrameWriter(IStompFrameWriter writer)
        {
            _writer = writer;
            _serialTaskExecuter = new SerialTaskExecuter();
        }
        
        public Task WriteAsync(Frame frame, CancellationToken cancellationToken)
        {
            return _serialTaskExecuter.Execute(() => _writer.WriteAsync(frame, cancellationToken), cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
