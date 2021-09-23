using System.Collections.Generic;
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
    public class AccountServiceTests
    {
        public AccountServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts));
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));

            _aetherDbContext = aetherDbContextMock.Object;

            var memberContextCacheServiceMock = new Mock<IMemberContextCacheService>();
            _memberContextCacheService = memberContextCacheServiceMock.Object;
        }


        [Fact]
        public async Task Add_should_not_add_account_when_member_has_one()
        {
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Add(new MemberContext(1, "hash", 1, string.Empty), new AccountRequest());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_name_is_not_specified()
        {
            var accountRequest = new AccountRequest(null, string.Empty, null, null, string.Empty, string.Empty);
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_address_is_not_specified()
        {
            var accountRequest = new AccountRequest(null, string.Empty, null, null, "Tipcat.net", string.Empty);
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_phone_is_not_specified()
        {
            var accountRequest = new AccountRequest(null, "Dubai, Saraya Avenue Building, B2, 205", null, null, "Tipcat.net", string.Empty);
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_add_account()
        {
            var request = new AccountRequest(null, "Dubai, Saraya Avenue Building, B2, 205", null, null, "Tipcat.net", "+8 (800) 2000 500");
            var memberContext = new MemberContext(1, "hash", null, "kirill.taran@tipcat.net");
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, _, response) = await service.Add(memberContext, request);

            Assert.Equal(response.Name, request.Name);
            Assert.Equal(response.Address, request.Address);
            Assert.Equal(response.Email, memberContext.Email);
            Assert.Equal(response.Phone, request.Phone);
            Assert.Equal(response.CommercialName, request.Name);
        }


        [Fact]
        public async Task Get_should_not_get_account_when_context_has_no_account_ids()
        {
            const int accountId = 1;
            var memberContext = new MemberContext(1, "hash", null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_not_get_account_when_member_has_no_access_to_account()
        {
            const int accountId = 1;
            var memberContext = new MemberContext(1, "hash", 0, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_not_get_account_when_account_deactivated()
        {
            const int accountId = 1;
            var memberContext = new MemberContext(1, "hash", 1, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_get_account()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1,"hash",  accountId, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, _, accountInfo) = await service.Get(memberContext, accountId);

            Assert.Equal(accountInfo.Id, accountId);
        }


        [Fact]
        public async Task Update_should_not_update_if_account_request_has_no_id()
        {
            var memberContext = new MemberContext(1,"hash",  null, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Update(memberContext, new AccountRequest());

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData(3)]
        [InlineData(null)]
        public async Task Update_should_not_update_if_request_id_and_context_id_do_not_match(int? accountId)
        {
            var memberContext = new MemberContext(1,"hash",  accountId, string.Empty);
            var accountRequest = new AccountRequest(2, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Update(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_not_update_if_account_name_is_empty()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1,"hash",  accountId, string.Empty);
            var accountRequest = new AccountRequest(accountId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Update(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_not_update_if_account_address_is_empty()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1,"hash",  accountId, string.Empty);
            var accountRequest = new AccountRequest(accountId, string.Empty, string.Empty, string.Empty, "Tipcat.net", string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, isFailure) = await service.Update(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_not_update_if_account_phone_is_empty()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1, "hash", accountId, string.Empty);
            var accountRequest = new AccountRequest(accountId, "Dubai, Saraya Avenue Building, B2, 205", string.Empty, string.Empty, "Tipcat.net", string.Empty);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

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

            var memberContext = new MemberContext(1,"hash",  accountId, string.Empty);
            var accountRequest = new AccountRequest(accountId, address, string.Empty, string.Empty, name, phone);
            var service = new AccountService(_aetherDbContext, _memberContextCacheService);

            var (_, _, account) = await service.Update(memberContext, accountRequest);

            Assert.Equal(account.Address, address);
            Assert.Equal(account.Name, name);
            Assert.Equal(account.Phone, phone);
        }
    

        private readonly IEnumerable<Account> _accounts = new []
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
        
        private readonly IEnumerable<Member> _members = new []
        {
            new Member
            {
                Id = 1
            }
        };
        
        private readonly AetherDbContext _aetherDbContext;
        private readonly IMemberContextCacheService _memberContextCacheService;
    }
}
