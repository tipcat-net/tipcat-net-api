using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public class FacilityRequest
{
    [JsonConstructor]
    public FacilityRequest(int? id, string name, string address, int? accountId)
    {
        Id = id;
        Name = name;
        Address = address;
        AccountId = accountId;
    }


    public FacilityRequest(int? id, int? accountId)
    {
        Id = id;
        AccountId = accountId;
        Name = string.Empty;
        Address = string.Empty;
    }


    public FacilityRequest(int? id, FacilityRequest request) : this(id, request.Name, request.Address, request.AccountId)
    { }


    public FacilityRequest(int? id, int? accountId, FacilityRequest request) : this(id, request.Name, request.Address, accountId)
    { }


    public int? Id { get; }
    [Required]
    public string Name { get; }
    [Required]
    public string Address { get; }
    public int? AccountId { get; }


    public static FacilityRequest CreateWithAccountIdAndName(int accountId, string name)
        => new(null, name, string.Empty, accountId);
}