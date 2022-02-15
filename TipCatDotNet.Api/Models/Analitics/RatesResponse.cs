using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Analitics;

public readonly struct RatesResponse
{
    [JsonConstructor]
    public RatesResponse(List<DataRates> data)
    {
        Data = data;
    }


    [JsonPropertyName("data")]
    public List<DataRates> Data { get; }
}


public readonly struct DataRates
{
    [JsonConstructor]
    public DataRates(string id, dynamic rates)
    {
        Id = id;
        Rates = rates;
    }


    [JsonPropertyName("id")]
    public string Id { get; }
    [JsonPropertyName("rates")]
    public dynamic Rates { get; }
}