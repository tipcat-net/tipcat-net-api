using System.Collections.Generic;

namespace TipCatDotNet.Api.Infrastructure;

public static class ConnectionStringBuilder
{
    public static string Build(Dictionary<string, string> dbOptions)
        => string.Format(ConnectionStringTemplate, dbOptions["host"], dbOptions["port"], dbOptions["username"], dbOptions["password"]);


    private const string ConnectionStringTemplate = "Server={0};Port={1};User Id={2};Password={3};Database=aether;Pooling=true;";
}