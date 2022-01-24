using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities.Validators;
using Microsoft.EntityFrameworkCore;

namespace TipCatDotNet.Api.Services.HospitalityFacilities;

public class FacilityService : IFacilityService
{
    public FacilityService(AetherDbContext context, IMemberService memberService)
    {
        _context = context;
        _memberService = memberService;
    }


    public Task<Result<FacilityResponse>> Add(MemberContext memberContext, FacilityRequest request, CancellationToken cancellationToken)
    {
        return Validate()
            .Bind(() => AddInternal(new Facility
            {
                AccountId = (int)request.AccountId!,
                Address = request.Address,
                Name = request.Name,
                SessionEndTime = request.SessionEndTime
            }, cancellationToken))
            .Bind(facilityId => GetFacility(request.AccountId!.Value, facilityId, cancellationToken));


        Result Validate()
        {
            var validator = new FacilityRequestValidator(memberContext, _context);
            return validator.ValidateAdd(request).ToResult();
        }
    }


    public Task<Result<int>> AddDefault(int accountId, string name, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(() => AddInternal(new Facility
            {
                AccountId = accountId,
                Address = string.Empty,
                IsDefault = true,
                Name = name,
                SessionEndTime = default
            }, cancellationToken));


        Result Validate()
        {
            var validator = new FacilityRequestValidator(_context);
            return validator.ValidateAddDefault(FacilityRequest.CreateWithAccountIdAndName(accountId, name)).ToResult();
        }
    }


    public Task<List<FacilityResponse>> Get(int accountId, CancellationToken cancellationToken = default)
        => GetFacilities(accountId, cancellationToken);


    public Task<Result> TransferMember(MemberContext memberContext, int memberId, int facilityId, int accountId, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(Transfer);


        Result Validate()
        {
            var validator = new MemberTransferValidator(memberContext, _context);
            return validator.Validate((facilityId, memberId, accountId)).ToResult();
        }


        async Task<Result> Transfer()
        {
            var member = await _context.Members
                .SingleAsync(m => m.Id == memberId, cancellationToken);

            member.FacilityId = facilityId;

            _context.Members.Update(member);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }


    public Task<Result<FacilityResponse>> Update(MemberContext memberContext, FacilityRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(UpdateInternal)
            .Bind(() => GetFacility(request.AccountId!.Value, request.Id!.Value, cancellationToken));


        Result Validate()
        {
            var validator = new FacilityRequestValidator(memberContext, _context);
            return validator.ValidateGetOrUpdate(request).ToResult();
        }


        async Task<Result> UpdateInternal()
        {
            var targetFacility = await _context.Facilities
                .SingleOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (targetFacility is null)
                return Result.Failure($"The facility with ID {request.Id} was not found.");

            targetFacility.Address = request.Address;
            targetFacility.Name = request.Name;
            targetFacility.Modified = DateTime.UtcNow;
            targetFacility.SessionEndTime = request.SessionEndTime;

            _context.Facilities.Update(targetFacility);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }


    private async Task<Result<int>> AddInternal(Facility facility, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        facility.Created = now;
        facility.IsActive = true;
        facility.Modified = now;

        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync(cancellationToken);

        return facility.Id;
    }


    private async Task<Result<FacilityResponse>> GetFacility(int accountId, int facilityId, CancellationToken cancellationToken)
    {
        var facility = (await GetFacilities(accountId, cancellationToken))
            .SingleOrDefault(f => f.Id == facilityId);

        if (!facility!.Equals(default))
            return facility;

        return Result.Failure<FacilityResponse>($"The facility with ID {facilityId} was not found.");
    }


    private async Task<List<FacilityResponse>> GetFacilities(int accountId, CancellationToken cancellationToken)
    {
        var facilities = await _context.Facilities
            .Where(f => f.AccountId == accountId)
            .Select(FacilityProjection())
            .ToListAsync(cancellationToken);

        var members = (await _memberService.Get(accountId, cancellationToken))
            .GroupBy(x => x.FacilityId!) // Assume FacilityId must not be null here, because at that stage the manager already have an account
            .ToDictionary(x => x.Key!.Value, x => x.ToList());

        var result = new List<FacilityResponse>(facilities.Count);
        foreach (var facility in facilities)
        {
            members.TryGetValue(facility.Id, out var facilityMembers);
            result.Add(new FacilityResponse(facility, facilityMembers));
        }

        return result;
    }


    private static Expression<Func<Facility, FacilityResponse>> FacilityProjection()
        => facility => new FacilityResponse(facility.Id, facility.Name, facility.Address, facility.AccountId, facility.AvatarUrl, null,
            facility.SessionEndTime);


    private readonly AetherDbContext _context;
    private readonly IMemberService _memberService;
}