using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TipCatDotNet.Api.Infrastructure;

public static class DbContextExtensions
{
    public static void DetachEntities(this DbContext context)
    {
        var entries = context.ChangeTracker?.Entries()
                .Where(e => e.State != EntityState.Detached)
            ?? new List<EntityEntry>();

        foreach (var entry in entries)
        {
            /*if (entry.Entity is null)
                continue;*/

            entry.State = EntityState.Detached;
        }
    }
}