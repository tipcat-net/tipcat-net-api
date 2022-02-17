using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Infrastructure.FunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Models.Preferences;
using static System.Text.Json.Nodes.JsonNode;

namespace TipCatDotNet.Api.Services.Preferences;

public class PreferencesService : IPreferencesService
{
    public PreferencesService(AetherDbContext context)
    {
        _context = context;
    }


    public Task<Result<PreferencesResponse>> AddOrUpdate(MemberContext memberContext, PreferencesRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return ValidateJson()
            .BindWithTransaction(_context, () => UpdateMemberPreferencesAndReturnPermissions()
                .Bind(UpdateAccountPreferences))
            .Map(() => Get(memberContext, cancellationToken));


        Result ValidateJson()
        {
            try
            {
                Parse(request.ApplicationPreferences);
                return Result.Success();
            }
            catch
            {
                return Result.Failure("Can't parse application preferences. Probably, that's not a valid JSON.");
            }
        }


        async Task<Result> UpdateAccountPreferences(MemberPermissions permissions)
        {
            if (!(permissions.HasFlag(MemberPermissions.Manager) | permissions.HasFlag(MemberPermissions.Supervisor)))
                return Result.Success();

            var account = await _context.Accounts
                .SingleAsync(a => a.Id == memberContext.AccountId, cancellationToken);

            account.Preferences = request.ServerSidePreferences;
            account.Modified = now;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync(cancellationToken);
            _context.DetachEntities();

            return Result.Success();
        }
        

        async Task<Result<MemberPermissions>> UpdateMemberPreferencesAndReturnPermissions()
        {
            var member = await _context.Members
                .SingleAsync(m => m.Id == memberContext.Id, cancellationToken);
        
            member.ApplicationPreferences = request.ApplicationPreferences;
            member.Modified = now;
            
            _context.Members.Update(member);
            await _context.SaveChangesAsync(cancellationToken);
            _context.DetachEntities();

            return member.Permissions;
        }
    }


    public async Task<PreferencesResponse> Get(MemberContext memberContext, CancellationToken cancellationToken = default)
    {
        var applicationPreferences = await _context.Members
            .Where(m => m.Id == memberContext.Id)
            .Select(m => m.ApplicationPreferences)
            .SingleAsync(cancellationToken);

        var serverSidePreferences = await _context.Accounts
            .Where(a => a.Id == memberContext.AccountId)
            .Select(a => a.Preferences)
            .SingleOrDefaultAsync(cancellationToken) ?? DefaultServerSidePreferences;

        return new PreferencesResponse(serverSidePreferences, applicationPreferences);
    }


    // The application must know a structure of server-side preferences, so we have to materialize them here.
    // Also I assume, server-side preferences are equal to account preferences so far.
    private static AccountPreferences DefaultServerSidePreferences => new();


    private readonly AetherDbContext _context;
}