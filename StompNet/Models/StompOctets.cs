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

using System.IO;

namespace StompNet.Models
{
    /// <summary>
    /// STOMP protocol special octets. 
    /// This class is used to escape/unescape headers for STOMP 1.2.
    /// </summary>
    internal static class StompOctets
    {
        public const char Backslash = '\\';
        public const char CarriageReturn = '\r';
        public const char Colon = ':';
        public const char EndOfFrame = '\0';
        public const char LineFeed = '\n';

        public const char EscapedBackslash = '\\';
        public const char EscapedCarriageReturn = 'r';
        public const char EscapedColon = 'c';
        public const char EscapedLineFeed = 'n';

        public const byte BackslashByte = 92;
        public const byte CarriageReturnByte = 13;
        public const byte ColonByte = 58;
        public const byte EndOfFrameByte = 0;
        public const byte LineFeedByte = 10;

        public const byte EscapedBackslashByte = 92; /*\*/
        public const byte EscapedCarriageReturnByte = 114; /*r*/
        public const byte EscapedColonByte = 99; /*c*/
        public const byte EscapedLineFeedByte = 110; /*n*/

        public static readonly byte[] EscapedBackslashArray = new[] { BackslashByte, EscapedBackslashByte };
        public static readonly byte[] EscapedCarriageReturnArray = new[] { BackslashByte, EscapedCarriageReturnByte };
        public static readonly byte[] EscapedColonArray = new[] { BackslashByte, EscapedColonByte };
        public static readonly byte[] EscapedLineFeedArray = new[] { BackslashByte, EscapedLineFeedByte };

        public static byte[] EscapeOctet(char octet)
        {
            switch (octet)
            {
                case Colon:
                    return EscapedColonArray;
                case Backslash:
                    return EscapedBackslashArray;
                case CarriageReturn:
                    return EscapedCarriageReturnArray;
                case LineFeed:
                    return EscapedLineFeedArray;
                default:
                    return null;
            }
        }

        public static char UnescapeOctet(char octet)
        {
            switch (octet)
            {
                case EscapedColon:
                    return Colon;
                case EscapedBackslash:
                    return Backslash;
                case EscapedLineFeed:
                    return LineFeed;
                case EscapedCarriageReturn:
                    return CarriageReturn;
                default:
                    throw new InvalidDataException(string.Format("Escape sequence '\\{0}' is invalid.'", octet));
            }
        }
    }
}
