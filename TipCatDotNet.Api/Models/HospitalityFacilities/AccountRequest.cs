﻿using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct AccountRequest
    {
        public AccountRequest(int? id, string address, string? commercialName, string? email, string name, string phone)
        {
            Id = id;
            Address = address;
            CommercialName = commercialName;
            Email = email;
            Name = name;
            Phone = phone;
        }


        public int? Id { get; }
        [Required]
        public string Address { get; }
        public string? CommercialName { get; }
        public string? Email { get; }
        [Required]
        public string Name { get; }
        [Required]
        public string Phone { get; }
    }
}
