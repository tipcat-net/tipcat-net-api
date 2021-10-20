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
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;
using TipCatDotNet.Api.Models.HospitalityFacilities.Validators;
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
            return Validate()
                .Bind(AddInternal)
                .Bind(facilityId => GetFacility(facilityId, cancellationToken));


            Result Validate()
            {
                var validator = new FacilityRequestValidator(memberContext, _context);
                var validationResult = validator.ValidateAdd(request);
                return validationResult.ToResult();
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
            return Validate()
                .Bind(TransferInternal);


            Result Validate()
            {
                var validator = new FacilityRequestValidator(new MemberContext(memberId, string.Empty, null, null), _context);
                var validationResult = validator.ValidateTransferMember(new FacilityRequest(facilityId, string.Empty, null));
                return validationResult.ToResult();
            }


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
            return Validate()
                .Bind(UpdateInternal)
                .Bind(() => GetFacility((int)request.Id!, cancellationToken));


            Result Validate()
            {
                var validator = new FacilityRequestValidator(memberContext, _context);
                var validationResult = validator.ValidateGetOrUpdate(request);
                return validationResult.ToResult();
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


        public Task<Result<FacilityResponse>> Get(MemberContext memberContext, int facilityId, int accountId, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(() => GetFacility(facilityId, cancellationToken));

            Result Validate()
            {
                var validator = new FacilityRequestValidator(memberContext, _context);
                var validationResult = validator.ValidateGetOrUpdate(new FacilityRequest(facilityId, string.Empty, accountId));
                return validationResult.ToResult();
            }
        }


        public Task<Result<List<FacilityResponse>>> Get(MemberContext memberContext, int accountId, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(() => GetFacilities(accountId, cancellationToken));

            Result Validate()
            {
                var validator = new FacilityRequestValidator(memberContext, _context);
                var validationResult = validator.ValidateGetAll(new FacilityRequest(null, string.Empty, accountId));
                return validationResult.ToResult();
            }
        }


        public Task<Result<SlimFacilityResponse>> GetSlim(MemberContext memberContext, int facilityId, int accountId, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(() => GetSlimFacility(facilityId, accountId, cancellationToken));

            Result Validate()
            {
                var validator = new FacilityRequestValidator(memberContext, _context);
                var validationResult = validator.ValidateGetOrUpdate(new FacilityRequest(facilityId, string.Empty, accountId));
                return validationResult.ToResult();
            }
        }


        public Task<Result<List<SlimFacilityResponse>>> GetSlim(MemberContext memberContext, int accountId, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(() => GetSlimFacilities(accountId, cancellationToken));

            Result Validate()
            {
                var validator = new FacilityRequestValidator(memberContext, _context);
                var validationResult = validator.ValidateGetAll(new FacilityRequest(null, string.Empty, accountId));
                return validationResult.ToResult();
            }
        }


        private Result Validate(MemberContext? memberContext, FacilityRequest request)
        {
            var validator = new FacilityRequestValidator(memberContext, _context);
            var validationResult = validator.Validate(request);
            return validationResult.ToResult();
        }


        private async Task<Result<FacilityResponse>> GetFacility(int facilityId, CancellationToken cancellationToken)
        {
            var facility = await _context.Facilities
                .Where(f => f.Id == facilityId)
                .Select(FacilityProjection())
                .SingleOrDefaultAsync(cancellationToken);

            if (!facility.Equals(default))
                return facility;

            return Result.Failure<FacilityResponse>($"The facility with ID {facilityId} was not found.");
        }


        private async Task<Result<SlimFacilityResponse>> GetSlimFacility(int facilityId, int accountId, CancellationToken cancellationToken)
        {
            var (_, isFailure, facilities, error) = await GetSlimFacilities(accountId, cancellationToken);

            if (isFailure)
            {
                return Result.Failure<SlimFacilityResponse>(error);
            }

            var facility = facilities.SingleOrDefault(f => f.Id == facilityId);

            if (!facility!.Equals(default))
                return facility;

            return Result.Failure<SlimFacilityResponse>($"The facility with ID {facilityId} was not found.");
        }


        private async Task<Result<List<FacilityResponse>>> GetFacilities(int accountId, CancellationToken cancellationToken)
            => await _context.Facilities
                .Where(f => f.AccountId == accountId)
                .Select(FacilityProjection())
                .ToListAsync(cancellationToken);


        private async Task<Result<List<SlimFacilityResponse>>> GetSlimFacilities(int accountId, CancellationToken cancellationToken)
            => await _context.Facilities
                .Where(f => f.AccountId == accountId)
                .Select(SlimFacilityProjection())
                .ToListAsync(cancellationToken);


        private static Expression<Func<Facility, FacilityResponse>> FacilityProjection()
            => facility => new FacilityResponse(facility.Id, facility.Name, facility.AccountId);


        private Expression<Func<Facility, SlimFacilityResponse>> SlimFacilityProjection()
            => facility => new SlimFacilityResponse(
                       facility.Id,
                       facility.Name,
                       _context.Members
                           .Where(m => m.FacilityId == facility.Id)
                           .Select(MemberProjection())
                           .ToList()
               );


        private static Expression<Func<Member, MemberResponse>> MemberProjection()
            => member => new MemberResponse(member.Id, member.AccountId, member.FirstName, member.LastName, member.Email, member.MemberCode, member.QrCodeUrl,
                member.Permissions);


        private readonly AetherDbContext _context;

        private readonly ILogger<FacilityService> _logger;
    }
}