using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Data.Models.Stripe;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Services.Payments;
using TipCatDotNet.Api.Models.Common.Enums;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Enums;
using TipCatDotNet.ApiTests.Utils;
using TipCatDotNet.Api.Filters.Payment;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Moq;
using Xunit;

namespace TipCatDotNet.ApiTests;

public class TransactionServiceTests
{
    public TransactionServiceTests()
    {
        var aetherDbContextMock = MockContextFactory.Create();
        aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));
        aetherDbContextMock.Setup(c => c.Transactions).Returns(DbSetMockProvider.GetDbSetMock(_transactions));
        aetherDbContextMock.Setup(c => c.StripeAccounts).Returns(DbSetMockProvider.GetDbSetMock(_stripeAccounts));

        _aetherDbContext = aetherDbContextMock.Object;

        _service = new TransactionService(_aetherDbContext, It.IsAny<ITransactionSorting>());
    }


    [Fact]
    public async Task Add_transaction_should_return_success()
    {
        var message = "Thanks for a great evening";
        var paymentIntent = new PaymentIntent()
        {
            Id = "5",
            Amount = 125,
            Currency = "usd",
            Metadata = new Dictionary<string, string>
            {
                { "MemberId", "1" },
            },
            Status = "required_payment_method"
        };

        var (_, isFailure) = await _service.Add(paymentIntent, message);
        var createdTransaction = await _aetherDbContext.Transactions
            .SingleOrDefaultAsync(t => t.PaymentIntentId == "5");

        Assert.False(isFailure);
        Assert.NotNull(createdTransaction);
        Assert.Equal(message, createdTransaction!.Message);
    }


    [Fact]
    public async Task Update_transaction_should_return_success()
    {
        var message = "Thanks for a great evening";
        var paymentIntent = new PaymentIntent { Id = "1", Amount = 1999, Currency = "usd" };

        var (_, isFailure) = await _service.Update(paymentIntent, message);
        var updatedTransaction = await _aetherDbContext.Transactions
            .SingleOrDefaultAsync(t => t.PaymentIntentId == "1");

        Assert.False(isFailure);
        Assert.NotNull(updatedTransaction);
        Assert.Equal((decimal)19.99, updatedTransaction!.Amount);
    }


    [Fact]
    public async Task Get_default_size_of_transactions_should_return_success()
    {
        const int accountId = 2;
        const int skipLast = 0;
        const int topLast = 20;
        const TransactionFilterProperty property = TransactionFilterProperty.Created;
        const SortVariant variant = SortVariant.ASC;
        var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
        var transactionSortingMock = new Mock<ITransactionSorting>();
        transactionSortingMock.Setup(s => s.ByCreatedASC(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<TransactionFilterProperty>(), It.IsAny<SortVariant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TransactionResponse>());

        TransactionService service = new TransactionService(_aetherDbContext, transactionSortingMock.Object);

        var (_, isFailure, transactionList) = await service.Get(memberContext, skipLast, topLast, property, variant);

        Assert.False(isFailure);
    }


    [Fact]
    public async Task Get_transactions_should_return_success()
    {
        const int accountId = 2;
        const int skip = 2;
        const int top = 2;
        const TransactionFilterProperty property = TransactionFilterProperty.Amount;
        const SortVariant variant = SortVariant.DESC;
        var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
        var transactionSortingMock = new Mock<ITransactionSorting>();
        transactionSortingMock.Setup(s => s.ByAmountDESC(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<TransactionFilterProperty>(), It.IsAny<SortVariant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TransactionResponse>());

        TransactionService service = new TransactionService(_aetherDbContext, transactionSortingMock.Object);


        var (_, isFailure, transactionList) = await service.Get(memberContext, skip, top, property, variant);

        Assert.False(isFailure);
    }


    // [Fact]
    // public async Task Get_transactions_should_return_error_when_skip_not_valid()
    // {
    //     const int accountId = 2;
    //     const int skip = -1;
    //     const int top = 2;
    //     var memberContext = new MemberContext(1, "hash", accountId, string.Empty);

    //     var (_, isFailure) = await _service.Get(memberContext, skip, top);

    //     Assert.True(isFailure);
    // }


    // [Fact]
    // public async Task Get_transactions_should_return_error_when_top_is_negative()
    // {
    //     const int accountId = 2;
    //     const int skip = 2;
    //     const int top = -1;
    //     var memberContext = new MemberContext(1, "hash", accountId, string.Empty);

    //     var (_, isFailure) = await _service.Get(memberContext, skip, top);

    //     Assert.True(isFailure);
    // }


    // [Fact]
    // public async Task Get_transactions_should_return_error_when_top_greater_than_restriction()
    // {
    //     const int accountId = 2;
    //     const int skip = 2;
    //     const int top = 101;
    //     var memberContext = new MemberContext(1, "hash", accountId, string.Empty);

    //     var (_, isFailure) = await _service.Get(memberContext, skip, top);

    //     Assert.True(isFailure);
    // }


    private readonly IEnumerable<Transaction> _transactions = new[]
    {
        new Transaction
        {
            Id = 1,
            Amount = 25,
            Currency = "usd",
            MemberId = 1,
            PaymentIntentId = "1",
            State = "succeeded"
        },
        new Transaction
        {
            Id = 2,
            Amount = 50,
            Currency = "usd",
            MemberId = 1,
            PaymentIntentId = "2",
            State = "succeeded"
        },
        new Transaction
        {
            Id = 3,
            Amount = 75,
            Currency = "usd",
            MemberId = 1,
            PaymentIntentId = "3",
            State = "succeeded"
        },
        new Transaction
        {
            Id = 4,
            Amount = 100,
            Currency = "usd",
            MemberId = 1,
            PaymentIntentId = "4",
            State = "succeeded"
        }
    };


    private readonly IEnumerable<Member> _members = new[]
    {
        new Member
        {
            Id = 1
        }
    };


    private readonly IEnumerable<StripeAccount> _stripeAccounts = new[]
    {
        new StripeAccount
        {
            Id = 1,
            StripeId = "acc_1",
            MemberId = 1
        }
    };


    private readonly AetherDbContext _aetherDbContext;
    private readonly TransactionService _service;
}