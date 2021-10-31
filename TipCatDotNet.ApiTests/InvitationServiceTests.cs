using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Auth;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Permissions.Enums;
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
            aetherDbContextMock.Setup(c => c.MemberInvitations).Returns(DbSetMockProvider.GetDbSetMock(_memberInvitations));

            _aetherDbContextMock = aetherDbContextMock.Object;
        }


        [Fact]
        public async Task Send_should_return_error_when_user_was_not_added()
        {
            var request = new MemberRequest(154, 1, "Katrina", "Fisher", "katrina.fisher@example.com", MemberPermissions.Employee);
            var userManagementClientMock = new Mock<IUserManagementClient>();
            userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<string>("Error"));

            var service = new InvitationService(_aetherDbContextMock, userManagementClientMock.Object);
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

            var service = new InvitationService(_aetherDbContextMock, userManagementClientMock.Object);
            var (_, isFailure) = await service.Send(request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Send_should_return_success()
        {
            var request = new MemberRequest(154, 1, "Katrina", "Fisher", "katrina.fisher@example.com", MemberPermissions.Employee);
            var userManagementClientMock = new Mock<IUserManagementClient>();
            userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString());

            userManagementClientMock.Setup(c => c.ChangePassword(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://invitation.com/url-address");

            var service = new InvitationService(_aetherDbContextMock, userManagementClientMock.Object);
            var (_, isFailure, error) = await service.Send(request);

            Assert.False(isFailure);
        }


        private readonly IEnumerable<MemberInvitation> _memberInvitations = Array.Empty<MemberInvitation>();
    
        
        private readonly AetherDbContext _aetherDbContextMock;
    }
}
