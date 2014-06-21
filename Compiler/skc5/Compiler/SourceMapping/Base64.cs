using System.Diagnostics;
using System;

/*
 * Copyright 2011 The Closure Compiler Authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace SharpKit.Compiler.SourceMapping
{


    ///
    /// <summary> * A utility class for working with Base64 values.
    /// * @author johnlenz@google.com (John Lenz) </summary>
    /// 
    internal sealed class Base64
    {

        // This is a utility class
        private Base64()
        {
        }

        ///  
        ///   <summary> *  A map used to convert integer values in the range 0-63 to their base64
        ///   *  values. </summary>
        ///   
        private const string BASE64_MAP = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "abcdefghijklmnopqrstuvwxyz" + "0123456789+/";

        ///  
        ///   <summary> * A map used to convert base64 character into integer values. </summary>
        ///   
        private static readonly int[] BASE64_DECODE_MAP = new int[256];
        static Base64()
        {
            for (int i = 0; i < BASE64_DECODE_MAP.Length; i++)
            {
                BASE64_DECODE_MAP[i] = -1;
            }
            for (int i = 0; i < BASE64_MAP.Length; i++)
            {
                BASE64_DECODE_MAP[BASE64_MAP[i]] = i;
            }
        }

        ///  
        ///   * <param name="value"> A value in the range of 0-63. </param>
        ///   * <returns> a base64 digit. </returns>
        ///   
        public static char toBase64(int value)
        {
            Debug.Assert((value <= 63 && value >= 0), "value out of range:" + value);
            return BASE64_MAP[value];
        }

        ///  
        ///   * <param name="c"> A base64 digit. </param>
        ///   * <returns> A value in the range of 0-63. </returns>
        ///   
        public static int fromBase64(char c)
        {
            int result = BASE64_DECODE_MAP[c];
            Debug.Assert((result != -1), "invalid char");
            return BASE64_DECODE_MAP[c];
        }
    }
}