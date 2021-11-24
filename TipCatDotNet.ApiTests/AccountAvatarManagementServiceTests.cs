using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Images;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Services.Images;
using TipCatDotNet.ApiTests.Utils;
using Xunit;

namespace TipCatDotNet.ApiTests;

public class AccountAvatarManagementServiceTests
{
    public AccountAvatarManagementServiceTests()
    {
        var optionsMock = new Mock<IOptionsMonitor<AvatarManagementServiceOptions>>();
        optionsMock.Setup(o => o.CurrentValue)
            .Returns(new AvatarManagementServiceOptions { BucketName = string.Empty });

        _options = optionsMock.Object;


        var aetherDbContextMock = MockContextFactory.Create();
        aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts));

        _aetherDbContext = aetherDbContextMock.Object;


        var awsImageManagementServiceMock = new Mock<IAwsImageManagementService>();
        awsImageManagementServiceMock.Setup(s => s.Upload(It.IsAny<string>(), It.IsAny<FormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Func<string, FormFile, string, CancellationToken, Result<string>>((_, _, key, _) => key));

        _awsImageManagementServiceMock = awsImageManagementServiceMock.Object;

        _memberContext = new MemberContext(1, string.Empty, 1, null);

    }


    [Fact]
    public async Task AddOrUpdate_should_return_error_when_file_is_null()
    {
        var request = new AccountAvatarRequest();
        var service = new AccountAvatarManagementService(_options, _aetherDbContext, _awsImageManagementServiceMock);

        var (_, isFailure) = await service.AddOrUpdate(_memberContext, request);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task AddOrUpdate_should_return_error_when_file_is_not_image()
    {
        var request = new AccountAvatarRequest(0, new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, string.Empty, "file.doc"));
        var service = new AccountAvatarManagementService(_options, _aetherDbContext, _awsImageManagementServiceMock);

        var (_, isFailure) = await service.AddOrUpdate(_memberContext, request);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task AddOrUpdate_should_return_error_when_account_id_is_zero()
    {
        var request = new AccountAvatarRequest(0, new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, string.Empty, "file.jpg"));
        var service = new AccountAvatarManagementService(_options, _aetherDbContext, _awsImageManagementServiceMock);

        var (_, isFailure) = await service.AddOrUpdate(_memberContext, request);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task AddOrUpdate_should_return_error_when_current_member_does_not_belong_to_account()
    {
        var request = new AccountAvatarRequest(2, new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, string.Empty, "file.jpg"));
        var service = new AccountAvatarManagementService(_options, _aetherDbContext, _awsImageManagementServiceMock);

        var (_, isFailure) = await service.AddOrUpdate(_memberContext, request);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task AddOrUpdate_should_return_avatar_url()
    {
        var request = new AccountAvatarRequest(1, new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, string.Empty, "file.jpg"));
        var service = new AccountAvatarManagementService(_options, _aetherDbContext, _awsImageManagementServiceMock);

        var (_, isFailure, url) = await service.AddOrUpdate(_memberContext, request);

        var expected = AvatarKeyHelper.BuildAccountKey(request.AccountId);
        Assert.False(isFailure);
        Assert.Equal(expected, url);
    }


    [Fact]
    public async Task Remove_should_return_error_when_account_id_is_zero()
    {
        var request = new AccountAvatarRequest(0, new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, string.Empty, "file.jpg"));
        var service = new AccountAvatarManagementService(_options, _aetherDbContext, _awsImageManagementServiceMock);

        var (_, isFailure) = await service.Remove(_memberContext, request);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task Remove_should_return_error_when_current_member_does_not_belong_to_account()
    {
        var request = new AccountAvatarRequest(2, new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, string.Empty, "file.jpg"));
        var service = new AccountAvatarManagementService(_options, _aetherDbContext, _awsImageManagementServiceMock);

        var (_, isFailure) = await service.Remove(_memberContext, request);

        Assert.True(isFailure);
    }


    [Fact]
    public async Task Remove_should_return_result()
    {
        var request = new AccountAvatarRequest(1, new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, string.Empty, "file.jpg"));
        var service = new AccountAvatarManagementService(_options, _aetherDbContext, _awsImageManagementServiceMock);

        var (_, isFailure) = await service.Remove(_memberContext, request);

        Assert.False(isFailure);
    }


    private readonly IEnumerable<Account> _accounts = new[]
    {
        new Account
        {
            Id = 1
        }
    };


    private readonly AetherDbContext _aetherDbContext;
    private readonly IAwsImageManagementService _awsImageManagementServiceMock;
    private readonly MemberContext _memberContext;
    private readonly IOptionsMonitor<AvatarManagementServiceOptions> _options;
}