using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TipCatDotNet.Api.Infrastructure.Converters.EntityFramework;

public class DateTimeKindConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeKindConverter()
        : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    { }
}