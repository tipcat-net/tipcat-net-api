using System.IO;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class FileHelper
    {
        public static void Delete(string path)
        {
            File.Delete(path);
        }
        
    }
}