using System;
using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public readonly struct FacilityResponse
{
    public FacilityResponse(int id, string name, string address, int accountId, string? avatarUrl, List<MemberResponse>? members, TimeOnly sessionEndTime,
        string operatingName, string email, string phone)
    {
        Id = id;
        Address = address;
        AccountId = accountId;
        AvatarUrl = avatarUrl;
        OperatingName = operatingName;
        Email = email;
        Members = members ?? new List<MemberResponse>(0);
        Name = name;
        Phone = phone;
        SessionEndTime = sessionEndTime;
    }


    public FacilityResponse(in FacilityResponse response, List<MemberResponse>? members) : this(response.Id, response.Name, response.Address,
        response.AccountId, response.AvatarUrl, members, response.SessionEndTime, response.OperatingName, response.Email, response.Phone)
    { }


    public int Id { get; }
    public string Name { get; }
    public string Address { get; }
    public int AccountId { get; }
    public string? AvatarUrl { get; }
    public string Email { get; }
    public List<MemberResponse> Members { get; }
    public string OperatingName { get; }
    public string Phone { get; }
    public TimeOnly SessionEndTime { get; }
}