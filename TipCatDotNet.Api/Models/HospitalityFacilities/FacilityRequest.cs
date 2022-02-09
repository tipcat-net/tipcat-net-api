using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public class FacilityRequest
{
    [JsonConstructor]
    public FacilityRequest(int? id, string name, string address, int? accountId, TimeOnly sessionEndTime, string? commercialName = null, string? email = null,
        string? phone = null)
    {
        Id = id;
        Address = address;
        AccountId = accountId;
        CommercialName = commercialName ?? string.Empty;
        Email = email ?? string.Empty;
        Name = name;
        Phone = phone ?? string.Empty;
        SessionEndTime = sessionEndTime;
    }


    public FacilityRequest(int? id, int? accountId)
    {
        Id = id;
        AccountId = accountId;
        CommercialName = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Name = string.Empty;
        Address = string.Empty;
    }


    public FacilityRequest(int? id, FacilityRequest request) : this(id, request.Name, request.Address, request.AccountId, request.SessionEndTime,
        request.CommercialName, request.Email, request.Phone)
    { }


    public FacilityRequest(int? id, int? accountId, FacilityRequest request) : this(id, request.Name, request.Address, accountId, request.SessionEndTime,
        request.CommercialName, request.Email, request.Phone)
    { }


    public static FacilityRequest CreateWithAccountIdAndName(int accountId, string name)
        => new(null, name, string.Empty, accountId, default);


    public int? Id { get; }
    [Required]
    public string Address { get; }
    public int? AccountId { get; }
    public string CommercialName { get; }
    public string Email { get; }
    [Required]
    public string Name { get; }
    public string Phone { get; }
    public TimeOnly SessionEndTime { get; }
}