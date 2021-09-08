﻿using System;
using System.Collections.Generic;

namespace TipCatDotNet.Api.Infrastructure
{
    public class MemberCodeGenerator
    {
        public static string Compute(int target)
        {
            var hash = string.Empty;
            var quotient = target;
            do
            {
                quotient = Math.DivRem(quotient, CodeNumeralSystemTable.Count, out var remainder);
                hash += CodeNumeralSystemTable[remainder];
            } while (quotient != 0);

            return hash.PadLeft(4, '0');
        }


        private static readonly Dictionary<long, string> CodeNumeralSystemTable = new()
        {
            {0, "1"},
            {1, "2"},
            {2, "3"},
            {3, "4"},
            {4, "5"},
            {5, "6"},
            {6, "7"},
            {7, "8"},
            {8, "9"},
            {9, "A"},
            {10, "B"},
            {11, "C"},
            {12, "D"},
            {13, "E"},
            {14, "F"},
            {15, "G"},
            {16, "H"},
            {17, "I"},
            {18, "J"},
            {19, "K"},
            {20, "L"},
            {21, "M"},
            {22, "N"},
            {23, "O"},
            {24, "P"},
            {25, "Q"},
            {26, "R"},
            {27, "S"},
            {28, "T"},
            {29, "U"},
            {30, "V"},
            {31, "W"},
            {32, "X"},
            {33, "Y"},
            {34, "Z"}
        };
    }
}