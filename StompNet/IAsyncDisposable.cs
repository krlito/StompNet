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

namespace StompNet
{
    /// <summary>
    /// It's like an IDisposable, but with an async dispose method.
    /// </summary>
    public interface IAsyncDisposable : IDisposable
    {
        Task DisposeAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// IAsyncDisposable Extensions.
    /// </summary>
    public static class AsyncDisposableExtensions
    {
        /// <summary>
        /// Dispose asynchronously with no cancellation token.
        /// </summary>
        /// <param name="disposable"></param>
        /// <returns>A Task representing the dispose operation.</returns>
        public static Task DisposeAsync(this IAsyncDisposable disposable)
        {
            return disposable.DisposeAsync(CancellationToken.None);
        }
    }
}
