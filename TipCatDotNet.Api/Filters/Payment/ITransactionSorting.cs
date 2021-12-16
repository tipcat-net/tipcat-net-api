using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.Common.Enums;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Enums;

namespace TipCatDotNet.Api.Filters.Payment;

public interface ITransactionSorting
{
    Task<Result<List<TransactionResponse>>> ByCreatedASC(int memberId, int skip, int top,
        TransactionFilterProperty property, SortVariant variant, CancellationToken cancellationToken);

    Task<Result<List<TransactionResponse>>> ByCreatedDESC(int memberId, int skip, int top,
        TransactionFilterProperty property, SortVariant variant, CancellationToken cancellationToken);

    Task<Result<List<TransactionResponse>>> ByAmountASC(int memberId, int skip, int top,
        TransactionFilterProperty property, SortVariant variant, CancellationToken cancellationToken);

    Task<Result<List<TransactionResponse>>> ByAmountDESC(int memberId, int skip, int top,
        TransactionFilterProperty property, SortVariant variant, CancellationToken cancellationToken);
}
