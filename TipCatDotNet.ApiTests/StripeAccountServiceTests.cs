using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Data.Models.Stripe;
using TipCatDotNet.Api.Data;
using TipCatDotNet.ApiTests.Utils;
using TipCatDotNet.Api.Models.Auth;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.Api.Services.Images;
using TipCatDotNet.Api.Options;
using Xunit;
using Stripe;

namespace TipCatDotNet.ApiTests
{
    public class StripeAccountServiceTests
    {
        public StripeAccountServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));
            aetherDbContextMock.Setup(c => c.StripeAccounts).Returns(DbSetMockProvider.GetDbSetMock(_stripeAccounts));

            _aetherDbContext = aetherDbContextMock.Object;

            var stripeAccountServiceMock = new Mock<Stripe.AccountService>();
            stripeAccountServiceMock.Setup(s => s.CreateAsync(It.IsAny<AccountCreateOptions>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Stripe.Account());
            stripeAccountServiceMock.Setup(s => s.UpdateAsync(It.IsAny<string>(), It.IsAny<AccountUpdateOptions>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Stripe.Account());
            stripeAccountServiceMock.Setup(s => s.GetAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Stripe.Account());
            stripeAccountServiceMock.Setup(s => s.DeleteAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Stripe.Account());

            _stripeAccountService = stripeAccountServiceMock.Object;
        }


        [Fact]
        public async Task Add_should_return_success()
        {
            const int accountId = 5;
            const string firstName = "Elizabeth";
            const string lastName = "Omara";
            var memberRequest = new MemberRequest(1, accountId, firstName, lastName, null, MemberPermissions.Manager);
            var service = new StripeAccountService(_aetherDbContext, _stripeAccountService, It.IsAny<IOptions<StripeOptions>>());

            var (_, isFailure) = await service.Add(memberRequest, It.IsAny<CancellationToken>());
            var isRelatedAccountCreate = await _aetherDbContext.StripeAccounts
                .AnyAsync(s => s.MemberId == 1);

            Assert.False(isFailure);
            Assert.True(isRelatedAccountCreate);
        }



        [Fact]
        public async Task Update_should_return_success()
        {
            const int accountId = 5;
            const string firstName = "Anna";
            const string lastName = "Omara";
            var memberRequest = new MemberRequest(2, accountId, firstName, lastName, null, MemberPermissions.Manager);
            var service = new StripeAccountService(_aetherDbContext, _stripeAccountService, It.IsAny<IOptions<StripeOptions>>());

            var (_, isFailure) = await service.Update(memberRequest, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Update_should_return_error_when_has_no_any_related_accounts()
        {
            const int accountId = 5;
            const string firstName = "Elizabeth";
            const string lastName = "Omara";
            var memberRequest = new MemberRequest(1, accountId, firstName, lastName, null, MemberPermissions.Manager);
            var service = new StripeAccountService(_aetherDbContext, _stripeAccountService, It.IsAny<IOptions<StripeOptions>>());

            var (_, isFailure) = await service.Update(memberRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_return_success()
        {
            var memberId = 2;
            var service = new StripeAccountService(_aetherDbContext, _stripeAccountService, It.IsAny<IOptions<StripeOptions>>());

            var (_, isFailure) = await service.Remove(memberId, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Remove_should_return_error_when_has_no_any_related_accounts()
        {
            var memberId = 1;
            var service = new StripeAccountService(_aetherDbContext, _stripeAccountService, It.IsAny<IOptions<StripeOptions>>());

            var (_, isFailure) = await service.Remove(memberId, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        private readonly IEnumerable<Member> _members = new[]
        {
            new Member
            {
                Id = 1,
                IdentityHash = "hash",
                FirstName = "Elizabeth",
                LastName = "Omara",
                AccountId = 5,
                Email = null,
                FacilityId = 1,
                Permissions = MemberPermissions.Manager
            },
            new Member
            {
                Id = 2,
                IdentityHash = "hash",
                FirstName = "Anna",
                LastName = "Omara",
                AccountId = 5,
                FacilityId = 1,
                Email = null,
                Permissions = MemberPermissions.Employee
            },
            new Member
            {
                Id = 7,
                IdentityHash = "e6b02f80930f7e255776dbc8934a7eced41ea1db65f845a00d9442adf846f2dd",
                FirstName = "Ian",
                LastName = "Moss",
                Email = null,
                Permissions = MemberPermissions.Manager
            }
        };


        private readonly IEnumerable<StripeAccount> _stripeAccounts = new[]
        {
            new StripeAccount
            {
                StripeId = "acct_1K1pnLPFSaYTKHxh",
                MemberId = 2
            },
            new StripeAccount
            {
                StripeId = "acc_7",
                MemberId = 7
            }
        };


        private readonly AetherDbContext _aetherDbContext;

        private readonly Stripe.AccountService _stripeAccountService;
    }
}