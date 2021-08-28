using System;
using Microsoft.Extensions.Logging;

namespace TipCatDotNet.Api.Infrastructure.Logging
{
    public static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            EmployeeAuthorizationFailure = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1000, "EmployeeAuthorizationFailure"),
                "Employee authorization failure: '{Error}'");
            
            EmployeeAuthorizationSuccess = LoggerMessage.Define<string, string>(LogLevel.Debug,
                new EventId(1001, "EmployeeAuthorizationSuccess"),
                "Successfully authorized employee '{Email}' for '{Permissions}'");
            
        }
    
                
         public static void LogEmployeeAuthorizationFailure(this ILogger logger, string Error, Exception exception = null)
            => EmployeeAuthorizationFailure(logger, Error, exception);
                
         public static void LogEmployeeAuthorizationSuccess(this ILogger logger, string Email, string Permissions, Exception exception = null)
            => EmployeeAuthorizationSuccess(logger, Email, Permissions, exception);
    
    
        
        private static readonly Action<ILogger, string, Exception> EmployeeAuthorizationFailure;
        
        private static readonly Action<ILogger, string, string, Exception> EmployeeAuthorizationSuccess;
    }
}