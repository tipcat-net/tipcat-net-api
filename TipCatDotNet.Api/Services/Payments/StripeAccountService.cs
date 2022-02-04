using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Stripe;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Options;
using TipCatData = TipCatDotNet.Api.Data.Models;

namespace TipCatDotNet.Api.Services.Payments;

public class StripeAccountService : IStripeAccountService
{
    public StripeAccountService(AetherDbContext context, Stripe.AccountService accountService, IOptions<StripeOptions> stripeOptions)
    {
        _context = context;
        _accountService = accountService;
        _stripeOptions = stripeOptions;
    }


    public Task<Result> AddForMember(MemberRequest request, CancellationToken cancellationToken)
    {
        return Result.Success()
            .Bind(CreateStripeAccount)
            .Bind(CreateRelatedAccount);


        async Task<Result<string>> CreateStripeAccount()
        {
            var options = new AccountCreateOptions
            {
                Country = "AE",
                Type = "custom", // TODO: leave it for now
                // BusinessType = "individual",
                // Individual = new AccountIndividualOptions
                // {
                //     FirstName = request.FirstName,
                //     LastName = request.LastName,
                //     Email = request.Email
                // },
                Metadata = new Dictionary<string, string>()
                {
                    { "MemberId", request.Id!.ToString() ?? string.Empty },
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true,
                    },
                    // TODO: Figure out which cababilities (Payment_methods) account requested
                }
            };
            try
            {
                var account = await _accountService.CreateAsync(options, cancellationToken: cancellationToken);
                return account.Id;
            }
            catch (StripeException ex)
            {
                return Result.Failure<string>(ex.Message);
            }
        }


        async Task<Result> CreateRelatedAccount(string accountId)
        {
            var now = DateTime.UtcNow;

            var newRelatedAccount = new StripeAccount
            {
                StripeId = accountId,
                MemberId = (int)request.Id!
            };

            _context.StripeAccounts.Add(newRelatedAccount);
            await _context.SaveChangesAsync(cancellationToken);
            _context.DetachEntities();

            await SetStripeAccountActive(accountId, request.Id.Value, cancellationToken);

            return Result.Success();
        }
    }


    public Task<Result> AddForAccount(TipCatData.HospitalityFacility.Account account, CancellationToken cancellationToken)
    {
        return Result.Success()
            .Bind(CreateStripeAccount)
            .Bind(SetStripeAccount);


        async Task<Result<string>> CreateStripeAccount()
        {
            var options = new AccountCreateOptions
            {
                Country = "AE",
                Type = "custom",
                BusinessType = "company",
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    Name = account.Name,
                    ProductDescription = account.OperatingName,
                    // Mcc = "", TODO
                    // SupportAddress = new AddressOptions
                    // {
                    //     City = "",
                    //     Country = "",
                    //     Line1 = "",
                    //     Line2 = "",
                    //     PostalCode = "",
                    //     State = ""
                    // },
                    SupportPhone = account.Phone,
                    SupportEmail = account.Email,
                },
                Company = new AccountCompanyOptions
                {
                    // Address = new AddressOptions
                    // {
                    //     City = "",
                    //     Country = "",
                    //     Line1 = "",
                    //     Line2 = "",
                    //     PostalCode = "",
                    //     State = ""
                    // },
                    Name = account.Name,
                    Phone = account.Phone,
                    // RegistrationNumber = "",
                    // TaxId = "",
                    // VatId = "",
                    // Verification = new AccountCompanyVerificationOptions
                    // {
                    //     Document = new AccountCompanyVerificationDocumentOptions
                    //     {
                    //         Back = "", //The back of a document returned by a file upload
                    //         Front = "" //The front of a document returned by a file upload
                    //     }
                    // }                    
                },
                Metadata = new Dictionary<string, string>()
                {
                    { "AccountId", account.Id!.ToString() ?? string.Empty },
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true,
                    },
                    // TODO: Figure out which cababilities (Payment_methods) account requested
                }
            };
            try
            {
                var account = await _accountService.CreateAsync(options, cancellationToken: cancellationToken);
                return account.Id;
            }
            catch (StripeException ex)
            {
                return Result.Failure<string>(ex.Message);
            }
        }


        async Task<Result> SetStripeAccount(string stripeAccountId)
        {
            account.StripeAccount = stripeAccountId;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }



    public async Task<Result> SetStripeAccountActive(string stripeAccountId, int memberId, CancellationToken cancellationToken)
    {
        var targetMember = await _context.Members
                       .SingleAsync(m => m.Id == memberId, cancellationToken);

        targetMember.ActiveStripeId = stripeAccountId;
        targetMember.Modified = DateTime.UtcNow;

        _context.Members.Update(targetMember);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<Result<StripeAccountResponse>> Retrieve(MemberRequest request, CancellationToken cancellationToken)
    {
        var stripeAccount = await _context.StripeAccounts
            .SingleOrDefaultAsync(s => s.MemberId == request.Id, cancellationToken);

        if (stripeAccount == null)
            return Result.Failure<StripeAccountResponse>("This is not presented member's account!");

        try
        {
            var account = await _accountService.GetAsync(stripeAccount.StripeId, cancellationToken: cancellationToken);
            if (account.Metadata["MemberId"] != request.Id.ToString())
                return Result.Failure<StripeAccountResponse>("This is not presented member's account!");

            return new StripeAccountResponse(account.Id);
        }
        catch (StripeException ex)
        {
            return Result.Failure<StripeAccountResponse>(ex.Message);
        }
    }


    public async Task<Result> Update(MemberRequest request, CancellationToken cancellationToken)
    {
        var stripeAccount = await _context.StripeAccounts
            .SingleOrDefaultAsync(s => s.MemberId == request.Id, cancellationToken);

        if (stripeAccount == null)
            return Result.Failure("This is not presented member's account!");

        var (_, isFailure, isMatch, error) = await AreAccountsMatch(stripeAccount.MemberId, stripeAccount.StripeId, cancellationToken);

        if (isFailure)
            return Result.Failure(error);

        var updateOptions = new AccountUpdateOptions
        {
            Individual = new AccountIndividualOptions
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            }
        };
        try
        {
            var updatedAccount = await _accountService.UpdateAsync(stripeAccount.StripeId, updateOptions, cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (StripeException ex)
        {
            return Result.Failure(ex.Message);
        }
    }


    public async Task<Result> AttachDefaultExternal(PayoutMethodRequest request, CancellationToken cancellationToken)
    {
        var stripeAccount = await _context.StripeAccounts
            .SingleOrDefaultAsync(s => s.MemberId == request.MemberId, cancellationToken);

        if (stripeAccount == null)
            return Result.Failure("This is not presented member's account!");

        var (_, isFailure, isMatch, error) = await AreAccountsMatch(stripeAccount.MemberId, stripeAccount.StripeId, cancellationToken);

        if (isFailure)
            return Result.Failure(error);

        var attachOptions = new AccountUpdateOptions
        {
            ExternalAccount = request.Token
        };
        try
        {
            var updatedAccount = await _accountService.UpdateAsync(stripeAccount.StripeId, attachOptions, cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (StripeException ex)
        {
            return Result.Failure(ex.Message);
        }
    }


    /// <summary>
    /// Remove stripe account.
    /// Accounts created using test-mode keys can be deleted at any time.
    /// Custom or Express accounts created using live-mode keys can only be deleted once all balances are zero.
    /// </summary>
    public Task<Result> Remove(int memberId, CancellationToken cancellationToken)
    {
        return Result.Success()
            .Bind(RemoveStripeAccount)
            .Bind(RemoveRelatedAccount);


        async Task<Result<StripeAccount>> RemoveStripeAccount()
        {
            var stripeAccount = await _context.StripeAccounts
                .SingleOrDefaultAsync(s => s.MemberId == memberId, cancellationToken);

            if (stripeAccount == null)
                return Result.Failure<StripeAccount>("This is not presented member's account!");

            var (_, isFailure, isMatch, error) = await AreAccountsMatch(stripeAccount.MemberId, stripeAccount.StripeId, cancellationToken);

            if (isFailure)
                return Result.Failure<StripeAccount>(error);

            try
            {
                var deletedAccount = await _accountService.DeleteAsync(stripeAccount.StripeId, cancellationToken: cancellationToken);
                return stripeAccount;
            }
            catch (StripeException ex)
            {
                return Result.Failure<StripeAccount>(ex.Message);
            }
        }


        async Task<Result> RemoveRelatedAccount(StripeAccount account)
        {
            _context.StripeAccounts.Remove(account);
            await _context.SaveChangesAsync(cancellationToken);
            _context.DetachEntities();

            return Result.Success();
        }
    }


    private async Task<Result<string>> CreateStripeAccount(RelatedObjects relatedObject, int? relatedObjectId, CancellationToken cancellationToken)
    {
        var options = new AccountCreateOptions
        {
            Country = "AE",
            Type = "custom",
            BusinessType = "company",
            BusinessProfile = new AccountBusinessProfileOptions
            {

            },
            Metadata = new Dictionary<string, string>()
                {
                    { (relatedObject == RelatedObjects.Member) ? "MemberId": "AccountId", relatedObjectId!.ToString() ?? string.Empty },
                },
            Capabilities = new AccountCapabilitiesOptions
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions
                {
                    Requested = true,
                },
                Transfers = new AccountCapabilitiesTransfersOptions
                {
                    Requested = true,
                },
                // TODO: Figure out which cababilities (Payment_methods) account requested
            }
        };
        try
        {
            var account = await _accountService.CreateAsync(options, cancellationToken: cancellationToken);
            return account.Id;
        }
        catch (StripeException ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }


    private async Task<Result<bool>> AreAccountsMatch(int memberId, string accountId, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountService.GetAsync(accountId, cancellationToken: cancellationToken);

            if (!account.Metadata.ContainsKey("MemberId"))
                return Result.Failure<bool>("The account does not contain member's metadata.");

            if (account.Metadata["MemberId"] != memberId.ToString())
                return Result.Failure<bool>("This is not presented member's account!");

            return true;
        }
        catch (StripeException ex)
        {
            return Result.Failure<bool>(ex.Message);
        }
    }


    private enum RelatedObjects
    {
        Member,
        Account
    }


    private readonly AetherDbContext _context;
    private readonly Stripe.AccountService _accountService;
    private readonly IOptions<StripeOptions> _stripeOptions;
}