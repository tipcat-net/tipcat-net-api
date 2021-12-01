using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using TipCatDotNet.Api.Models.Payments;

namespace TipCatDotNet.Api.Services.Payments;

public class ProFormaInvoiceService : IProFormaInvoiceService
{
    public async Task<ProFormaInvoice> Get(CancellationToken cancellationToken)
    {
        var serviceFee = new MoneyAmount(2.9m, Currencies.AED);

        return await Task.FromResult(new ProFormaInvoice(amount: null, serviceFee));
    }
}