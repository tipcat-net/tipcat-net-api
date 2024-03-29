﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public readonly struct AccountRequest
{
    [JsonConstructor]
    public AccountRequest(int? id, string address, string name, string currency, string? operatingName = null, string? email = null, string? phone = null)
    {
        Id = id;
        Address = address;
        Email = email;
        Name = name;
        OperatingName = operatingName;
        Phone = phone;
        Currency = currency;
    }


    public AccountRequest(int? id, in AccountRequest request) : this(id, request.Address, request.Name, request.Currency, request.OperatingName, request.Email, request.Phone)
    { }


    public int? Id { get; }
    [Required]
    public string Address { get; }
    public string? Email { get; }
    [Required]
    public string Name { get; }
    public string? OperatingName { get; }
    public string? Phone { get; }
    [Required]
    public string Currency { get; }


    public static AccountRequest CreateEmpty(int? id) => new(id, string.Empty, string.Empty, string.Empty);
}