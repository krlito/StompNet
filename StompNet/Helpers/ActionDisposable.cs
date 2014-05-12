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

namespace StompNet.Helpers
{
	/// <summary>
	/// Disposable class that calls a custom action when it is disposed.
	/// </summary>
    internal class ActionDisposable : IDisposable
    {
        private readonly Action _action;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">Action to be invoked when this instance is disposed.</param>
        public ActionDisposable(Action action)
        {
            _action = action;
        }
        
        public void Dispose()
        {
            _action();
        }
    }
}
