using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Graph;
using Moq;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;
using TipCatDotNet.Api.Services.Graph;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.ApiTests.Utils;
using Xunit;

namespace TipCatDotNet.ApiTests
{
    public class MemberServiceTests
    {
        public MemberServiceTests()
        {
            var aetherDbContextMock = MockContextFactory.Create();
            aetherDbContextMock.Setup(c => c.Members).Returns(DbSetMockProvider.GetDbSetMock(_members));
            aetherDbContextMock.Setup(c => c.Accounts).Returns(DbSetMockProvider.GetDbSetMock(_accounts));

            _aetherDbContext = aetherDbContextMock.Object;

            var microsoftGraphClientMock = new Mock<IMicrosoftGraphClient>();
            microsoftGraphClientMock.Setup(c => c.InviteMember(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Invitation
                {
                    InviteRedeemUrl = string.Empty
                });

            _microsoftGraphClient = microsoftGraphClientMock.Object;

            var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
            qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<string>());

            _qrCodeGenerator = qrCodeGeneratorMock.Object;
        }


        [Fact]
        public async Task Add_should_return_error_when_first_name_is_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Add(memberContext, new MemberRequest());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_last_name_is_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, null, "Angela", string.Empty, null, MemberPermissions.None);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_request_permissions_are_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, null, "Angela", "Carey", null, MemberPermissions.None);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_account_id_is_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, null, "Angela", "Carey", null, MemberPermissions.Employee);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_request_emails_is_empty()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var memberRequest = new MemberRequest(null, 5, "Angela", "Carey", null, MemberPermissions.Employee);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_current_member_does_not_belongs_to_target_account()
        {
            var memberContext = new MemberContext(1, string.Empty, 3, null);
            var memberRequest = new MemberRequest(null, 5, "Angela", "Carey", "AngelaDCarey@armyspy.com", MemberPermissions.Employee);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_error_when_account_already_has_manager()
        {
            var memberContext = new MemberContext(1, string.Empty, 8, null);
            var memberRequest = new MemberRequest(null, 8, "Angela", "Carey", "AngelaDCarey@armyspy.com", MemberPermissions.Manager);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Add(memberContext, memberRequest);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_should_return_member()
        {
            const int accountId = 5;
            const string firstName = "Angela";
            const string lastName = "Carey";
            var memberContext = new MemberContext(1, string.Empty, accountId, null);
            var memberRequest = new MemberRequest(null, accountId, firstName, lastName, "AngelaDCarey@armyspy.com", MemberPermissions.Employee);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, _, member) = await service.Add(memberContext, memberRequest);

            Assert.Equal(firstName, member.FirstName);
            Assert.Equal(lastName, member.LastName);
            Assert.Equal(accountId, member.AccountId);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_token_id_is_null()
        {
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.AddCurrent(null);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_identity_hash_already_used()
        {
            const string objectId = "426af836-dcef-4b72-b99b-a92c3e0a41a3";
            var microsoftGraphClientMock = new Mock<IMicrosoftGraphClient>();
            microsoftGraphClientMock.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    GivenName = null
                });

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator);

            var (_, isFailure) = await service.AddCurrent(objectId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_users_given_name_is_null()
        {
            const string objectId = "73bfedfa-3d86-4e37-8677-bfb20b74ad95";
            var microsoftGraphClientMock = new Mock<IMicrosoftGraphClient>();
            microsoftGraphClientMock.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    GivenName = null
                });

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator);

            var (_, isFailure) = await service.AddCurrent(objectId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_users_surname_is_null()
        {
            const string objectId = "73bfedfa-3d86-4e37-8677-bfb20b74ad95";
            var microsoftGraphClientMock = new Mock<IMicrosoftGraphClient>();
            microsoftGraphClientMock.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    GivenName = "David",
                    Surname = null
                });

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator);

            var (_, isFailure) = await service.AddCurrent(objectId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task AddCurrent_should_return_error_when_email_is_null()
        {
            const string objectId = "73bfedfa-3d86-4e37-8677-bfb20b74ad95";
            var microsoftGraphClientMock = new Mock<IMicrosoftGraphClient>();
            microsoftGraphClientMock.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    GivenName = "David",
                    Surname = "Thomas",
                    Identities = new List<ObjectIdentity>
                    {
                        new()
                        {
                            IssuerAssignedId = null,
                            SignInType = MemberService.EmailSignInType
                        }
                    }
                });

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator);

            var (_, isFailure) = await service.AddCurrent(objectId);

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData("73bfedfa-3d86-4e37-8677-bfb20b74ad95", "David", "Thomas", "dtomas@gmail.com")]
        [InlineData("4ac0fbad-c7bb-433d-96a3-47a11716f2d5", "Clark", "Owens", "clark11@hotmail.com")]
        public async Task AddCurrent_should_return_member(string objectId, string givenName, string surname, string email)
        {
            var microsoftGraphClientMock = new Mock<IMicrosoftGraphClient>();
            microsoftGraphClientMock.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    GivenName = givenName,
                    Surname = surname,
                    Identities = new List<ObjectIdentity>
                    {
                        new()
                        {
                            IssuerAssignedId = email,
                            SignInType = MemberService.EmailSignInType
                        }
                    }
                });

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, _qrCodeGenerator);

            var (_, isFailure, member) = await service.AddCurrent(objectId);

            Assert.False(isFailure);
            Assert.Equal(givenName, member.FirstName);
            Assert.Equal(surname, member.LastName);
            Assert.Equal(MemberPermissions.Manager, member.Permissions);
            Assert.Equal(email, member.Email);
        }


        [Theory]
        [InlineData("73bfedfa-3d86-4e37-8677-bfb20b74ad95", "David", "Thomas", "dtomas@gmail.com")]
        public async Task AddCurrent_should_return_member_with_qrcode(string objectId, string givenName, string surname, string email)
        {
            const string initUrl = "https://dev.tipcat.net/api/members/52AS2BS9AS/pay";
            var microsoftGraphClientMock = new Mock<IMicrosoftGraphClient>();
            microsoftGraphClientMock.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    GivenName = givenName,
                    Surname = surname,
                    Identities = new List<ObjectIdentity>
                    {
                        new()
                        {
                            IssuerAssignedId = email,
                            SignInType = MemberService.EmailSignInType
                        }
                    }
                });
            var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
            qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<string>(initUrl));

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, qrCodeGeneratorMock.Object);

            var (_, isFailure, member) = await service.AddCurrent(objectId);

            Assert.Equal(initUrl, member.QrCodeUrl);
        }


        [Theory]
        [InlineData("4ac0fbad-c7bb-433d-96a3-47a11716f2d5", "Clark", "Owens", "clark11@hotmail.com")]
        public async Task AddCurrent_should_return_empty_qrcode_when_amazons3_unreachable(string objectId, string givenName, string surname, string email)
        {
            var microsoftGraphClientMock = new Mock<IMicrosoftGraphClient>();
            microsoftGraphClientMock.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    GivenName = givenName,
                    Surname = surname,
                    Identities = new List<ObjectIdentity>
                    {
                        new()
                        {
                            IssuerAssignedId = email,
                            SignInType = MemberService.EmailSignInType
                        }
                    }
                });
            var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
            qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<string>("Amazon S3 service unreachable."));

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, microsoftGraphClientMock.Object, qrCodeGeneratorMock.Object);

            var (_, isFailure, member) = await service.AddCurrent(objectId);

            Assert.True(string.IsNullOrEmpty(member.QrCodeUrl));
        }

        [Fact]
        public async Task Regenerate_should_return_error_when_current_member_does_not_belongs_to_target_account()
        {
            var memberContext = new MemberContext(4, string.Empty, 5, null);
            const int memberId = 1;
            const int accountId = 2;

            var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
            qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<string>("Amazon S3 service reachable."));

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, qrCodeGeneratorMock.Object);

            var (_, isFailure) = await service.RegenerateQR(memberContext, memberId, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Regenerate_should_return_empty_qrcode_when_amazons3_unreachable()
        {
            var memberContext = new MemberContext(1, string.Empty, 2, null);
            const int memberId = 1;
            const int accountId = 2;

            var qrCodeGeneratorMock = new Mock<IQrCodeGenerator>();
            qrCodeGeneratorMock.Setup(c => c.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<string>("Amazon S3 service reachable."));

            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure, member) = await service.RegenerateQR(memberContext, memberId, accountId);

            Assert.True(string.IsNullOrEmpty(member.QrCodeUrl));
        }


        [Fact]
        public async Task Get_all_should_return_error_when_current_member_not_belong_to_target_account()
        {
            var context = new MemberContext(1, "hash", 7, string.Empty);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Get(context, 813);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_all_should_return_all_account_members()
        {
            const int accountId = 9;
            var membersCount = _members
                .Count(m => m.AccountId == accountId);

            var context = new MemberContext(1, "hash", accountId, string.Empty);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, _, members) = await service.Get(context, accountId);

            Assert.Equal(membersCount, members.Count);
            Assert.All(members, member =>
            {
                Assert.Equal(accountId, member.AccountId);
            });
        }


        [Theory]
        [InlineData(0)]
        [InlineData(null)]
        public async Task Get_should_return_error_when_current_member_not_belong_to_target_account(int? accountId)
        {
            var context = new MemberContext(1, "hash", accountId, string.Empty);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Get(context, 8, 13);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_return_error_when_target_member_does_not_belong_to_target_account()
        {
            const int accountId = 5;
            var context = new MemberContext(1, "hash", accountId, string.Empty);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Get(context, 16, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_return_error_when_target_member_does_not_exist()
        {
            const int accountId = 5;
            var context = new MemberContext(1, "hash", accountId, string.Empty);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Get(context, 15, accountId);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_should_return_member()
        {
            const int accountId = 5;
            const int memberId = 17;
            var context = new MemberContext(1, "hash", accountId, string.Empty);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, _, member) = await service.Get(context, memberId, accountId);

            Assert.Equal(member.Id, memberId);
            Assert.Equal(member.AccountId, accountId);
        }


        [Fact]
        public async Task GetCurrent_should_throws_exception_when_member_context_is_null()
        {
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            await Assert.ThrowsAsync<NullReferenceException>(async () => await service.GetCurrent(null));
        }


        [Fact]
        public async Task GetCurrent_should_return_error_when_member_is_not_found()
        {
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.GetCurrent(new MemberContext(0, "hash", 0, string.Empty));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task GetCurrent_should_return_member_when_member_is_found()
        {
            const int memberId = 1;
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure, memberInfoResponse) = await service.GetCurrent(new MemberContext(memberId, "hash", 0, string.Empty));

            Assert.False(isFailure);
            Assert.Equal(memberInfoResponse.Id, memberId);
        }


        [Fact]
        public async Task Remove_should_return_error_when_current_and_target_members_are_same()
        {
            const int memberIs = 1;
            var memberContext = new MemberContext(memberIs, string.Empty, null, null);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Remove(memberContext, memberIs, 3);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_return_error_when_current_member_does_not_belong_to_target_account()
        {
            var memberContext = new MemberContext(1, string.Empty, null, null);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Remove(memberContext, 88, 3);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_return_error_when_target_member_does_not_belong_to_target_account()
        {
            var memberContext = new MemberContext(26, string.Empty, 9, null);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Remove(memberContext, 88, 9);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_return_error_when_target_member_is_manager()
        {
            var memberContext = new MemberContext(26, string.Empty, 9, null);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Remove(memberContext, 89, 9);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Remove_should_remove_member()
        {
            var memberContext = new MemberContext(26, string.Empty, 9, null);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Remove(memberContext, 90, 9);

            Assert.False(isFailure);
        }


        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task Update_return_error_when_member_id_is_null_or_zero(int? memberId)
        {
            var request = new MemberRequest(memberId, null, string.Empty, string.Empty, string.Empty, MemberPermissions.None);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task Update_return_error_when_account_id_is_null_or_zero(int? accountId)
        {
            var request = new MemberRequest(1, accountId, string.Empty, string.Empty, string.Empty, MemberPermissions.None);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_first_name_is_empty()
        {
            var request = new MemberRequest(15, 5, string.Empty, string.Empty, string.Empty, MemberPermissions.None);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_last_name_is_empty()
        {
            var request = new MemberRequest(15, 5, "Krin", string.Empty, string.Empty, MemberPermissions.None);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_permissions_are_nor_set()
        {
            var request = new MemberRequest(15, 5, "Krin", "Anderson", string.Empty, MemberPermissions.None);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Update(new MemberContext(1, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_current_member_does_not_belong_to_account()
        {
            var request = new MemberRequest(14, 5, "Krin", "Anderson", string.Empty, MemberPermissions.Manager);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Update(new MemberContext(17, "hash", 0, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_target_member_does_not_belong_to_account()
        {
            var request = new MemberRequest(14, 6, "Krin", "Anderson", string.Empty, MemberPermissions.Manager);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Update(new MemberContext(17, "hash", 6, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_return_error_when_member_is_not_found()
        {
            var request = new MemberRequest(14, 5, "Krin", "Anderson", string.Empty, MemberPermissions.Manager);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

            var (_, isFailure) = await service.Update(new MemberContext(17, "hash", 5, string.Empty), request);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_should_update_member()
        {
            const string firstName = "Krin";
            const string lastName = "Anderson";
            var request = new MemberRequest(17, 5, firstName, lastName, string.Empty, MemberPermissions.Manager);
            var service = new MemberService(new NullLoggerFactory(), _aetherDbContext, _microsoftGraphClient, _qrCodeGenerator);

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
                Email = null,
                Permissions = MemberPermissions.Manager
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
            }
        };


        private readonly IEnumerable<Account> _accounts = Array.Empty<Account>();


        private readonly AetherDbContext _aetherDbContext;
        private readonly IMicrosoftGraphClient _microsoftGraphClient;
        private readonly IQrCodeGenerator _qrCodeGenerator;
    }
}
