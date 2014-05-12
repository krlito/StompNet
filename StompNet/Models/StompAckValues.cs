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

namespace StompNet.Models
{
    /// <summary>
    /// Ack values as defined by STOMP protocol specification.
    /// </summary>
    public static class StompAckValues
    {
        public const string AckAutoValue = "auto";
        public const string AckClientValue = "client";
        public const string AckClientIndividualValue = "client-individual";

        private static readonly ISet<string> _ackValidValues = new HashSet<string> { AckAutoValue, AckClientIndividualValue, AckClientValue };

        public static bool IsValidAckValue(string ackValue)
        {
            return _ackValidValues.Contains(ackValue);
        }

        public static void ThrowIfInvalidAckValue(string ackValue)
        {
            if(!_ackValidValues.Contains(ackValue))
                throw new ArgumentException(string.Format("{0} header value MUST be: '{1}', '{2}' or '{3}'", StompHeaders.Ack, AckAutoValue, AckClientIndividualValue, AckClientValue));

        }
    }
}
