using System;
using Microsoft.Extensions.Logging;

namespace TipCatDotNet.Api.Infrastructure.Logging
{
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
            
        }
    
                
         public static void LogMemberAuthorizationFailure(this ILogger logger, string Error, Exception exception = null)
            => MemberAuthorizationFailure(logger, Error, exception);
                
         public static void LogMemberAuthorizationSuccess(this ILogger logger, string Email, string Permissions, Exception exception = null)
            => MemberAuthorizationSuccess(logger, Email, Permissions, exception);
    
    
        
        private static readonly Action<ILogger, string, Exception> MemberAuthorizationFailure;
        
        private static readonly Action<ILogger, string, string, Exception> MemberAuthorizationSuccess;
    }
}