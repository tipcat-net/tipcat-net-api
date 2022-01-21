using System;
using Microsoft.Extensions.Logging;

namespace TipCatDotNet.Api.Infrastructure.Logging;

public static class LoggerExtensions
{
    static LoggerExtensions()
    {
        MemberAuthorizationFailure = LoggerMessage.Define<string>(LogLevel.Warning,
            new EventId(1000, "MemberAuthorizationFailure"),
            "Member authorization failure: '{Error}'");

        MemberAuthorizationSuccess = LoggerMessage.Define<string, string>(LogLevel.Debug,
            new EventId(1001, "MemberAuthorizationSuccess"),
            "Successfully authorized member '{Email}' for '{Permissions}'");

        NoIdentifierOnMemberAddition = LoggerMessage.Define(LogLevel.Warning,
            new EventId(1002, "NoIdentifierOnMemberAddition"),
            "The provided Jwt token contains no ID, but it supposes to be");

        AmazonS3Unreachable = LoggerMessage.Define<string>(LogLevel.Warning,
            new EventId(1003, "AmazonS3Unreachable"),
            "Amazon S3 service admission: '{Error}'");

        Auth0Exception = LoggerMessage.Define<string>(LogLevel.Warning,
            new EventId(1004, "Auth0Exception"),
            "Authorization service exception: '{Error}'");

        StripeException = LoggerMessage.Define<string>(LogLevel.Warning,
            new EventId(1005, "StripeException"),
            "Stripe service exception: '{Error}'");

        MemberBelongFacilityFailure = LoggerMessage.Define<string>(LogLevel.Warning,
            new EventId(1006, "MemberBelongFacilityFailure"),
            "Member belong to facility failure: '{Error}'");

        AccountResumeDoesntExist = LoggerMessage.Define<string>(LogLevel.Warning,
            new EventId(1007, "AccountResumeDoesntExistException"),
            "AccountResume service failure: '{Error}'");
    }


    public static void LogMemberAuthorizationFailure(this ILogger logger, string Error, Exception exception = null)
       => MemberAuthorizationFailure(logger, Error, exception);

    public static void LogMemberAuthorizationSuccess(this ILogger logger, string Email, string Permissions, Exception exception = null)
       => MemberAuthorizationSuccess(logger, Email, Permissions, exception);

    public static void LogNoIdentifierOnMemberAddition(this ILogger logger, Exception exception = null)
       => NoIdentifierOnMemberAddition(logger, exception);

    public static void LogMemberBelongFacilityFailure(this ILogger logger, string Error, Exception exception = null)
       => MemberBelongFacilityFailure(logger, Error, exception);

    public static void LogAmazonS3Unreachable(this ILogger logger, string Error, Exception exception = null)
       => AmazonS3Unreachable(logger, Error, exception);

    public static void LogAuth0Exception(this ILogger logger, string Error, Exception exception = null)
       => Auth0Exception(logger, Error, exception);

    public static void LogStripeException(this ILogger logger, string Error, Exception exception = null)
       => StripeException(logger, Error, exception);

    public static void LogAccountResumeDoesntExist(this ILogger logger, string Error, Exception exception = null)
       => AccountResumeDoesntExist(logger, Error, exception);


    private static readonly Action<ILogger, string, Exception> MemberAuthorizationFailure;

    private static readonly Action<ILogger, string, string, Exception> MemberAuthorizationSuccess;

    private static readonly Action<ILogger, Exception> NoIdentifierOnMemberAddition;

    private static readonly Action<ILogger, string, Exception> MemberBelongFacilityFailure;

    private static readonly Action<ILogger, string, Exception> AmazonS3Unreachable;

    private static readonly Action<ILogger, string, Exception> Auth0Exception;

    private static readonly Action<ILogger, string, Exception> StripeException;
    private static readonly Action<ILogger, string, Exception> AccountResumeDoesntExist;
}