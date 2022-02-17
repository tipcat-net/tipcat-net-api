using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Moq;
using Newtonsoft.Json;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Models.Preferences;
using TipCatDotNet.Api.Services.Preferences;
using TipCatDotNet.ApiTests.Utils;
using Xunit;

namespace TipCatDotNet.ApiTests;

public class PreferencesServiceTests
{
    // TODO: update tests, when server-side settings appear
    public PreferencesServiceTests()
    {
        var aetherDbContextMock = MockContextFactory.Create();
        aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts)).Verifiable();
        aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members)).Verifiable();

        _aetherDbContext = aetherDbContextMock.Object;
    }


    [Theory]
    [InlineData("")]
    [InlineData("{null")]
    public async Task AddOrUpdate_should_return_error_when_application_settings_is_not_parseable(string applicationPreferences)
    {
        var memberContext = new MemberContext(1, string.Empty, 1, null);
        var service = new PreferencesService(_aetherDbContext);

        var (_, isFailure) = await service.AddOrUpdate(memberContext, new PreferencesRequest(new AccountPreferences(), applicationPreferences));

        Assert.True(isFailure);
    }

    
    [Fact]
    public async Task AddOrUpdate_should_not_update_server_side_preferences_if_target_member_is_not_manager_or_supervisor()
    {
        var aetherDbContextMock = MockContextFactory.Create();
        aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts)).Verifiable();
        aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members)).Verifiable();
        
        var memberContext = new MemberContext(1, string.Empty, 1, null);
        var service = new PreferencesService(aetherDbContextMock.Object);

        var (_, isFailure) = await service.AddOrUpdate(memberContext, new PreferencesRequest(new AccountPreferences(), "{}"));

        Assert.False(isFailure);
        aetherDbContextMock.Verify(c => c.Members, Times.Exactly(3));
        aetherDbContextMock.Verify(c => c.Accounts, Times.Once);
    }

    
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task AddOrUpdate_should_update_server_side_preferences_if_target_member_is_manager_or_supervisor(int managerId)
    {
        var aetherDbContextMock = MockContextFactory.Create();
        aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts)).Verifiable();
        aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members)).Verifiable();
        
        var memberContext = new MemberContext(managerId, string.Empty, 1, null);
        var service = new PreferencesService(aetherDbContextMock.Object);

        var (_, isFailure) = await service.AddOrUpdate(memberContext, new PreferencesRequest(new AccountPreferences(), "{}"));

        Assert.False(isFailure);
        aetherDbContextMock.Verify(c => c.Members, Times.Exactly(3));
        aetherDbContextMock.Verify(c => c.Accounts, Times.Exactly(3));
    }

    
    [Fact]
    public async Task Get_should_return_preferences()
    {
        var memberContext = new MemberContext(1, string.Empty, 1, null);
        var applicationPreferences = "{}";
        var service = new PreferencesService(_aetherDbContext);

        var (_, isFailure, preferences) = await service.AddOrUpdate(memberContext, new PreferencesRequest(new AccountPreferences(), applicationPreferences));

        Assert.False(isFailure);
        Assert.Equal(applicationPreferences, preferences.ApplicationSidePreferences);
        Assert.Equal(JsonConvert.SerializeObject(new AccountPreferences()), JsonConvert.SerializeObject(preferences.ServerSidePreferences));
    }


    private readonly IEnumerable<Account> _accounts = new[]
    {
        new Account
        {
            Id = 1
        }
    };

    private readonly IEnumerable<Member> _members = new[]
    {
        new Member
        {
            Id = 1,
            AccountId = 1,
            Permissions = MemberPermissions.Employee
        },
        new Member
        {
            Id = 2,
            AccountId = 1,
            Permissions = MemberPermissions.Manager
        },
        new Member
        {
            Id = 3,
            AccountId = 1,
            Permissions = MemberPermissions.Supervisor
        }
    };


    private readonly AetherDbContext _aetherDbContext;
}