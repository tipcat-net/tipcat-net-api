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
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.ApiTests.Utils;
using Xunit;

namespace TipCatDotNet.ApiTests
{
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
        }


        [Fact]
        public async Task Send_should_return_error_when_user_was_not_added()
        {
            var request = new MemberRequest(154, 1, "Katrina", "Fisher", "katrina.fisher@example.com", MemberPermissions.Employee);
            var userManagementClientMock = new Mock<IUserManagementClient>();
            userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<string>("Error"));

            var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, userManagementClientMock.Object);
            var (_, isFailure) = await service.Send(request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Send_should_return_error_when_password_reset_ticket_was_not_created()
        {
            var request = new MemberRequest(154, 1, "Katrina", "Fisher", "katrina.fisher@example.com", MemberPermissions.Employee);
            var userManagementClientMock = new Mock<IUserManagementClient>();
            userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString());

            userManagementClientMock.Setup(c => c.ChangePassword(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<string>("Error"));

            var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, userManagementClientMock.Object);
            var (_, isFailure) = await service.Send(request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Send_should_return_success()
        {
            var request = new MemberRequest(22, 1, "Katrina", "Fisher", "katrina.fisher@example.com", MemberPermissions.Employee);
            var userManagementClientMock = new Mock<IUserManagementClient>();
            userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString());

            userManagementClientMock.Setup(c => c.ChangePassword(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://invitation.com/url-address");

            var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, userManagementClientMock.Object);
            var (_, isFailure) = await service.Send(request);

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Redeem_should_return_error_when_invitation_not_exist()
        {
            const int memberId = 15;

            var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient);
            var (_, isFailure) = await service.Redeem(memberId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Redeem_should_return_success()
        {
            const int memberId = 16;

            var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient);
            var (_, isFailure) = await service.Redeem(memberId);

            Assert.False(isFailure);
        }


        [Theory]
        [InlineData(15)]
        [InlineData(17)]
        [InlineData(18)]
        public async Task Resend_should_return_error_when_no_invitations_found(int memberId)
        {
            var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient);
            var (_, isFailure) = await service.Resend(memberId);

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData(19)]
        [InlineData(20)]
        public async Task Resend_should_return_error_when_no_members_found(int memberId)
        {
            var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient);
            var (_, isFailure) = await service.Resend(memberId);

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData(21)]
        [InlineData(22)]
        public async Task Resend_should_return_success(int memberId)
        {
            var service = new InvitationService(_optionsMonitor, _aetherDbContext, _mailSender, _userManagementClient);
            var (_, isFailure) = await service.Resend(memberId);

            Assert.False(isFailure);
        }


        private readonly IEnumerable<Account> _accounts = new List<Account>
        {
            new()
            {
                Id = 1,
                OperatingName = "Tipcat.net"
            }
        };


        private readonly IEnumerable<Member> _members = new List<Member>
        {
            new()
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
                MemberId = 17,
                State = InvitationStates.None
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
                MemberId = 19,
                State = InvitationStates.NotSent
            },
            new()
            {
                Id = 1,
                Code = null,
                Link = string.Empty,
                MemberId = 20,
                State = InvitationStates.Sent
            },
            new()
            {
                Id = 1,
                Code = null,
                Link = string.Empty,
                MemberId = 21,
                State = InvitationStates.NotSent
            },
            new()
            {
                Id = 1,
                Code = null,
                Link = string.Empty,
                MemberId = 22,
                State = InvitationStates.Sent
            }
        };
        
        private readonly AetherDbContext _aetherDbContext;
        private readonly IOptionsMonitor<InvitationServiceOptions> _optionsMonitor;
        private readonly IMailSender _mailSender;
        private readonly IUserManagementClient _userManagementClient;
    }
}
