using System.IO;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class FileUploadHelper
    {
        public static bool Save( IFormFile file, string path, string? id, out string fullPath)
        {
            fullPath = null!;
            if (file.Length == 0)
                return false;

            CreateDirectory(path);
            var fileName = id + file.FileName;
            fullPath = Path.Combine(path, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            file.CopyTo(stream);

            return true;
        }

        public static void Delete(string path)
        {
            File.Delete(path);
        }

        public static long GetSize( IFormFile file)
        {
            return file.Length/1024/1024;
        }

        public static bool IsImage( IFormFile file)
        {
            var extension = GetExtension(file.FileName);
            return extension switch
            {
                "jpeg" => true,
                "jpg" => true,
                "png" => true,
                _ => false
            };
        }

        private static void CreateDirectory(string path)
        {
            var pathItems = path.Split("/");
            var newPath = "";
            for (var i = 0; i < pathItems.Length; i++)
            {
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }
        }

        private static string GetExtension(string fileName)
        {
            return "";
        }
    }
}