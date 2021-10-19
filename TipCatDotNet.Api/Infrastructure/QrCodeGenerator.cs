using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using QRCoder;
using HappyTravel.AmazonS3Client.Services;

namespace TipCatDotNet.Api.Infrastructure
{
    public class QrCodeGenerator : IQrCodeGenerator
    {
        public QrCodeGenerator(IAmazonS3ClientService client)
        {
            _client = client;
        }


        public async Task<Result<string>> Generate(string memberCode, CancellationToken cancellationToken)
        {
            var url = $"https://dev.tipcat.net/{memberCode}/pay";

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(PixelsPerModule);

            using var stream = new MemoryStream();
            qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            return await _client.Add(_client.Options.DefaultBucketName, memberCode, stream, cancellationToken);
        }


        public const int PixelsPerModule = 20;

        private readonly IAmazonS3ClientService _client;
    }
}