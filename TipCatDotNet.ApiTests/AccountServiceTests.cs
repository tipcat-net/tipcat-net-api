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
using TipCatDotNet.Api.Services;
using Xunit;

namespace TipCatDotNet.ApiTests
{
    public class AccountServiceTests
    {
        public AccountServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts));
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));
            aetherDbContextMock.Setup(c => c.Facilities).Returns(DbSetMockProvider.GetDbSetMock(_facilities));

            _aetherDbContext = aetherDbContextMock.Object;

            var memberContextCacheServiceMock = new Mock<IMemberContextCacheService>();
            _memberContextCacheService = memberContextCacheServiceMock.Object;

            var facilityServiceMock = new Mock<IFacilityService>();
            facilityServiceMock.Setup(c => c.Get(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FacilityResponse>());

            _facilityService = facilityServiceMock.Object;
        }


        [Fact]
        public async Task Add_should_not_add_account_when_member_has_one()
        {
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Add(new MemberContext(1, "hash", 1, string.Empty), new AccountRequest());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_name_is_not_specified()
        {
            var accountRequest = new AccountRequest(null, string.Empty, string.Empty);
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_address_is_not_specified()
        {
            var accountRequest = new AccountRequest(null, string.Empty, "Tipcat.net");
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_phone_and_email_are_not_specified()
        {
            var accountRequest = new AccountRequest(null, "Dubai, Saraya Avenue Building, B2, 205", "Tipcat.net");
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_add_account_when_phone_is_specified()
        {
            var request = new AccountRequest(null, "Dubai, Saraya Avenue Building, B2, 205", "Tipcat.net", null, null, "+8 (800) 2000 500");
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure, response) = await service.Add(memberContext, request);

            Assert.False(isFailure);
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.Address, response.Address);
            Assert.Equal(string.Empty, response.Email);
            Assert.Equal(request.Phone, response.Phone);
            Assert.Equal(request.Name, response.OperatingName);
        }


        [Fact]
        public async Task Add_should_add_account_when_email_is_specified()
        {
            var request = new AccountRequest(null, "Dubai, Saraya Avenue Building, B2, 205", "Tipcat.net", email: "kirill.taran@tipcat.net");
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure, response) = await service.Add(memberContext, request);

            Assert.False(isFailure);
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.Address, response.Address);
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(string.Empty, response.Phone);
            Assert.Equal(request.Name, response.OperatingName);
        }


        [Fact]
        public async Task Add_should_add_account()
        {
            var request = new AccountRequest(null, "Dubai, Saraya Avenue Building, B2, 205", "Tipcat.net", null, null, "+8 (800) 2000 500");
            var memberContext = new MemberContext(1, "hash", null, "kirill.taran@tipcat.net");
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure, response) = await service.Add(memberContext, request);

            Assert.False(isFailure);            
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.Address, response.Address);
            Assert.Equal(memberContext.Email, response.Email);
            Assert.Equal(request.Phone, response.Phone);
            Assert.Equal(request.Name, response.OperatingName);
        }


        [Fact]
        public async Task Add_should_create_default_facility()
        {
            const string expectedFacilityName = "Default facility";
            var request = new AccountRequest(null, "Dubai, Saraya Avenue Building, B2, 205", "Tipcat.net", null, null, "+8 (800) 2000 500");
            var memberContext = new MemberContext(1, "hash", null, "kirill.taran@tipcat.net");
            var facilityServiceMock = new Mock<IFacilityService>();
            facilityServiceMock.Setup(c => c.Get(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Func<int, CancellationToken, List<FacilityResponse>>((accountId, _) =>
                {
                    _aetherDbContext.Facilities.Add(new Facility
                    {
                        AccountId = accountId,
                        Name = expectedFacilityName
                    });
                    _aetherDbContext.SaveChanges();

                    return new List<FacilityResponse>();
                }));
            
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, facilityServiceMock.Object);

            var (_, isFailure, response) = await service.Add(memberContext, request);
            var defaultFacility = await _aetherDbContext.Facilities
                .SingleAsync(f => f.AccountId == response.Id, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
            Assert.True(defaultFacility is not null);
            Assert.Equal(expectedFacilityName, defaultFacility!.Name);
        }


        [Fact]
        public async Task Get_should_not_get_account_when_context_has_no_account_ids()
        {
            const int accountId = 1;
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_not_get_account_when_member_has_no_access_to_account()
        {
            const int accountId = 1;
            var memberContext = new MemberContext(1, "hash", 0, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_not_get_account_when_account_deactivated()
        {
            const int accountId = 1;
            var memberContext = new MemberContext(1, "hash", 1, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_get_account()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, _, accountInfo) = await service.Get(memberContext, accountId);

            Assert.Equal(accountInfo.Id, accountId);
            Assert.All(accountInfo.Facilities, facility =>
            {
                Assert.Equal(accountId, facility.AccountId);
                var memberCount = _members
                .Count(m => m.FacilityId == facility.Id);
                Assert.Equal(memberCount, facility.Members.Count);
            });
        }


        [Fact]
        public async Task Update_should_not_update_if_account_request_has_no_id()
        {
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Update(memberContext, new AccountRequest());

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData(3)]
        [InlineData(null)]
        public async Task Update_should_not_update_if_request_id_and_context_id_do_not_match(int? accountId)
        {
            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
            var accountRequest = new AccountRequest(2, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Update(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_not_update_if_account_name_is_empty()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
            var accountRequest = new AccountRequest(accountId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Update(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_not_update_if_account_address_is_empty()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
            var accountRequest = new AccountRequest(accountId, string.Empty, "Tipcat.net", string.Empty, string.Empty, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Update(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_not_update_if_account_phone_is_empty()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
            var accountRequest = new AccountRequest(accountId, "Dubai, Saraya Avenue Building, B2, 205", "Tipcat.net", string.Empty, string.Empty, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, isFailure) = await service.Update(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_update_account()
        {
            const int accountId = 2;
            const string address = "Dubai, Saraya Avenue Building, B2, 205";
            const string name = "Tipcat.net";
            const string phone = "+8 (800) 2000 500";

            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
            var accountRequest = new AccountRequest(accountId, address, name, string.Empty, string.Empty, phone);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService, _facilityService);

            var (_, _, account) = await service.Update(memberContext, accountRequest);

            Assert.Equal(account.Address, address);
            Assert.Equal(account.Name, name);
            Assert.Equal(account.Phone, phone);
            Assert.All(account.Facilities, facility =>
            {
                Assert.Equal(accountId, facility.AccountId);
                var memberCount = _members
                .Count(m => m.FacilityId == facility.Id);
                Assert.Equal(memberCount, facility.Members.Count);
            });
        }


        private readonly IEnumerable<Account> _accounts = new[]
        {
            new Account
            {
                Id = 1,
                IsActive = false
            },
            new Account
            {
                Id = 2,
                IsActive = true
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
        private readonly IMemberContextCacheService _memberContextCacheService;
        private readonly IFacilityService _facilityService;
    }
}
