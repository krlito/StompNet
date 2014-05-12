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
    /// Wrapper around an IStompFrameReader for thread-safety. 
    /// STOMP frames will be read in a sequential/serial FIFO manner.
    /// </summary>
    public class StompSerialFrameReader : IStompFrameReader
    {
        private readonly IStompFrameReader _reader;
        private readonly ITaskExecuter<Frame> _serialTaskExecuter;

        public string ProtocolVersion
        {
            get { return _reader.ProtocolVersion; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader">Frame reader to be wrapped.</param>
        public StompSerialFrameReader(IStompFrameReader reader)
        {
            _reader = reader;
            _serialTaskExecuter = new SerialTaskExecuter<Frame>();
        }
        
        public Task<Frame> ReadFrameAsync(CancellationToken cancellationToken)
        {
            return _serialTaskExecuter.Execute(() => _reader.ReadFrameAsync(cancellationToken), cancellationToken);
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
