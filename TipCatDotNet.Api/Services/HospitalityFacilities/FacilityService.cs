using System;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class FacilityService : IFacilityService
    {
        public FacilityService(ILoggerFactory loggerFactory, AetherDbContext context)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<FacilityService>();
        }


        public async Task<Result<int>> AddDefaultFacility(int accountId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var defualtFacility = new Facility
            {
                Name = "Default facility",
                AccountId = accountId,
                Created = now,
                Modified = now,
                State = ModelStates.Active
            };

            _context.Facilities.Add(defualtFacility);
            await _context.SaveChangesAsync(cancellationToken);

            return accountId;
        }


        private readonly AetherDbContext _context;

        private readonly ILogger<FacilityService> _logger;
    }
}