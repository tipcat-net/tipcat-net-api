using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MailSender;
using Microsoft.Extensions.Options;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Auth;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.Company;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.Api.Services.Company;
using TipCatDotNet.ApiTests.Utils;
using Xunit;

namespace TipCatDotNet.ApiTests;

public class InvitationServiceTests
{
    public InvitationServiceTests()
    {
        var aetherDbContextMock = MockContextFactory.Create();
        aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts));
        aetherDbContextMock.Setup(c => c.MemberInvitations).Returns(DbSetMockProvider.GetDbSetMock(_memberInvitations));
        aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));

        _aetherDbContext = aetherDbContextMock.Object;

        var optionsMonitorMock = new Mock<IOptionsMonitor<InvitationServiceOptions>>();
        optionsMonitorMock.Setup(o => o.CurrentValue)
            .Returns(new InvitationServiceOptions
            {
                TemplateId = string.Empty
            });

        _optionsMonitor = optionsMonitorMock.Object;

        var mailSenderMock = new Mock<IMailSender>();
        mailSenderMock.Setup(s => s.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(Result.Success);

        _mailSender = mailSenderMock.Object;

        var userManagementClientMock = new Mock<IUserManagementClient>();
        _userManagementClient = userManagementClientMock.Object;

        _memberContext = new MemberContext(1, string.Empty, 1, null);

        var companyInfoServiceMock = new Mock<ICompanyInfoService>();
        companyInfoServiceMock.Setup(s => s.Get())
            .Returns(new CompanyInfo(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));

        _companyInfoService = companyInfoServiceMock.Object;
    }


    [Fact]
    public async Task CreateAndSend_should_return_error_when_user_was_not_added()
    {
        var request = new MemberRequest(154, 1, "Katrina", "Fisher", "katrina.fisher@example.com", MemberPermissions.Employee);
        var userManagementClientMock = new Mock<IUserManagementClient>();
        userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>("Error"));

        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, userManagementClientMock.Object, _companyInfoService);
        var (_, isFailure) = await service.CreateAndSend(request);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task CreateAndSend_should_return_error_when_password_reset_ticket_was_not_created()
    {
        var request = new MemberRequest(154, 1, "Katrina", "Fisher", "katrina.fisher@example.com", MemberPermissions.Employee);
        var userManagementClientMock = new Mock<IUserManagementClient>();
        userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid().ToString());

        userManagementClientMock.Setup(c => c.ChangePassword(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>("Error"));

        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, userManagementClientMock.Object, _companyInfoService);
        var (_, isFailure) = await service.CreateAndSend(request);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task CreateAndSend_should_return_success()
    {
        var request = new MemberRequest(22, 1, "Katrina", "Fisher", "katrina.fisher@example.com", MemberPermissions.Employee);
        var userManagementClientMock = new Mock<IUserManagementClient>();
        userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid().ToString());

        userManagementClientMock.Setup(c => c.ChangePassword(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://invitation.com/url-address");

        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, userManagementClientMock.Object, _companyInfoService);
        var (_, isFailure) = await service.CreateAndSend(request);

        Assert.False(isFailure);
    }


    [Fact]
    public async Task Redeem_should_return_error_when_invitation_not_exist()
    {
        const int memberId = 15;

        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient, _companyInfoService);
        var (_, isFailure) = await service.Redeem(memberId);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task Redeem_should_return_success()
    {
        const int memberId = 16;

        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient, _companyInfoService);
        var (_, isFailure) = await service.Redeem(memberId);

        Assert.False(isFailure);
    }


    [Fact]
    public async Task Send_should_return_error_when_current_member_does_not_belong_to_account()
    {
        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient, _companyInfoService);
        var (_, isFailure) = await service.Send(_memberContext, Build(8));

        Assert.True(isFailure);
    }


    [Fact]
    public async Task Send_should_return_error_when_no_invitations_found()
    {
        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient, _companyInfoService);
        var (_, isFailure) = await service.Send(_memberContext, Build(15));

        Assert.True(isFailure);
    }


    [Fact]
    public async Task Send_should_return_error_when_invitation_already_accepted()
    {
        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient, _companyInfoService);
        var (_, isFailure) = await service.Send(_memberContext, Build(18));

        Assert.True(isFailure);
    }


    [Theory]
    [InlineData(21)]
    [InlineData(22)]
    public async Task Send_should_return_success(int memberId)
    {
        var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient, _companyInfoService);
        var (_, isFailure) = await service.Send(_memberContext, Build(memberId));

        Assert.False(isFailure);
    }


    private static MemberRequest Build(int memberId) 
        => new(memberId, 1, "cherly.hoffman@example.com");


    private readonly IEnumerable<Account> _accounts = new List<Account>
    {
        new()
        {
            Id = 1,
            OperatingName = "Tipcat.net"
        }
    };


    private readonly IEnumerable<MemberInvitation> _memberInvitations = new List<MemberInvitation>
    {
        new()
        {
            Id = 1,
            Code = null,
            Link = string.Empty,
            MemberId = 16
        },
        new()
        {
            Id = 1,
            Code = null,
            Link = string.Empty,
            MemberId = 18,
            State = InvitationStates.Accepted
        },
        new()
        {
            Id = 1,
            Code = null,
            Link = string.Empty,
            MemberId = 21,
            State = InvitationStates.NotSent,
            Created = DateTime.UtcNow.AddDays(5)
        },
        new()
        {
            Id = 1,
            Code = null,
            Link = string.Empty,
            MemberId = 22,
            State = InvitationStates.Sent,
            Created = DateTime.UtcNow.AddDays(5)
        },
        new()
        {
            Id = 1,
            Code = null,
            Link = string.Empty,
            MemberId = 23,
            State = InvitationStates.Sent,
            Created = DateTime.UtcNow.AddDays(-7)
        }
    };


    private readonly IEnumerable<Member> _members = new List<Member>
    {
        new()
        {
            Id = 15,
            AccountId = 1,
            Email = "cherly.hoffman@example.com"
        },
        new()
        {
            Id = 17,
            AccountId = 1,
            Email = "cherly.hoffman@example.com"
        },
        new()
        {
            Id = 18,
            AccountId = 1,
            Email = "cherly.hoffman@example.com"
        }
        ,new()
        {
            Id = 21,
            AccountId = 1,
            Email = "cherly.hoffman@example.com"
        },
        new()
        {
            Id = 22,
            AccountId = 1,
            Email = "katrina.fisher@example.com"
        },
        new()
        {
            Id = 23,
            AccountId = 1,
            Email = "katrina.fisher@example.com"
        }
    };

        
    private readonly AetherDbContext _aetherDbContext;
    private readonly ICompanyInfoService _companyInfoService;
    private readonly IOptionsMonitor<InvitationServiceOptions> _optionsMonitor;
    private readonly IMailSender _mailSender;
    private readonly MemberContext _memberContext;
    private readonly IUserManagementClient _userManagementClient;
}