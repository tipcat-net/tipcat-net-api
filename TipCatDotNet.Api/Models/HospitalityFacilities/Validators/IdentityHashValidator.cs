using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TipCatDotNet.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class IdentityHashValidator : AbstractValidator<string?>
    {
        public IdentityHashValidator(AetherDbContext context)
        {
            _context = context;

            RuleFor(x => x)
                .NotEmpty()
                .MustAsync(CheckIfMemberAlreadyAdded)
                .WithMessage("Another user was already added from this token data.");
        }

        private async Task<bool> CheckIfMemberAlreadyAdded(string? identityHash, CancellationToken cancellationToken = default)
                => !await _context.Members
                    .Where(m => m.IdentityHash == identityHash)
                    .AnyAsync(cancellationToken);


        private readonly AetherDbContext _context;
    }
}