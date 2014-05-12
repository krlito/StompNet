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
using System.Collections.Generic;

namespace StompNet.Helpers
{
    /// <summary>
    /// Some IEnumerable extensions.
    /// </summary>
    internal static class EnumerableExtensions
    {
    	/// <summary>
    	/// For each element of the IEnumerable, invoke an action.
    	/// </summary>
    	/// <param name="source">IEnumerable source of the elements.</param>
    	/// <param name="action">Action to be invoked for each element of the IEnumerable.</param>
        public static void Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// For each element of the IEnumerable, invoke an action.
        /// If the action fails, continue with the next one.
        /// </summary>
        /// <param name="source">IEnumerable source of the elements.</param>
        /// <param name="action">Action to be invoked for each element of the IEnumerable.</param>
        public static void DoIgnoringExceptions<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                try
                {
                    action(item);
                }
                catch { }
            }
        }
    }
}
