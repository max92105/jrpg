using System;
using System.IO;

namespace JRPG.Core.Utils
{
    public static class PathHelper
    {
        public static string GetRelativePath(string basePath, string fullPath)
        {
            Uri baseUri = new Uri(AppendSlash(basePath));
            Uri fullUri = new Uri(fullPath);
            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private static string AppendSlash(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;
        }
    }
}
