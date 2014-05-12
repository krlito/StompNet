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
	/// Some IObservable extensions. 
	/// </summary>
    public static class ObservableExtensions
    {
    	/// <summary>
    	/// Allows to subscribe to an observable using delegates.
    	/// </summary>
    	/// <param name="observable">Observable to be subscribed to.</param>
        /// <param name="onNext">Action to be invoked when the observable calls OnNext.</param>
        /// <param name="onError">Action to be invoked when the observable calls OnError.</param>
        /// <param name="onCompleted">Action to be invoked when the observable calls OnCompleted.</param>
    	/// <returns>An IDisposable which ca be used to unsubscribe.</returns>
        public static IDisposable SubscribeEx<T>(this IObservable<T> observable, Action<T> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
        {
            return observable.Subscribe(new ActionObserver<T>(onNext, onError, onCompleted));
        }

    }
}
