using System;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
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


        public Task<Result<int>> AddDefault(int accountId, CancellationToken cancellationToken)
        {
            return Result.Success()
                .EnsureTargetAccountHasNoDefault(_context, accountId, cancellationToken)
                .Bind(CreateDefaultFacility);

            async Task<Result<int>> CreateDefaultFacility()
            {
                var now = DateTime.UtcNow;

                var defualtFacility = new Facility
                {
                    Name = "Default facility",
                    AccountId = accountId,
                    Created = now,
                    Modified = now,
                    State = ModelStates.Active
                    // TODO: Need some mark to define default
                };

                _context.Facilities.Add(defualtFacility);
                await _context.SaveChangesAsync(cancellationToken);

                return defualtFacility.Id;
            }
        }


        public async Task<Result<int>> TransferMember(int memberId, int facilityId, CancellationToken cancellationToken)
        {
            var member = await _context.Members
                .SingleAsync(m => m.Id == memberId);

            member.FacilityId = facilityId;

            _context.Members.Update(member);
            await _context.SaveChangesAsync(cancellationToken);

            return memberId;
        }


        private readonly AetherDbContext _context;

        private readonly ILogger<FacilityService> _logger;
    }
}