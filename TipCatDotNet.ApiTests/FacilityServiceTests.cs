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
            var memberServiceMock = new Mock<IMemberService>();
            
            var facilityRequest = new FacilityRequest(null, "Test facility", 2);
            var service = new FacilityService(_aetherDbContext, memberServiceMock.Object);

            var (_, isFailure) = await service.Add(memberContext, facilityRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_add_facility()
        {
            const string facilityName = "Test facility 2";
            const int facilityAccountId = 1;
            var memberContext = new MemberContext(1, string.Empty, facilityAccountId, null);
            var memberServiceMock = new Mock<IMemberService>();
            
            var request = new FacilityRequest(null, facilityName, facilityAccountId);
            var service = new FacilityService(_aetherDbContext, memberServiceMock.Object);

            var (_, isFailure, response) = await service.Add(memberContext, request, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.AccountId, response.AccountId);
        }


        [Fact]
        public async Task Get_all_should_return_all_account_facilities()
        {
            const int accountId = 1;
            var facilitiesCount = _facilities
                .Count(m => m.AccountId == accountId);

            var memberServiceMock = new Mock<IMemberService>();
            
            var service = new FacilityService(_aetherDbContext, memberServiceMock.Object);

            var facilities = await service.Get(accountId);

            Assert.Equal(facilitiesCount, facilities.Count);
            Assert.All(facilities, facility =>
            {
                Assert.Equal(accountId, facility.AccountId);
                var memberCount = _members
                .Count(m => m.FacilityId == facility.Id);
                Assert.Equal(memberCount, facility.Members.ToList().Count);
            });
        }


        [Fact]
        public async Task Update_should_return_error_when_current_member_does_not_belongs_to_target_account()
        {
            const int facilityId = 2;
            const string facilityName = "Test facility 2";
            const int facilityAccountId = 2;
            var memberContext = new MemberContext(1, string.Empty, 1, null);
            var memberServiceMock = new Mock<IMemberService>();
            
            var request = new FacilityRequest(facilityId, facilityName, facilityAccountId);
            var service = new FacilityService(_aetherDbContext, memberServiceMock.Object);

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
            var memberServiceMock = new Mock<IMemberService>();
            
            var request = new FacilityRequest(facilityId, facilityName, facilityAccountId);
            var service = new FacilityService(_aetherDbContext, memberServiceMock.Object);

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
            var memberServiceMock = new Mock<IMemberService>();
            
            var request = new FacilityRequest(facilityId, facilityName, accountId);
            var service = new FacilityService(_aetherDbContext, memberServiceMock.Object);

            var (_, isFailure, response) = await service.Update(memberContext, request);

            Assert.False(isFailure);
            Assert.Equal(request.Id, response.Id);
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.AccountId, response.AccountId);
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
                AccountId = 1,
                FacilityId = 1
            }
        };

        private readonly AetherDbContext _aetherDbContext;
    }
}