using System;
using HappyTravel.Money.Models;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class MoneyAmountExtensions
    {
        public static int GetDecimalDigitsCount(this MoneyAmount tipsAmount)
            =>  (Decimal.GetBits(tipsAmount.Amount)[3] >> 16) & 0x000000FF;
    }
}