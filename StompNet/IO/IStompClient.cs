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

namespace StompNet.IO
{
    /// <summary>
    /// Stomp client interface.
    /// </summary>
    public interface IStompClient : IStompFrameObservable, IStompFrameWriter
    {
        /// <summary>
        /// Get next receipt id.
        /// </summary>
        /// <returns>The next receipt id.</returns>
        string GetNextReceiptId();

        /// <summary>
        /// Get next subscription id.
        /// </summary>
        /// <returns>The next subscription id.</returns>
        string GetNextSubscriptionId();

        /// <summary>
        /// Get next transaction id.
        /// </summary>
        /// <returns>The next transaction id.</returns>
        string GetNextTransactionId();
    }
}
