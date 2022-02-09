using System;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility;

public class Facility
{
    public int Id { get; set; }
    public string Address { get; set; } = null!;
    public int AccountId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CommercialName { get; set; }
    public DateTime Created { get; set; }
    public string? Email { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; }
    public DateTime Modified { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public TimeOnly SessionEndTime { get; set; }
}