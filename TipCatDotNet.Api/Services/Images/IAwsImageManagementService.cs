using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Services.Images;

public interface IAwsImageManagementService
{
    Task<Result> Delete(string bucketName, string key, CancellationToken cancellationToken);
    Task<Result<string>> Upload(string bucketName, FormFile file, string key, CancellationToken cancellationToken);
}