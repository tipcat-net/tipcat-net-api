
using System.IO;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class DirectoryHelper
    {
        public static void CreateDirectory(string path)
        {
            var pathItems = path.Split("/");
            var newPath = "";
            for (var i = 0; i < pathItems.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(pathItems[i]))
                {
                    continue;
                }

                newPath += pathItems[i] + "/";
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }
        }
    }
}