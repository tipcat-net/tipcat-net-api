using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Flurl;
using HappyTravel.AmazonS3Client.Services;
using Microsoft.Extensions.Options;
using QRCoder;
using TipCatDotNet.Api.Options;

namespace TipCatDotNet.Api.Services.Images;

public class QrCodeGenerator : IQrCodeGenerator
{
    public QrCodeGenerator(IAmazonS3ClientService client, IOptionsMonitor<QrCodeGeneratorOptions> options)
    {
        _client = client;
        _options = options.CurrentValue;
    }


    public async Task<Result<string>> Generate(string memberCode, CancellationToken cancellationToken)
    {
        var url = _options.BaseServiceUrl.AppendPathSegment($"/{memberCode}/pay");

        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(PixelsPerModule);

        await using var stream = new MemoryStream(qrCodeImage);

        return await _client.Add(_client.Options.DefaultBucketName, memberCode, stream, cancellationToken);
    }


    public const int PixelsPerModule = 20;

    private readonly IAmazonS3ClientService _client;
    private readonly QrCodeGeneratorOptions _options;
}