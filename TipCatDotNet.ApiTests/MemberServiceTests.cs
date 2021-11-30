using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.ApiTests.Utils;
using TipCatDotNet.Api.Models.Auth;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.Api.Services.Images;
using Xunit;

namespace TipCatDotNet.ApiTests
{
    public class MemberServiceTests
    {
        public MemberServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts));
            aetherDbContextMock.Setup(c => c.Facilities).Returns(DbSetMockProvider.GetDbSetMock(_facilities));
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));

            _aetherDbContext = aetherDbContextMock.Object;

            var userManagementClientMock = new Mock<IUserManagementClient>();
            userManagementClientMock.Setup(c => c.Add(It.IsAny<MemberRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            _userManagementClient = userManagementClientMock.Object;

            var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
            qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<string>());

            _qrCodeGenerator = qrCodeGeneratorMock.Object;

            var invitationServiceMock = new Mock<IInvitationService>();
            invitationServiceMock.Setup(s => s.GetState(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, InvitationStates>());

            _invitationService = invitationServiceMock.Object;

            var stripeAccountServiceMock = new Mock<IStripeAccountService>();
            stripeAccountServiceMock.Setup(s => s.Add(It.IsAny<MemberRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<int>());
            stripeAccountServiceMock.Setup(s => s.Update(It.IsAny<MemberRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            stripeAccountServiceMock.Setup(s => s.Retrieve(It.IsAny<MemberRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<StripeAccountResponse>());
            stripeAccountServiceMock.Setup(s => s.Remove(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _stripeAccountService = stripeAccountServiceMock.Object;
        }


        [Fact]
        public async Task Add_should_return_error_when_first_name_is_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Add(memberContext, new MemberRequest());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_last_name_is_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, null, "Angela", string.Empty, null, MemberPermissions.None);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_request_permissions_are_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, null, "Angela", "Carey", null, MemberPermissions.None);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_account_id_is_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, null, "Angela", "Carey", null, MemberPermissions.Employee);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_request_email_exists()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, 5, "Angela", "Carey", null, MemberPermissions.Employee);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_request_email_is_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, 5, "Angela", "Carey", "existing@email.com", MemberPermissions.Employee);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_current_member_does_not_belongs_to_target_account()
        {
            var memberContext = new MemberContext(1, string.Empty, 3, null);
            var memberRequest = new MemberRequest(null, 5, "Angela", "Carey", "AngelaDCarey@armyspy.com", MemberPermissions.Employee);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_account_already_has_manager()
        {
            var memberContext = new MemberContext(1, string.Empty, 8, null);
            var memberRequest = new MemberRequest(null, 8, "Angela", "Carey", "AngelaDCarey@armyspy.com", MemberPermissions.Manager);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        // [Fact]
        // public async Task Add_should_return_member()
        // {
        //     const int accountId = 5;
        //     const string firstName = "Angela";
        //     const string lastName = "Carey";
        //     var memberContext = new MemberContext(1, string.Empty, accountId, null);
        //     var memberRequest = new MemberRequest(null, accountId, firstName, lastName, "AngelaDCarey@armyspy.com", MemberPermissions.Employee);
        //     var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

        //     var (_, _, member) = await service.Add(memberContext, memberRequest);

        //     Assert.Equal(firstName, member.FirstName);
        //     Assert.Equal(lastName, member.LastName);
        //     Assert.Equal(accountId, member.AccountId);
        // }


        [Fact]
        public async Task AddCurrent_should_return_error_when_token_id_is_null()
        {
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.AddCurrent(null);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_identity_hash_already_used()
        {
            const string objectId = "426af836-dcef-4b72-b99b-a92c3e0a41a3";
            var microsoftGraphClientMock = new Mock<IUserManagementClient>();
            microsoftGraphClientMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserContext(null, null, null));

            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.AddCurrent(objectId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_users_given_name_is_null()
        {
            const string objectId = "73bfedfa-3d86-4e37-8677-bfb20b74ad95";
            var microsoftGraphClientMock = new Mock<IUserManagementClient>();
            microsoftGraphClientMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserContext(null, null, null));

            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.AddCurrent(objectId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_users_surname_is_null()
        {
            const string objectId = "73bfedfa-3d86-4e37-8677-bfb20b74ad95";
            var microsoftGraphClientMock = new Mock<IUserManagementClient>();
            microsoftGraphClientMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserContext("David", null, null));

            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.AddCurrent(objectId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_email_is_null()
        {
            const string objectId = "73bfedfa-3d86-4e37-8677-bfb20b74ad95";
            var microsoftGraphClientMock = new Mock<IUserManagementClient>();
            microsoftGraphClientMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserContext("David", "Thomas", null));

            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.AddCurrent(objectId);

            Assert.True(isFailure);
        }


        // [Theory]
        // [InlineData("73bfedfa-3d86-4e37-8677-bfb20b74ad95", "David", "Thomas", "dtomas@gmail.com")]
        // [InlineData("4ac0fbad-c7bb-433d-96a3-47a11716f2d5", "Clark", "Owens", "clark11@hotmail.com")]
        // public async Task AddCurrent_should_return_member(string objectId, string givenName, string surname, string email)
        // {
        //     var microsoftGraphClientMock = new Mock<IUserManagementClient>();
        //     microsoftGraphClientMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(new UserContext(givenName, surname, email));

        //     var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator, _invitationService);

        //     var (_, isFailure, member) = await service.AddCurrent(objectId);

        //     Assert.False(isFailure);
        //     Assert.Equal(givenName, member.FirstName);
        //     Assert.Equal(surname, member.LastName);
        //     Assert.Equal(MemberPermissions.Manager, member.Permissions);
        //     Assert.Equal(email, member.Email);
        // }


        // [Theory]
        // [InlineData("73bfedfa-3d86-4e37-8677-bfb20b74ad95", "David", "Thomas", "dtomas@gmail.com")]
        // public async Task AddCurrent_should_return_member_with_qr_code(string objectId, string givenName, string surname, string email)
        // {
        //     const string initUrl = "https://dev.tipcat.net/52AS2BS9AS/pay";
        //     var microsoftGraphClientMock = new Mock<IUserManagementClient>();
        //     microsoftGraphClientMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(new UserContext(givenName, surname, email));
        //     var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
        //     qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(Result.Success(initUrl));

        //     var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, qrCodeGeneratorMock.Object, _invitationService);

        //     var (_, isFailure, member) = await service.AddCurrent(objectId);

        //     Assert.False(isFailure);
        //     Assert.Equal(initUrl, member.QrCodeUrl);
        // }


        // [Theory]
        // [InlineData("4ac0fbad-c7bb-433d-96a3-47a11716f2d5", "Clark", "Owens", "clark11@hotmail.com")]
        // public async Task AddCurrent_should_return_empty_qr_code_when_amazons3_unreachable(string objectId, string givenName, string surname, string email)
        // {
        //     var microsoftGraphClientMock = new Mock<IUserManagementClient>();
        //     microsoftGraphClientMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(new UserContext(givenName, surname, email));
        //     var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
        //     qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(Result.Failure<string>("Amazon S3 service unreachable."));

        //     var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, qrCodeGeneratorMock.Object, _invitationService);

        //     var (_, isFailure, member) = await service.AddCurrent(objectId);

        //     Assert.False(isFailure);
        //     Assert.True(string.IsNullOrEmpty(member.QrCodeUrl));
        // }

        [Fact]
        public async Task Regenerate_should_return_error_when_current_member_does_not_belongs_to_target_account()
        {
            const string initUrl = "https://dev.tipcat.net/52AS2BS9AS/pay";
            var memberContext = new MemberContext(4, string.Empty, 5, null);
            const int memberId = 1;
            const int accountId = 2;

            var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
            qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(initUrl));

            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, qrCodeGeneratorMock.Object, _invitationService);

            var (_, isFailure, member) = await service.RegenerateQr(memberContext, memberId, accountId);

            Assert.True(isFailure);
            Assert.True(string.IsNullOrEmpty(member.QrCodeUrl));
        }


        [Fact]
        public async Task Regenerate_should_return_empty_qr_code_when_amazons3_unreachable()
        {
            const int memberId = 1;
            const int accountId = 5;

            var memberContext = new MemberContext(memberId, string.Empty, accountId, null);
            var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
            qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<string>("Amazon S3 service reachable."));

            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, qrCodeGeneratorMock.Object, _invitationService);

            var (_, isFailure, member) = await service.RegenerateQr(memberContext, memberId, accountId);

            Assert.False(isFailure);
            Assert.True(string.IsNullOrEmpty(member.QrCodeUrl));
        }


        [Fact]
        public async Task Get_all_should_return_all_account_members()
        {
            const int accountId = 9;
            var membersCount = _members
                .Count(m => m.AccountId == accountId);

            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var members = await service.Get(accountId);

            Assert.Equal(membersCount, members.Count);
            Assert.All(members, member =>
            {
                Assert.Equal(accountId, member.AccountId);
            });
        }


        [Fact]
        public async Task GetCurrent_should_throws_exception_when_member_context_is_null()
        {
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            await Assert.ThrowsAsync<NullReferenceException>(async () => await service.GetCurrent(null));
        }


        [Fact]
        public async Task GetCurrent_should_return_error_when_member_is_not_found()
        {
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.GetCurrent(new MemberContext(0, "hash", 0, string.Empty));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task GetCurrent_should_return_member_when_member_is_found()
        {
            const int memberId = 1;
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure, memberInfoResponse) = await service.GetCurrent(new MemberContext(memberId, "hash", 0, string.Empty));

            Assert.False(isFailure);
            Assert.Equal(memberInfoResponse.Id, memberId);
        }


        [Fact]
        public async Task Remove_should_return_error_when_current_and_target_members_are_same()
        {
            const int memberIs = 1;
            var memberContext = new MemberContext(memberIs, string.Empty, null, null);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Remove(memberContext, memberIs, 3);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_return_error_when_current_member_does_not_belong_to_target_account()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Remove(memberContext, 88, 3);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_return_error_when_target_member_does_not_belong_to_target_account()
        {
            var memberContext = new MemberContext(26, string.Empty, 9, null);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Remove(memberContext, 88, 9);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_return_error_when_target_member_is_manager()
        {
            var memberContext = new MemberContext(26, string.Empty, 9, null);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Remove(memberContext, 89, 9);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_remove_member()
        {
            var memberContext = new MemberContext(26, string.Empty, 9, null);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Remove(memberContext, 90, 9);

            Assert.False(isFailure);
        }


        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task Update_return_error_when_member_id_is_null_or_zero(int? memberId)
        {
            var request = new MemberRequest(memberId, null, string.Empty, string.Empty, string.Empty, MemberPermissions.None);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task Update_return_error_when_account_id_is_null_or_zero(int? accountId)
        {
            var request = new MemberRequest(1, accountId, string.Empty, string.Empty, string.Empty, MemberPermissions.None);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_first_name_is_empty()
        {
            var request = new MemberRequest(15, 5, string.Empty, string.Empty, string.Empty, MemberPermissions.None);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_last_name_is_empty()
        {
            var request = new MemberRequest(15, 5, "Krin", string.Empty, string.Empty, MemberPermissions.None);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_permissions_are_nor_set()
        {
            var request = new MemberRequest(15, 5, "Krin", "Anderson", string.Empty, MemberPermissions.None);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_current_member_does_not_belong_to_account()
        {
            var request = new MemberRequest(14, 5, "Krin", "Anderson", string.Empty, MemberPermissions.Manager);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(17, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_target_member_does_not_belong_to_account()
        {
            var request = new MemberRequest(14, 6, "Krin", "Anderson", string.Empty, MemberPermissions.Manager);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(17, "hash", 6, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_member_is_not_found()
        {
            var request = new MemberRequest(14, 5, "Krin", "Anderson", string.Empty, MemberPermissions.Manager);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(17, "hash", 5, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_error_when_position_property_is_too_long()
        {
            const string firstName = "Krin";
            const string lastName = "Anderson";
            const string position = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc eget dui convallis, pellentesque tortor eget, consectetur laoreet.";
            var request = new MemberRequest(17, 5, firstName, lastName, string.Empty, MemberPermissions.Manager, position);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, isFailure) = await service.Update(new MemberContext(17, "hash", 5, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_update_member()
        {
            const string firstName = "Krin";
            const string lastName = "Anderson";
            var request = new MemberRequest(17, 5, firstName, lastName, string.Empty, MemberPermissions.Manager);
            var service = new MemberService(_stripeAccountService, new NullLoggerFactory(), _aetherDbContext, _userManagementClient, _qrCodeGenerator, _invitationService);

            var (_, _, member) = await service.Update(new MemberContext(17, "hash", 5, string.Empty), request);

            Assert.Equal(member.FirstName, firstName);
            Assert.Equal(member.LastName, lastName);
        }


        private readonly IEnumerable<Member> _members = new[]
        {
            new Member
            {
                Id = 1,
                IdentityHash = "hash",
                FirstName = "Elizabeth",
                LastName = "Omara",
                AccountId = 5,
                Email = null,
                FacilityId = 1,
                Permissions = MemberPermissions.Manager
            },
            new Member
            {
                Id = 2,
                IdentityHash = "hash",
                FirstName = "Anna",
                LastName = "Omara",
                AccountId = 5,
                FacilityId = 1,
                Email = null,
                Permissions = MemberPermissions.Employee
            },
            new Member
            {
                Id = 7,
                IdentityHash = "e6b02f80930f7e255776dbc8934a7eced41ea1db65f845a00d9442adf846f2dd",
                FirstName = "Ian",
                LastName = "Moss",
                Email = null,
                Permissions = MemberPermissions.Manager
            },
            new Member
            {
                Id = 17,
                AccountId = 5,
                IdentityHash = "hash",
                FirstName = "Zachary",
                LastName = "White",
                Email = null,
                Permissions = MemberPermissions.Manager
            },
            new Member
            {
                Id = 25,
                AccountId = 8,
                IdentityHash = "hash",
                FirstName = "Zachary",
                LastName = "White",
                Email = null,
                Permissions = MemberPermissions.Manager
            },
            new Member
            {
                Id = 26,
                AccountId = 9,
                IdentityHash = "hash",
                FirstName = "Zachary",
                LastName = "White",
                Email = null,
                Permissions = MemberPermissions.Manager
            },
            new Member
            {
                Id = 89,
                AccountId = 9,
                IdentityHash = "hash",
                FirstName = "Zachary",
                LastName = "White",
                Email = null,
                Permissions = MemberPermissions.Manager
            },
            new Member
            {
                Id = 90,
                AccountId = 9,
                IdentityHash = "hash",
                FirstName = "Zachary",
                LastName = "White",
                Email = null,
                Permissions = MemberPermissions.Employee
            },
            new Member
            {
                Id = 91,
                AccountId = 9,
                IdentityHash = "hash",
                FirstName = "Zachary",
                LastName = "White",
                Email = "existing@email.com",
                Permissions = MemberPermissions.Employee
            }
        };


        private readonly IEnumerable<Account> _accounts = new[]
        {
            new Account
            {
                Id = 1,
                IsActive = false
            },
            new Account
            {
                Id = 2,
                IsActive = true
            }
        };

        private readonly IEnumerable<Facility> _facilities = new[]
        {
            new Facility
            {
                Id = 1,
                Name = "Default facility",
                AccountId = 1
            },
            new Facility
            {
                Id = 2,
                Name = "Test facility for first account",
                AccountId = 1
            },
            new Facility
            {
                Id = 3,
                Name = "Default facility",
                AccountId = 2
            }
        };


        private readonly AetherDbContext _aetherDbContext;
        private readonly IInvitationService _invitationService;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly IStripeAccountService _stripeAccountService;
        private readonly IUserManagementClient _userManagementClient;
    }
}
