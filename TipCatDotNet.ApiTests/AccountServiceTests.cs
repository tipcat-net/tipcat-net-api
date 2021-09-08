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

            _aetherDbContext = aetherDbContextMock.Object;
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
        
        private readonly AetherDbContext _aetherDbContext;
    }
}
