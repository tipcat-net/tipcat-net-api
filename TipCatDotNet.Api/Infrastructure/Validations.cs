using System;
using System.Text.RegularExpressions;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class Validations
    {
        public static bool IsEmail(string param, bool nullable = true)
        {
            if (param == null)
            {
                return nullable;
            }
            
            var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            var match = regex.Match(param);
            return match.Success;
        }
    }
}