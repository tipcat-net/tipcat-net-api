﻿using TipCatDotNet.Api.Models.Company;

namespace TipCatDotNet.Api.Services.Company;

// TODO: move details to a table
public class CompanyInfoService
{
    public static CompanyInfo Get
        => new("TIP CAT L.L.C.", "United Arab Emirates", "Dubai", "B105, Saraya Avenue building", "36690", "995090");
}