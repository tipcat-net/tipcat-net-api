using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Services.Images;

public interface IAwsImageManagementService
{
    Task<Result<string>> Upload(FormFile file, string key, CancellationToken cancellationToken);
}