using System;
using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public class FacilityResponse
{
    public FacilityResponse(int id, string name, string address, int accountId, string? avatarUrl, List<MemberResponse>? members, TimeOnly sessionEndTime)
    {
        Id = id;
        Name = name;
        Address = address;
        AccountId = accountId;
        AvatarUrl = avatarUrl;
        Members = members ?? new List<MemberResponse>();
        SessionEndTime = sessionEndTime;
    }


    public FacilityResponse(in FacilityResponse response, List<MemberResponse>? members) : this(response.Id, response.Name, response.Address,
        response.AccountId, response.AvatarUrl, members, response.SessionEndTime)
    { }


    public int Id { get; }
    public string Name { get; }
    public string Address { get; }
    public int AccountId { get; }
    public string? AvatarUrl { get; }
    public List<MemberResponse> Members { get; }
    public TimeOnly SessionEndTime { get; }
}