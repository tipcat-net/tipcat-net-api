using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.ApiTests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace TipCatDotNet.ApiTests
{
    public class FacilityServiceTests
    {
        public FacilityServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts));
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));
            aetherDbContextMock.Setup(c => c.Facilities).Returns(DbSetMockProvider.GetDbSetMock(_facilities));

            _aetherDbContext = aetherDbContextMock.Object;
        }


        [Fact]
        public async Task Add_should_return_error_when_current_member_does_not_belongs_to_target_account()
        {
            var memberContext = new MemberContext(1, string.Empty, 1, null);
            var facilityRequest = new FacilityRequest(null, "Test facility", 2);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure) = await service.Add(memberContext, facilityRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_add_facility()
        {
            const string facilityName = "Test facility 2";
            const int facilityAccountId = 1;
            var memberContext = new MemberContext(1, string.Empty, facilityAccountId, null);
            var request = new FacilityRequest(null, facilityName, facilityAccountId);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure, response) = await service.Add(memberContext, request, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
            Assert.Equal(response.Name, request.Name);
            Assert.Equal(response.AccountId, request.AccountId);
        }


        [Fact]
        public async Task Get_should_return_error_when_current_member_does_not_belongs_to_target_account()
        {
            const int facilityId = 2;
            const int accountId = 2;
            var memberContext = new MemberContext(1, string.Empty, 1, null);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure) = await service.Get(memberContext, facilityId, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_return_error_when_current_facility_does_not_belongs_to_target_account()
        {
            const int facilityId = 2;
            const int accountId = 1;
            var memberContext = new MemberContext(1, string.Empty, 1, null);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure) = await service.Get(memberContext, facilityId, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_get_facility()
        {
            const int facilityId = 1;
            const int accountId = 1;
            var memberContext = new MemberContext(1, string.Empty, 1, null);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure, response) = await service.Get(memberContext, facilityId, accountId);

            Assert.False(isFailure);
            Assert.Equal(response.Id, 1);
            Assert.Equal(response.Name, "Default facility");
        }


        [Fact]
        public async Task Get_all_should_return_error_when_current_member_not_belong_to_target_account()
        {
            const int facilityAccountId = 2;
            var context = new MemberContext(1, string.Empty, 1, null);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure) = await service.Get(context, facilityAccountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_all_should_return_all_account_facilities()
        {
            const int accountId = 1;
            var facilitiesCount = _facilities
                .Count(m => m.AccountId == accountId);

            var context = new MemberContext(1, string.Empty, accountId, null);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, _, slimFacilities) = await service.Get(context, accountId);

            Assert.Equal(facilitiesCount, slimFacilities.Count);
        }


        [Fact]
        public async Task Update_should_return_error_when_current_member_does_not_belongs_to_target_account()
        {
            const int facilityId = 2;
            const string facilityName = "Test facility 2";
            const int facilityAccountId = 2;
            var memberContext = new MemberContext(1, string.Empty, 1, null);
            var request = new FacilityRequest(facilityId, facilityName, facilityAccountId);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure) = await service.Update(memberContext, request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_return_error_when_current_facility_does_not_belongs_to_target_account()
        {
            const int facilityId = 1;
            const string facilityName = "Test facility";
            const int facilityAccountId = 2;
            var memberContext = new MemberContext(1, string.Empty, 1, null);
            var request = new FacilityRequest(facilityId, facilityName, facilityAccountId);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure) = await service.Update(memberContext, request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_update_facility()
        {
            const int facilityId = 1;
            const string facilityName = "Test facility";
            const int accountId = 1;
            var memberContext = new MemberContext(1, string.Empty, accountId, null);
            var request = new FacilityRequest(facilityId, facilityName, accountId);
            var service = new FacilityService(new NullLoggerFactory(), _aetherDbContext);

            var (_, isFailure, response) = await service.Update(memberContext, request);

            Assert.False(isFailure);
            Assert.Equal(response.Id, request.Id);
            Assert.Equal(response.Name, request.Name);
            Assert.Equal(response.AccountId, request.AccountId);
        }


        private readonly IEnumerable<Account> _accounts = new[]
        {
            new Account
            {
                Id = 1,
                State = ModelStates.Inactive
            },
            new Account
            {
                Id = 2,
                State = ModelStates.Active
            }
        };

        private readonly IEnumerable<Facility> _facilities = new[]
        {
            new Facility
            {
                Id = 1,
                Name = "Default facility",
                AccountId = 1
            },
            new Facility
            {
                Id = 2,
                Name = "Default facility",
                AccountId = 2
            },
            new Facility
            {
                Id = 3,
                Name = "Test facility",
                AccountId = 1
            },
        };

        private readonly IEnumerable<Member> _members = new[]
        {
            new Member
            {
                Id = 1,
                AccountId = 1
            }
        };

        private readonly AetherDbContext _aetherDbContext;
    }
}