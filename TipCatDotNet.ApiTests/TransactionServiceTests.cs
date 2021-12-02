using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Services.Payments;
using TipCatDotNet.ApiTests.Utils;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Xunit;

namespace TipCatDotNet.ApiTests
{
    public class TransactionServiceTests
    {
        public TransactionServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));
            aetherDbContextMock.Setup(c => c.Transactions).Returns(DbSetMockProvider.GetDbSetMock(_transactions));

            _aetherDbContext = aetherDbContextMock.Object;

            _service = new TransactionService(_aetherDbContext);
        }


        [Fact]
        public async Task Add_transaction_should_return_success()
        {
            var message = "Thanks for a great evening";
            var paymentIntent = new PaymentIntent()
            {
                Id = "5",
                Description = message,
                Amount = 125,
                Currency = "usd",
                Metadata = new Dictionary<string, string>
                {
                    { "MemberId", "1" },
                },
                Status = "required_payment_method"
            };

            var (_, isFailure) = await _service.Add(paymentIntent);
            var createdTransaction = await _aetherDbContext.Transactions
                    .SingleOrDefaultAsync(t => t.PaymentIntentId == "5");

            Assert.False(isFailure);
            Assert.True(createdTransaction != null);
            Assert.Equal(message, createdTransaction.Message);
        }


        [Fact]
        public async Task Update_transaction_should_return_success()
        {
            var paymentIntent = new PaymentIntent() { Id = "1", Amount = 1999, Currency = "usd" };

            var (_, isFailure) = await _service.Update(paymentIntent);
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
            const int topLast = 10;
            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);

            var (_, isFailure, transactionList) = await _service.Get(memberContext, skipLast, topLast);

            Assert.False(isFailure);
            Assert.Equal(4, transactionList.Count);
        }


        [Fact]
        public async Task Get_transactions_should_return_success()
        {
            const int accountId = 2;
            const int skip = 2;
            const int top = 2;
            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);

            var (_, isFailure, transactionList) = await _service.Get(memberContext, skip, top);

            Assert.False(isFailure);
            Assert.Equal(2, transactionList.Count);
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


        private readonly AetherDbContext _aetherDbContext;
        private readonly TransactionService _service;
    }
}