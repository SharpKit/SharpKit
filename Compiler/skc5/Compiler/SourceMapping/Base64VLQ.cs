using System.Text;
using System.Collections.Generic;
using System;
using System.Collections;
namespace SharpKit.Compiler.SourceMapping
{
    class Base64VLQ
    {

        // A Base64 VLQ digit can represent 5 bits, so it is base-32.
        private static int VLQ_BASE_SHIFT = 5;
        private static int VLQ_BASE = 1 << VLQ_BASE_SHIFT;

        // A mask of bits for a VLQ digit (11111), 31 decimal.
        private static int VLQ_BASE_MASK = VLQ_BASE - 1;

        // The continuation bit is the 6th bit.
        private static int VLQ_CONTINUATION_BIT = VLQ_BASE;

        /**
         * Converts from a two-complement value to a value where the sign bit is
         * is placed in the least significant bit.  For example, as decimals:
         *   1 becomes 2 (10 binary), -1 becomes 3 (11 binary)
         *   2 becomes 4 (100 binary), -2 becomes 5 (101 binary)
         */
        private static int ToVLQSigned(int value)
        {
            if (value < 0)
            {
                return ((-value) << 1) + 1;
            }
            else
            {
                return (value << 1) + 0;
            }
        }

        /**
         * Converts to a two-complement value from a value where the sign bit is
         * is placed in the least significant bit.  For example, as decimals:
         *   2 (10 binary) becomes 1, 3 (11 binary) becomes -1
         *   4 (100 binary) becomes 2, 5 (101 binary) becomes -2
         */
        private static int FromVLQSigned(int value)
        {
            bool negate = (value & 1) == 1;
            value = value >> 1;
            var x = negate ? -value : value;
            return x;
        }

        /**
         * Writes a VLQ encoded value to the provide appendable.
         * @throws IOException
         */
        public static void Encode(StringBuilder sb, int value)
        {
            value = ToVLQSigned(value);
            do
            {
                int digit = value & VLQ_BASE_MASK;
                value >>= VLQ_BASE_SHIFT;
                if (value > 0)
                {
                    digit |= VLQ_CONTINUATION_BIT;
                }
                var base64 = Base64.toBase64(digit);
                var xxx = Convert.ToBase64String(new byte[] { (byte)digit });
                sb.Append(base64);
            } while (value > 0);
        }

        /**
         * Decodes the next VLQValue from the provided CharIterator.
         */
        public static int? Decode(IEnumerator<char> in2)
        {
            int result = 0;
            bool continuation;
            int shift = 0;
            do
            {
                if (!in2.MoveNext())
                    return null;
                var c = in2.Current;
                int digit = Base64.fromBase64(c);
                continuation = (digit & VLQ_CONTINUATION_BIT) != 0;
                digit &= VLQ_BASE_MASK;
                result = result + (digit << shift);
                shift = shift + VLQ_BASE_SHIFT;
            } while (continuation);

            return FromVLQSigned(result);
        }
    }
}