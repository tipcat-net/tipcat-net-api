using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Models;
using HappyTravel.Money.Enums;
using TipCatDotNet.Api.Data;
using TipcatModels = TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Services.Payments;
using TipCatDotNet.ApiTests.Utils;
using TipCatDotNet.Api.Filters.Pagination;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using Xunit;

namespace TipCatDotNet.ApiTests
{
    public class PaymentServiceTests
    {
        public PaymentServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));
            aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts));

            _aetherDbContext = aetherDbContextMock.Object;


            var paymentIntentServiceMock = new Mock<PaymentIntentService>();
            paymentIntentServiceMock.Setup(c => c.CreateAsync(It.IsAny<PaymentIntentCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentIntent()
                {
                    Object = "payment_intent",
                    Metadata = new Dictionary<string, string>
                    {
                        { "MemberId", "100" },
                    },
                });
            paymentIntentServiceMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<PaymentIntentGetOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentIntent()
                {
                    Object = "payment_intent",
                    Metadata = new Dictionary<string, string>
                    {
                        { "MemberId", "1" },
                    },
                });
            paymentIntentServiceMock.Setup(c => c.CaptureAsync(It.IsAny<string>(), It.IsAny<PaymentIntentCaptureOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentIntent()
                {
                    // TODO : details of captured paymentIntent
                });

            _paymentIntentService = paymentIntentServiceMock.Object;


            var transactionServiceMock = new Mock<ITransactionService>();
            transactionServiceMock.Setup(c => c.Add(It.IsAny<PaymentIntent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            transactionServiceMock.Setup(c => c.Get(It.IsAny<MemberContext>(), It.IsAny<PaginationFilter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionResponse>());
            transactionServiceMock.Setup(c => c.Update(It.IsAny<PaymentIntent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _transactionService = transactionServiceMock.Object;
        }


        [Fact]
        public async Task GetDetails_should_return_success()
        {
            var memberCode = "6СD63FG42ASD";
            var service = new PaymentService(_transactionService, It.IsAny<IOptions<StripeOptions>>(), _paymentIntentService, _aetherDbContext);

            var (_, isFailure, paymentDetails) = await service.GetPreparationDetails(memberCode);

            Assert.False(isFailure);
            Assert.Equal(1, paymentDetails.Member.Id);
            Assert.Equal("Elizabeth", paymentDetails.Member.FirstName);
            Assert.Equal("Omara", paymentDetails.Member.LastName);
        }


        [Fact]
        public async Task GetDetails_should_return_error_when_member_was_not_found()
        {
            var memberCode = "5СD63FG42ASD";
            var service = new PaymentService(_transactionService, It.IsAny<IOptions<StripeOptions>>(), _paymentIntentService, _aetherDbContext);

            var (_, isFailure) = await service.GetPreparationDetails(memberCode);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_return_success()
        {
            var paymentIntentId = "pi_3JqtSnKOuc4NSEDL0cP5wDvQ";
            var service = new PaymentService(_transactionService, It.IsAny<IOptions<StripeOptions>>(), _paymentIntentService, _aetherDbContext);

            var (_, isFailure, paymentDetails) = await service.Get(paymentIntentId);

            Assert.False(isFailure);
            Assert.Equal(1, paymentDetails.Member.Id);
            Assert.Equal("Elizabeth", paymentDetails.Member.FirstName);
            Assert.Equal("Omara", paymentDetails.Member.LastName);
        }


        [Fact]
        public async Task Get_should_return_error_when_member_was_not_found()
        {
            var paymentIntentId = "pi_3JqtSnKOuc4NSEDL0cP5wDvQ";
            var paymentIntentServiceMock = new Mock<PaymentIntentService>();
            paymentIntentServiceMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<PaymentIntentGetOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentIntent()
                {
                    Object = "payment_intent",
                    Metadata = new Dictionary<string, string>
                    {
                        { "MemberId", "100" },
                    },
                });

            var paymentIntentService = paymentIntentServiceMock.Object;
            var service = new PaymentService(_transactionService, It.IsAny<IOptions<StripeOptions>>(), paymentIntentService, _aetherDbContext);

            var (_, isFailure) = await service.Get(paymentIntentId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Pay_should_return_error_when_member_does_not_exist()
        {
            var request = new PaymentRequest(101, new MoneyAmount(10, Currencies.USD));
            var service = new PaymentService(_transactionService, It.IsAny<IOptions<StripeOptions>>(), _paymentIntentService, _aetherDbContext);

            var (_, isFailure) = await service.Pay(request);

            Assert.True(isFailure);
        }


        private readonly IEnumerable<TipcatModels.Member> _members = new[]
        {
            new TipcatModels.Member
            {
                Id = 1,
                IdentityHash = "hash",
                FirstName = "Elizabeth",
                LastName = "Omara",
                Email = null,
                MemberCode = "6СD63FG42ASD",
                Permissions = MemberPermissions.Manager
            },
            new TipcatModels.Member
            {
                Id = 7,
                IdentityHash = "e6b02f80930f7e255776dbc8934a7eced41ea1db65f845a00d9442adf846f2dd",
                FirstName = "Ian",
                LastName = "Moss",
                Email = null,
                MemberCode = "7СD63FG42ASD",
                Permissions = MemberPermissions.Manager
            },
            new TipcatModels.Member
            {
                Id = 89,
                AccountId = 9,
                IdentityHash = "hash",
                FirstName = "Zachary",
                LastName = "White",
                Email = null,
                MemberCode = "8СD63FG42ASD",
                Permissions = MemberPermissions.Manager
            },
            new TipcatModels.Member
            {
                Id = 90,
                AccountId = 9,
                IdentityHash = "hash",
                FirstName = "Zachary",
                LastName = "White",
                Email = null,
                MemberCode = "9СD63FG42ASD",
                Permissions = MemberPermissions.Employee
            }
        };


        private readonly IEnumerable<TipcatModels.Account> _accounts = Array.Empty<TipcatModels.Account>();

        private readonly AetherDbContext _aetherDbContext;

        private readonly PaymentIntentService _paymentIntentService;

        private readonly ITransactionService _transactionService;
    }
}