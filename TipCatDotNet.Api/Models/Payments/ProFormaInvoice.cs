using HappyTravel.Money.Models;

namespace TipCatDotNet.Api.Models.Payments;

public readonly struct ProFormaInvoice
{
    public ProFormaInvoice(MoneyAmount? amount, MoneyAmount serviceFee)
    {
        Amount = amount;
        ServiceFee = serviceFee;
    }


    public MoneyAmount? Amount { get; }
    public MoneyAmount ServiceFee { get; }
}