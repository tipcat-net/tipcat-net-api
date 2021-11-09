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

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
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
                .Bind(AddInternal)
                .Bind(facilityId => GetFacility(request.AccountId!.Value ,facilityId, cancellationToken));


            Result Validate()
            {
                var validator = new FacilityRequestValidator(memberContext, _context);
                return validator.ValidateAdd(request).ToResult();
            }


            async Task<Result<int>> AddInternal()
            {
                var now = DateTime.UtcNow;

                var newFacility = new Facility
                {
                    Name = request.Name,
                    AccountId = (int)request.AccountId!,
                    Created = now,
                    Modified = now,
                    State = ModelStates.Active
                };

                _context.Facilities.Add(newFacility);
                await _context.SaveChangesAsync(cancellationToken);

                return newFacility.Id;
            }
        }


        public Task<Result<int>> AddDefault(int accountId, CancellationToken cancellationToken)
        {
            return Validate()
                .Bind(AddDefaultInternal);


            Result Validate()
            {
                var validator = new FacilityRequestValidator(_context);
                return validator.ValidateAddDefault(FacilityRequest.CreateWithAccountId(accountId)).ToResult();
            }


            async Task<Result<int>> AddDefaultInternal()
            {
                var now = DateTime.UtcNow;

                var defaultFacility = new Facility
                {
                    Name = "Default facility",
                    AccountId = accountId,
                    Created = now,
                    Modified = now,
                    State = ModelStates.Active,
                    IsDefault = true
                };

                _context.Facilities.Add(defaultFacility);
                await _context.SaveChangesAsync(cancellationToken);

                return defaultFacility.Id;
            }
        }


        public Task<Result<int>> TransferMember(int memberId, int facilityId, CancellationToken cancellationToken)
        {
            return Validate()
                .Bind(TransferInternal);


            Result Validate()
            {
                var validator = new FacilityRequestValidator(MemberContext.CreateEmpty() with {Id = memberId}, _context);
                return validator.ValidateTransferMember(new FacilityRequest(facilityId, string.Empty, null)).ToResult();
            }


            async Task<Result<int>> TransferInternal()
            {
                var member = await _context.Members
                    .SingleAsync(m => m.Id == memberId, cancellationToken);

                member.FacilityId = facilityId;

                _context.Members.Update(member);
                await _context.SaveChangesAsync(cancellationToken);

                return memberId;
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

                targetFacility.Name = request.Name;
                targetFacility.Modified = DateTime.UtcNow;

                _context.Facilities.Update(targetFacility);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }


        public Task<List<FacilityResponse>> Get(int accountId, CancellationToken cancellationToken = default) 
            => GetFacilities(accountId, cancellationToken);


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
                result.Add(new FacilityResponse(facility.Id, facility.Name, facility.AccountId, facilityMembers!));
            }

            return result;
        }


        private Expression<Func<Facility, FacilityResponse>> FacilityProjection()
            => facility => new FacilityResponse(facility.Id, facility.Name, facility.AccountId, new List<MemberResponse>());


        private readonly AetherDbContext _context;
        private readonly IMemberService _memberService;
    }
}