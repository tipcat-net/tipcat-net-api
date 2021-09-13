using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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
            aetherDbContextMock.Setup(c => c.AccountMembers).Returns(DbSetMockProvider.GetDbSetMock(_accountMembers));

            _aetherDbContext = aetherDbContextMock.Object;
        }


        [Fact]
        public async Task Add_should_not_add_account_when_member_has_one()
        {
            var service = new AccountService(_aetherDbContext);

            var (_, isFailure) = await service.Add(new MemberContext(1, 1, string.Empty), new AccountRequest());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_name_is_not_specified()
        {
            var accountRequest = new AccountRequest(string.Empty, null, null, string.Empty, string.Empty);
            var memberContext = new MemberContext(1, null, string.Empty);
            var service = new AccountService(_aetherDbContext);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_address_is_not_specified()
        {
            var accountRequest = new AccountRequest(string.Empty, null, null, "Tipcat.net", string.Empty);
            var memberContext = new MemberContext(1, null, string.Empty);
            var service = new AccountService(_aetherDbContext);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_not_add_account_when_phone_is_not_specified()
        {
            var accountRequest = new AccountRequest("Dubai, Saraya Avenue Building, B2, 205", null, null, "Tipcat.net", string.Empty);
            var memberContext = new MemberContext(1, null, string.Empty);
            var service = new AccountService(_aetherDbContext);

            var (_, isFailure) = await service.Add(memberContext, accountRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_add_account()
        {
            var request = new AccountRequest("Dubai, Saraya Avenue Building, B2, 205", null, null, "Tipcat.net", "+8 (800) 2000 500");
            var memberContext = new MemberContext(1, null, "kirill.taran@tipcat.net");
            var service = new AccountService(_aetherDbContext);

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
            var memberContext = new MemberContext(1, null, string.Empty);
            var service = new AccountService(_aetherDbContext);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_not_get_account_when_member_has_no_access_to_account()
        {
            const int accountId = 1;
            var memberContext = new MemberContext(1, 0, string.Empty);
            var service = new AccountService(_aetherDbContext);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_not_get_account_when_account_deactivated()
        {
            const int accountId = 1;
            var memberContext = new MemberContext(1, 1, string.Empty);
            var service = new AccountService(_aetherDbContext);

            var (_, isFailure) = await service.Get(memberContext, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_get_account()
        {
            const int accountId = 2;
            var memberContext = new MemberContext(1, accountId, string.Empty);
            var service = new AccountService(_aetherDbContext);

            var (_, _, accountInfo) = await service.Get(memberContext, accountId);

            Assert.Equal(accountInfo.Id, accountId);
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
        
        private readonly IEnumerable<AccountMember> _accountMembers = System.Array.Empty<AccountMember>();
        
        private readonly AetherDbContext _aetherDbContext;
    }
}
