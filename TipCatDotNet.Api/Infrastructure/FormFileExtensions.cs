using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class FormFileExtensions
    {
        /// <summary>
        /// save file to path
        /// </summary>
        /// <param name="file"></param>
        /// <param name="path"></param>
        /// <param name="id"></param>
        /// <param name="fileName"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static bool Save(this IFormFile file, string path, string? id, out string fileName, out string fullPath)
        {
            
            /*fullPath = null!;
            fileName = null!;
            try
            {
                if (file.Length == 0)
                    return false;

                fileName = id + Path.GetExtension(file.FileName);
                fullPath = Path.Combine(path, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                file.CopyTo(stream);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }*/

            throw new NotImplementedException(); // TODO add s3 storage save files

        }
        
        /// <summary>
        /// returned by mb format
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static long GetSize(this IFormFile file)
        {
            return file.Length/1024/1024;
        }
        
        public static bool IsImage(this IFormFile file)
        {
           
            return file.ContentType switch
            {
                "image/jpeg" => true,
                "image/png" => true,
                _ => false
            };
        }
        
    }
}