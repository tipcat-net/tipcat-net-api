using System;
using System.ComponentModel.DataAnnotations;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.Permissions.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public readonly struct MemberResponse
{
    public MemberResponse(int id, int? accountId, int? facilityId, string firstName, string lastName, string? avatarUrl, string? email, string? position,
        string memberCode, string? qrCodeUrl, MemberPermissions permissions, InvitationStates invitationState, bool isActive)
    {
        Id = id;
        AccountId = accountId;
        AvatarUrl = avatarUrl;
        Email = email;
        FacilityId = facilityId;
        FirstName = firstName;
        InvitationState = invitationState;
        IsActive = isActive;
        LastName = lastName;
        MemberCode = memberCode;
        Permissions = permissions;
        Position = position;
        QrCodeUrl = qrCodeUrl;
    }


    public MemberResponse(in MemberResponse response, InvitationStates invitationState) : this(response.Id, response.AccountId, response.FacilityId,
        response.FirstName, response.LastName, response.AvatarUrl, response.Email, response.Position, response.MemberCode, response.QrCodeUrl, 
        response.Permissions, invitationState, response.IsActive)
    { }


    public override bool Equals(object? obj) => obj is MemberResponse other && Equals(other);


    public bool Equals(in MemberResponse other)
        => (Id, FirstName, LastName, Email, Permissions, InvitationState, FacilityId, IsActive)
            == (other.Id, other.FirstName, other.LastName, other.Email, other.Permissions, other.InvitationState, other.FacilityId, other.IsActive);


    public override int GetHashCode() => HashCode.Combine(Id, FirstName, LastName, Email, (int)Permissions, (int)InvitationState, FacilityId, IsActive);


    public static bool operator ==(MemberResponse left, MemberResponse right) => left.Equals(right);


    public static bool operator !=(MemberResponse left, MemberResponse right) => !(left == right);


    [Required]
    public int Id { get; }
    public int? AccountId { get; }
    public string? AvatarUrl { get; }
    public string? Email { get; }
    public int? FacilityId { get; }
    [Required]
    public string FirstName { get; }
    [Required]
    public InvitationStates InvitationState { get; }
    [Required]
    public bool IsActive { get; }
    [Required]
    public string LastName { get; }
    [Required]
    public string MemberCode { get; }
    [Required]
    public MemberPermissions Permissions { get; }
    public string? Position { get; }
    public string? QrCodeUrl { get; }
}