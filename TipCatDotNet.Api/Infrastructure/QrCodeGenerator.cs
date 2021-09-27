using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using QRCoder;
using HappyTravel.AmazonS3Client.Services;

namespace TipCatDotNet.Api.Infrastructure
{
    public class QrCodeGenerator
    {
        public QrCodeGenerator(IAmazonS3ClientService client)
        {
            _client = client;
        }

        public async Task<string> Generate(int memberId, CancellationToken cancellationToken)
        {
            var url = $"https://dev.tipcat.net/api/members/{memberId}/pay"; // Leave it like this for now

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            using (MemoryStream stream = new MemoryStream())
            {
                qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return await _client.Add("bucketName", "key", stream, cancellationToken)
                    .Ensure(url => !string.IsNullOrWhiteSpace(url), "Url wasn't created by amazon's endpoint!")
                    .Finally(result => result.IsSuccess ? result.Value : result.Error);
            }
        }

        private readonly IAmazonS3ClientService _client;
    }
}