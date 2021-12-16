using System;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Extensions;
using Stripe;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class MoneyConverting
    {
        public static decimal ToFractionalUnits(in PaymentIntent paymentIntent)
            => paymentIntent.Amount / (decimal)Math.Pow(10, ToCurrency(paymentIntent.Currency).GetDecimalDigitsCount());


        public static Currencies ToCurrency(string currency)
            => Enum.Parse<Currencies>(currency.ToUpper());
    }
}