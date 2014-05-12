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
	/// General-purpose IObserver configured using custom actions/delegates. 
	/// </summary>
    internal class ActionObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="onNext">Action to be invoked when the observable calls OnNext.</param>
        /// <param name="onError">Action to be invoked when the observable calls OnError.</param>
        /// <param name="onCompleted">Action to be invoked when the observable calls OnCompleted.</param>
        public ActionObserver(Action<T> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnNext(T value)
        {
            if(_onNext != null) _onNext(value);
        }

        public void OnError(Exception error)
        {
            if (_onError != null) _onError(error);
        }

        public void OnCompleted()
        {
            if (_onCompleted != null) _onCompleted();
        }
    }
}
