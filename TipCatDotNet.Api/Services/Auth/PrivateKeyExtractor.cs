using System.IO;
using System.Security.Cryptography;
using System.Text;
using PemUtils;

namespace TipCatDotNet.Api.Services.Auth
{
    internal static class PrivateKeyExtractor
    {
        public static RSACryptoServiceProvider GetProviderFromKey(string privateKey)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(privateKey);
            var rsaProvider = new RSACryptoServiceProvider();

            using var stream = new MemoryStream(keyBytes);
            using var pemReader = new PemReader(stream);
            rsaProvider.ImportParameters(pemReader.ReadRsaKey());

            return rsaProvider;
        }
    }
}
