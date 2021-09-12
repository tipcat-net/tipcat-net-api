using System;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class EmailVerificationCodeGenerator
    {
        public static string Compute(byte length = 6)
        {
            return "111111";
            var max = GetMaxCode(length);
            var random = new Random();
            var code = random.Next(0, max).ToString();
            return ZeroFull(code, length);
        }

        public static string ToHash(string code)
        {
            return HashGenerator.ComputeSha256(code);
        }

        public static bool Compare(string hash, string code)
        {
            return hash == ToHash(code);
        }

        private static int GetMaxCode(byte length)
        {
            var max = 1;
            for (int i = 0; i < length; i++)
            {
                max *= 10;
            }

            return --max;
        }

        private static string ZeroFull(string number, byte length)
        {
            if (number.Length < length)
            {
                for (var i = 0; i < length - number.Length; i++)
                {
                    number = "0" + number;
                }
            }

            return number;
        }
    }
}