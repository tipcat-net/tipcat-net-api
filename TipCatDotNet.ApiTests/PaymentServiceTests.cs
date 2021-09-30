using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Graph;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;
using TipCatDotNet.Api.Services.Graph;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.ApiTests.Utils;
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
        }


        [Fact]
        public async Task GetReceiver_should_return_success()
        {
            var memberCode = "6СD63FG42ASD";
            var service = new PaymentService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure, receiver) = await service.GetReceiver(memberCode);

            Assert.False(isFailure);
            Assert.Equal(memberCode, receiver.MemberCode);
        }


        [Fact]
        public async Task GetReceiver_should_return_error_when_receiver_was_not_found()
        {
            var memberCode = "5СD63FG42ASD";
            var service = new PaymentService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure) = await service.GetReceiver(memberCode);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Pay_should_return_error_when_receiver_doesnt_exist()
        {
            var request = new PaymentRequest("5СD63FG42ASD");
            var service = new PaymentService(new NullLoggerFactory(), _aetherDbContext);

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
    }
}