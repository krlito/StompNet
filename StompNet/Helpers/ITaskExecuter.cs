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

namespace StompNet.Helpers
{
    /// <summary>
    /// Interface for a task executer/scheduler.
    /// </summary>
    internal interface ITaskExecuter
    {
        /// <summary>
        /// Execute a task.
        /// </summary>
        /// <param name="task">A function that returns the task to be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the scheduled task operation.</returns>
        Task Execute(Func<Task> task, CancellationToken cancellationToken);
    }

	/// <summary>
	/// Interface for a task executer/scheduler.
	/// </summary>
    internal interface ITaskExecuter<T> : ITaskExecuter
    {
        /// <summary>
        /// Execute a task.
        /// </summary>
        /// <param name="task">A function that returns the task to be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the scheduled task operation. The returned task result is the same as the original task's result.</returns>
        Task<T> Execute(Func<Task<T>> task, CancellationToken cancellationToken);
    }
}