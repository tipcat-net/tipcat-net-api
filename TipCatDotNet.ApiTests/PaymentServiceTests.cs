using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Models;
using HappyTravel.Money.Enums;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Services.Payments;
using TipCatDotNet.ApiTests.Utils;
using Microsoft.Extensions.Options;
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

            // There are credentials from test account
            _paymentSettings = Options.Create<PaymentSettings>(new PaymentSettings()
            {
                StripePublicKey = "pk_test_51JpquPKOuc4NSEDLcgySChDPhZ68l3WZ3i4haqHwwGQZdVNJlsGRnqPxcg27MXJgZZShmvk425FTsQVWDQYQ3kPa00qVRu3Vv8",
                StripePrivateKey = "sk_test_51JpquPKOuc4NSEDLRFBydynBYh121cuxofTTSy7wxgxwUY7DIb2iSByklCsW7MTywjbfw4vG5I1xP7E3GrNFdi4700A4fiGylV"
            });
        }


        [Fact]
        public async Task GetDetails_should_return_success()
        {
            var memberCode = "6СD63FG42ASD";
            var service = new PaymentService(_paymentSettings, _aetherDbContext);

            var (_, isFailure, paymentDetails) = await service.GetMemberDetails(memberCode);

            Assert.False(isFailure);
            Assert.Equal(1, paymentDetails.Member.Id);
            Assert.Equal("Elizabeth", paymentDetails.Member.FirstName);
            Assert.Equal("Omara", paymentDetails.Member.LastName);
        }


        [Fact]
        public async Task GetDetails_should_return_error_when_member_was_not_found()
        {
            var memberCode = "5СD63FG42ASD";
            var service = new PaymentService(_paymentSettings, _aetherDbContext);

            var (_, isFailure) = await service.GetMemberDetails(memberCode);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_return_success()
        {
            var paymentIntentId = "pi_3JqtSnKOuc4NSEDL0cP5wDvQ"; // Test payment intent created by stripe dashboard
            var service = new PaymentService(_paymentSettings, _aetherDbContext);

            var (_, isFailure, paymentDetails) = await service.Get(paymentIntentId);

            Assert.False(isFailure);
            Assert.Equal(1, paymentDetails.Member.Id);
            Assert.Equal("Elizabeth", paymentDetails.Member.FirstName);
            Assert.Equal("Omara", paymentDetails.Member.LastName);
        }


        [Fact]
        public async Task Get_should_return_error_when_member_was_not_found()
        {
            var paymentIntentId = "pi_3Jpr1BKOuc4NSEDL0374nYsU"; // Test payment intent created by stripe dashboard
            var service = new PaymentService(_paymentSettings, _aetherDbContext);

            var (_, isFailure) = await service.Get(paymentIntentId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Pay_should_return_error_when_member_does_not_exist()
        {
            var request = new PaymentRequest(101, "card" ,new MoneyAmount(10, Currencies.USD));
            var service = new PaymentService(_paymentSettings, _aetherDbContext);

            var (_, isFailure) = await service.Pay(request);

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
                Email = null,
                MemberCode = "6СD63FG42ASD",
                Permissions = MemberPermissions.Manager
            },
            new Member
            {
                Id = 7,
                IdentityHash = "e6b02f80930f7e255776dbc8934a7eced41ea1db65f845a00d9442adf846f2dd",
                FirstName = "Ian",
                LastName = "Moss",
                Email = null,
                MemberCode = "7СD63FG42ASD",
                Permissions = MemberPermissions.Manager
            },
            new Member
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
            new Member
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


        private readonly IEnumerable<Account> _accounts = Array.Empty<Account>();

        private readonly AetherDbContext _aetherDbContext;

        private readonly IOptions<PaymentSettings> _paymentSettings;
    }
}