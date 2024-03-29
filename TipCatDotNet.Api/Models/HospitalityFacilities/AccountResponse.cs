﻿using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public readonly struct AccountResponse
{
    public AccountResponse(int id, string name, string operatingName, string address, string? avatarUrl, string email, string phone, bool isActive,
        string currency, List<FacilityResponse> facilities)
    {
        Id = id;
        Address = address;
        AvatarUrl = avatarUrl;
        Email = email;
        IsActive = isActive;
        Name = name;
        OperatingName = operatingName;
        Phone = phone;
        Currency = currency;
        Facilities = facilities;
    }


    public int Id { get; }
    public string Address { get; }
    public string? AvatarUrl { get; }
    public string Email { get; }
    public bool IsActive { get; }
    public string Name { get; }
    public string OperatingName { get; }
    public string Phone { get; }
    public string Currency { get; }
    public List<FacilityResponse> Facilities { get; }
}