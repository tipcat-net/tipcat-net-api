using System;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility;

public class Facility
{
    public int Id { get; set; }
    public string Address { get; set; } = null!;
    public int AccountId { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime Created { get; set; }
    public string Email { get; set; } = null!;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; }
    public DateTime Modified { get; set; }
    public string Name { get; set; } = null!;
    public string OperatingName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public TimeOnly SessionEndTime { get; set; }
}