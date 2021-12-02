using System.Threading;
using System.Threading.Tasks;
using TipCatDotNet.Api.Models.Payments;

namespace TipCatDotNet.Api.Services.Payments;

public interface IProFormaInvoiceService
{
    Task<ProFormaInvoice> Get(CancellationToken cancellationToken);
}