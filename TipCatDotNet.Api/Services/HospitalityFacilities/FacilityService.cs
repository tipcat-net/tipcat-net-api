using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Infrastructure.FunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class FacilityService : IFacilityService
    {
        public FacilityService(ILoggerFactory loggerFactory, AetherDbContext context)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<FacilityService>();
        }


        public Task<Result<FacilityResponse>> Add(MemberContext memberContext, FacilityRequest request, CancellationToken cancellationToken)
        {
            return Result.Success()
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, request.AccountId)
                .Bind(AddInternal)
                .Bind(facilityId => GetFacility(facilityId, cancellationToken));


            // Result Validate()
            // {
            //     var validator = new FacilityRequestAddValidator();
            //     var validationResult = validator.Validate(request);
            //     if (!validationResult.IsValid)
            //         return validationResult.ToFailureResult();

            //     return Result.Success();
            // }


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
            return Result.Success()
                .EnsureTargetAccountHasNoDefault(_context, accountId, cancellationToken)
                .Bind(AddDefaultInternal);

            async Task<Result<int>> AddDefaultInternal()
            {
                var now = DateTime.UtcNow;

                var defualtFacility = new Facility
                {
                    Name = "Default facility",
                    AccountId = accountId,
                    Created = now,
                    Modified = now,
                    State = ModelStates.Active,
                    IsDefault = true
                };

                _context.Facilities.Add(defualtFacility);
                await _context.SaveChangesAsync(cancellationToken);

                return defualtFacility.Id;
            }
        }


        public Task<Result<int>> TransferMember(int memberId, int facilityId, CancellationToken cancellationToken)
        {
            return Result.Success()
                .EnsureTargetMemberFacilityIsEqualToActualOne(_context, memberId, facilityId, cancellationToken)
                .Bind(TransferInternal);

            async Task<Result<int>> TransferInternal()
            {
                var member = await _context.Members
                    .SingleAsync(m => m.Id == memberId);

                member.FacilityId = facilityId;

                _context.Members.Update(member);
                await _context.SaveChangesAsync(cancellationToken);

                return memberId;
            }
        }


        public Task<Result<FacilityResponse>> Update(MemberContext memberContext, FacilityRequest request, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, request.AccountId)
                .EnsureTargetFacilityBelongsToAccount(_context, request.Id, request.AccountId, cancellationToken)
                .Bind(UpdateInternal)
                .Bind(() => GetFacility((int)request.Id!, cancellationToken));


            // Result Validate()
            // {
            //     var validator = new FacilityRequestUpdateValidator();
            //     var validationResult = validator.Validate(request);
            //     if (!validationResult.IsValid)
            //         return validationResult.ToFailureResult();

            //     return Result.Success();
            // }


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


        public Task<Result<FacilityResponse>> Get(MemberContext memberContext, int facilityId, int accountId, CancellationToken cancellationToken = default)
            => Result.Success()
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, accountId)
                .EnsureTargetFacilityBelongsToAccount(_context, facilityId, accountId, cancellationToken)
                .Bind(() => GetFacility(facilityId, cancellationToken));


        public Task<Result<List<FacilityResponse>>> Get(MemberContext memberContext, int accountId, CancellationToken cancellationToken = default)
            => Result.Success()
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, accountId)
                .Bind(() => GetFacilities(accountId, cancellationToken));


        private async Task<Result<FacilityResponse>> GetFacility(int facilityId, CancellationToken cancellationToken)
        {
            var member = await _context.Facilities
                .Where(f => f.Id == facilityId)
                .Select(FacilityProjection())
                .SingleOrDefaultAsync(cancellationToken);

            if (!member.Equals(default))
                return member;

            return Result.Failure<FacilityResponse>($"The facility with ID {facilityId} was not found.");
        }


        private async Task<Result<List<FacilityResponse>>> GetFacilities(int accountId, CancellationToken cancellationToken)
            => await _context.Facilities
                .Where(f => f.AccountId == accountId)
                .Select(FacilityProjection())
                .ToListAsync(cancellationToken);


        private static Expression<Func<Facility, FacilityResponse>> FacilityProjection()
            => facility => new FacilityResponse(facility.Id, facility.Name, facility.AccountId);


        private readonly AetherDbContext _context;

        private readonly ILogger<FacilityService> _logger;
    }
}