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

namespace StompNet.Helpers
{
	/// <summary>
	/// Sequence number generator starting at 1.
	/// 
	/// This class is thread-safe.
	/// </summary>
    internal class SequenceNumberGenerator : ISequenceNumberGenerator
	{
	    private int _count = 0;

        public int Next()
        {
            return Interlocked.Increment(ref _count);
        }
    }
}
