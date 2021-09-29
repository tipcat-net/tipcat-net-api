using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TipCatDotNet.Api.Infrastructure
{
    public interface IQrCodeGenerator
    {
        Task<Result<string>> Generate(string memberCode, CancellationToken cancellationToken);
    }
}