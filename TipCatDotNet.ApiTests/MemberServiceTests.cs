using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Graph;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.ApiTests.Utils;
using Xunit;

namespace TipCatDotNet.ApiTests
{
    // https://mgwdevcom.wordpress.com/2020/11/03/unit-testing-with-graph-sdk/ used for GraphServiceClient mocking.
    public class MemberServiceTests
    {
        public MemberServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));

            _aetherDbContext = aetherDbContextMock.Object;

            _graphServiceClient = new GraphServiceClient(new MockAuthenticationProvider(), new MockHttpProvider());
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_member_id_is_null()
        {
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _graphServiceClient);

            var (_, isFailure) = await service.AddCurrent(null, MemberPermissions.None);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_users_given_name_is_null()
        {
            const string objectId = "73bfedfa-3d86-4e37-8677-bfb20b74ad95";
            const string requestUrl = "https://graph.microsoft.com/v1.0/users/" + objectId;
            var httpProvider = new MockHttpProvider();
            httpProvider.OnRequestExecuting += delegate (object _, MockRequestExecutingEventArgs eventArgs)
            {
                if(eventArgs?.RequestMessage?.RequestUri?.ToString() == requestUrl)
                {
                    eventArgs.Result = new User
                    {
                        GivenName = null
                    };
                }
            };

            var graphServiceClient = new GraphServiceClient(new MockAuthenticationProvider(), httpProvider);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, graphServiceClient);

            var (_, isFailure) = await service.AddCurrent(objectId, MemberPermissions.None);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_users_surname_is_null()
        {
            const string objectId = "73bfedfa-3d86-4e37-8677-bfb20b74ad95";
            const string requestUrl = "https://graph.microsoft.com/v1.0/users/" + objectId;
            var httpProvider = new MockHttpProvider();
            httpProvider.OnRequestExecuting += delegate (object _, MockRequestExecutingEventArgs eventArgs)
            {
                if(eventArgs?.RequestMessage?.RequestUri?.ToString() == requestUrl)
                {
                    eventArgs.Result = new User
                    {
                        GivenName = "David",
                        Surname = null
                    };
                }
            };

            var graphServiceClient = new GraphServiceClient(new MockAuthenticationProvider(), httpProvider);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, graphServiceClient);

            var (_, isFailure) = await service.AddCurrent(objectId, MemberPermissions.None);

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData("73bfedfa-3d86-4e37-8677-bfb20b74ad95", "David", "Thomas", MemberPermissions.Manager)]
        [InlineData("4ac0fbad-c7bb-433d-96a3-47a11716f2d5", "Clark", "Owens", MemberPermissions.Employee)]
        public async Task AddCurrent_should_return_member(string objectId, string givenName, string surname, MemberPermissions permissions)
        {
            var requestUrl = "https://graph.microsoft.com/v1.0/users/" + objectId;
            var httpProvider = new MockHttpProvider();
            httpProvider.OnRequestExecuting += delegate (object _, MockRequestExecutingEventArgs eventArgs)
            {
                if(eventArgs?.RequestMessage?.RequestUri?.ToString() == requestUrl)
                {
                    eventArgs.Result = new User
                    {
                        GivenName = givenName,
                        Surname = surname
                    };
                }
            };

            var graphServiceClient = new GraphServiceClient(new MockAuthenticationProvider(), httpProvider);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, graphServiceClient);

            var (_, isFailure, memberInfoResponse) = await service.AddCurrent(objectId, permissions);

            Assert.False(isFailure);
            Assert.Equal(givenName, memberInfoResponse.FirstName);
            Assert.Equal(surname, memberInfoResponse.LastName);
            Assert.Equal(permissions, memberInfoResponse.Permissions);
        }


        [Fact]
        public async Task GetCurrent_should_throws_exception_when_member_context_is_null()
        {
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _graphServiceClient);

            await Assert.ThrowsAsync<TargetInvocationException>(async () => await service.GetCurrent(null));
        }


        [Fact]
        public async Task GetCurrent_should_return_error_when_member_is_not_found()
        {
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _graphServiceClient);
            
            var (_, isFailure) = await service.GetCurrent(new MemberContext(0, string.Empty));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task GetCurrent_should_return_member_when_member_is_found()
        {
            const int memberId = 1;
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _graphServiceClient);
            
            var (_, isFailure, memberInfoResponse) = await service.GetCurrent(new MemberContext(memberId, string.Empty));

            Assert.False(isFailure);
            Assert.Equal(memberInfoResponse.Id, memberId);
        }
        

        private readonly IEnumerable<Member> _members = new []
        {
            new Member
            {
                Id = 1,
                FirstName = "Elizabeth",
                LastName = "Omara",
                Email = null,
                Permissions = MemberPermissions.Manager
            }
        };

        private readonly AetherDbContext _aetherDbContext;
        private readonly GraphServiceClient _graphServiceClient;
    }
}
