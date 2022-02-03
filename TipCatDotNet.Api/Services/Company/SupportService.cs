using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MailSender;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Company;
using TipCatDotNet.Api.Models.Company.Validators;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Mailing;
using TipCatDotNet.Api.Options;

namespace TipCatDotNet.Api.Services.Company;

public class SupportService : ISupportService
{
    public SupportService(IOptionsMonitor<SupportOptions> options, AetherDbContext context, ICompanyInfoService companyInfoService, IMailSender mailSender)
    {
        _companyInfoService = companyInfoService;
        _context = context;
        _mailSender = mailSender;
        _options = options.CurrentValue;
    }


    public Task<Result> SendRequest(MemberContext memberContext, SupportRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Map(GetMemberInfo)
            .Bind(SendMessageToSupport)
            .Bind(SendMessageToMember);


        Result Validate()
        {
            var validator = new SupportRequestValidator(memberContext);
            return validator.Validate(request)
                .ToResult();
        }


        async Task<MemberInfo> GetMemberInfo()
            => await _context.Members
                .Where(m => m.Id == memberContext.Id)
                .Select(m => new MemberInfo(m.FirstName, m.LastName))
                .SingleAsync(cancellationToken);


        async Task<Result<MemberInfo>> SendMessageToSupport(MemberInfo memberInfo)
        {
            var (_, isFailure, error) = await _mailSender.Send(_options.SupportRequestToSupportTemplateId, _options.SupportEmailAddress,
                new SupportRequestToSupportEmail(memberContext.Id, BuildFullName(memberInfo), request.Content, _companyInfoService.Get()));

            if (isFailure)
                return Result.Failure<MemberInfo>($"Can't send the request to support. Reason: {error}");

            return memberInfo;
        }


        Task<Result> SendMessageToMember(MemberInfo memberInfo)
            => _mailSender.Send(_options.SupportRequestToMemberTemplateId, memberContext.Email!,
                new SupportRequestToMemberEmail(memberInfo.FirstName, request.Content, _companyInfoService.Get()));


        string BuildFullName(MemberInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.FirstName))
                return "TODO"; // TODO: handle that case

            if (!string.IsNullOrWhiteSpace(info.FirstName) && !string.IsNullOrWhiteSpace(info.LastName))
                return $"{info.FirstName} {info.LastName}";

            return info.FirstName;
        }
    }


    private readonly ICompanyInfoService _companyInfoService;
    private readonly AetherDbContext _context;
    private readonly IMailSender _mailSender;
    private readonly SupportOptions _options;


    internal readonly struct MemberInfo
    {
        internal MemberInfo(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }


        internal string FirstName { get; }
        internal string LastName { get; }
    }
}