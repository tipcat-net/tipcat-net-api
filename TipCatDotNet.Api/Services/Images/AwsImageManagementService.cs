using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Services;
using Imageflow.Fluent;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Services.Images;

public class AwsImageManagementService : IAwsImageManagementService
{
    public AwsImageManagementService(IAmazonS3ClientService client)
    {
        _client = client;
    }


    public async Task<Result<string>> Upload(FormFile file, string key, CancellationToken cancellationToken)
    {
        using var binaryReader = new BinaryReader(file.OpenReadStream());
        var bytes = binaryReader.ReadBytes((int)file.Length);

        return await Result.Success()
            .Bind(EnsureDimensionsValid)
            .Bind(Convert)
            .Bind(UploadInternal);


        async Task<Result> EnsureDimensionsValid()
        {
            var info = await ImageJob.GetImageInfo(new BytesSource(bytes), cancellationToken);
            if (info.ImageWidth < MinimalWidth)
                return Result.Failure($"The image is too small. Minimal dimensions are {MinimalWidth}x{MinimalWidth}.");

            // Assuming a little calculation error may occur when an original image crops, so here's a tolerance check.
            var aspectRation = info.ImageWidth / info.ImageHeight;
            if (0.98 <= aspectRation || aspectRation <= 1.02)
                return Result.Failure("The image must have an aspect ratio close to 1:1.");

            return Result.Success();
        }


        async Task<Result<byte[]>> Convert()
        {
            using var imageJob = new ImageJob();
            var result = await imageJob.Decode(bytes)
                .Constrain(new Constraint(ConstraintMode.Within, (uint)MinimalWidth, (uint)MinimalWidth))
                .EncodeToBytes(new MozJpegEncoder(TargetQuality, progressive: true))
                .Finish()
                .InProcessAsync();

            var encoded = result?.TryGet(1);
            var encodedBytes = encoded?.TryGetBytes();
            if (encodedBytes is not null)
                return encodedBytes.Value.ToArray();
            
            return Result.Failure<byte[]>("Sorry, we couldn't process the provided image. Please, try another one."); 
        }


        async Task<Result<string>> UploadInternal(byte[] encodedBytes)
        {
            await using var stream = new MemoryStream(encodedBytes);
            return await _client.Add(_client.Options.DefaultBucketName, key, stream, cancellationToken);
        }
    }


    private const long MinimalWidth = 250;
    private const int TargetQuality = 80;


    private readonly IAmazonS3ClientService _client;
}