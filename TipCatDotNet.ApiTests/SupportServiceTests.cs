using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MailSender;
using Microsoft.Extensions.Options;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.Company;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Services.Company;
using TipCatDotNet.ApiTests.Utils;
using Xunit;

namespace TipCatDotNet.ApiTests;

public class SupportServiceTests
{
    public SupportServiceTests()
    {
        var aetherDbContextMock = MockContextFactory.Create();
        aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));

        _aetherDbContext = aetherDbContextMock.Object;

        var optionsMonitorMock = new Mock<IOptionsMonitor<SupportOptions>>();
        optionsMonitorMock.Setup(o => o.CurrentValue)
            .Returns(new SupportOptions
            {
                SupportEmailAddress = string.Empty,
                SupportRequestToMemberTemplateId = string.Empty,
                SupportRequestToSupportTemplateId = string.Empty
            });

        _optionsMonitor = optionsMonitorMock.Object;

        var companyInfoServiceMock = new Mock<ICompanyInfoService>();
        companyInfoServiceMock.Setup(s => s.Get())
            .Returns(new CompanyInfo(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));

        _companyInfoService = companyInfoServiceMock.Object;

        var sendGridMailSenderMock = new Mock<IMailSender>();
        sendGridMailSenderMock.Setup(s => s.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.FromResult(Result.Success()));

        _sendGridMailSender = sendGridMailSenderMock.Object;
    }


    [Fact]
    public async Task SendRequest_should_fail_when_member_has_no_email()
    {
        var service = new SupportService(_optionsMonitor, _aetherDbContext, _companyInfoService, _sendGridMailSender);

        var (_, isFailure) = await service.SendRequest(new MemberContext(15, "hash", null, null), new SupportRequest(string.Empty));

        Assert.True(isFailure);
    }


    [Fact]
    public async Task SendRequest_should_fail_when_request_has_no_content()
    {
        var service = new SupportService(_optionsMonitor, _aetherDbContext, _companyInfoService, _sendGridMailSender);

        var (_, isFailure) = await service.SendRequest(new MemberContext(15, "hash", null, "cherly.hoffman@example.com"), new SupportRequest(string.Empty));

        Assert.True(isFailure);
    }


    [Fact]
    public async Task SendRequest_should_fail_when_message_to_support_fails()
    {
        var sendGridMailSenderMock = new Mock<IMailSender>();
        sendGridMailSenderMock.Setup(s => s.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.FromResult(Result.Failure("error")))
            .Verifiable();

        var service = new SupportService(_optionsMonitor, _aetherDbContext, _companyInfoService, sendGridMailSenderMock.Object);

        var (_, isFailure) = await service.SendRequest(new MemberContext(15, "hash", null, "cherly.hoffman@example.com"), new SupportRequest("Content"));

        Assert.True(isFailure);
        sendGridMailSenderMock.Verify(s => s.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }


    [Fact]
    public async Task SendRequest_should_succeed()
    {
        var service = new SupportService(_optionsMonitor, _aetherDbContext, _companyInfoService, _sendGridMailSender);

        var (_, isFailure) = await service.SendRequest(new MemberContext(15, "hash", null, "cherly.hoffman@example.com"), new SupportRequest("Content"));

        Assert.False(isFailure);
    }


    private readonly IEnumerable<Member> _members = new List<Member>
    {
        new()
        {
            Id = 15,
            AccountId = 1,
            Email = "cherly.hoffman@example.com",
            FirstName = "Cherly",
            LastName = "Hoffman"
        }
    };

    
    private readonly AetherDbContext _aetherDbContext;
    private readonly ICompanyInfoService _companyInfoService;
    private readonly IOptionsMonitor<SupportOptions> _optionsMonitor;
    private readonly IMailSender _sendGridMailSender;
}