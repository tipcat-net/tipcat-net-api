using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using Microsoft.EntityFrameworkCore;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using Xunit;

namespace TipCatDotNet.ApiTests
{
    public class PermissionCheckerTests
    {
        [Fact]
        public async Task CheckMemberPermissions_should_return_error_when_member_has_none_permissions()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var cacheMock = new Mock<IMemoryFlow>();
            cacheMock.Setup(c => c.Options).Returns(new FlowOptions());
            cacheMock.Setup(c
                    => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<MemberPermissions>>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MemberPermissions.None);

            var service = new PermissionChecker(new AetherDbContext(new DbContextOptions<AetherDbContext>()), cacheMock.Object);

            var (_, isFailure) = await service.CheckMemberPermissions(memberContext, MemberPermissions.None);

            Assert.True(isFailure);
        }
        
        
        [Fact]
        public async Task CheckMemberPermissions_should_return_error_when_member_has_no_permissions()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var cacheMock = new Mock<IMemoryFlow>();
            cacheMock.Setup(c => c.Options).Returns(new FlowOptions());
            cacheMock.Setup(c
                    => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<MemberPermissions>>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MemberPermissions.None);

            var service = new PermissionChecker(new AetherDbContext(new DbContextOptions<AetherDbContext>()), cacheMock.Object);

            var (_, isFailure) = await service.CheckMemberPermissions(memberContext, MemberPermissions.Manager);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task CheckMemberPermissions_should_return_result_when_member_has_same_permission()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var cacheMock = new Mock<IMemoryFlow>();
            cacheMock.Setup(c => c.Options).Returns(new FlowOptions());
            cacheMock.Setup(c
                    => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<MemberPermissions>>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MemberPermissions.Employee);

            var service = new PermissionChecker(new AetherDbContext(new DbContextOptions<AetherDbContext>()), cacheMock.Object);

            var (_, isFailure) = await service.CheckMemberPermissions(memberContext, MemberPermissions.Employee);

            Assert.False(isFailure);
        }


        [Fact]
        public async Task CheckMemberPermissions_should_return_result_when_member_has_one_of_permissions()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var cacheMock = new Mock<IMemoryFlow>();
            cacheMock.Setup(c => c.Options).Returns(new FlowOptions());
            cacheMock.Setup(c
                    => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<MemberPermissions>>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MemberPermissions.Supervisor);

            var service = new PermissionChecker(new AetherDbContext(new DbContextOptions<AetherDbContext>()), cacheMock.Object);

            var (_, isFailure) = await service.CheckMemberPermissions(memberContext, MemberPermissions.Employee | MemberPermissions.Supervisor | MemberPermissions.Manager);

            Assert.False(isFailure);
        }
    }
}
