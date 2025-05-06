using System;
using System.IO;
using System.Linq;

namespace JRPG.Core.Utils
{
    public class AssetPathResolver
    {
        private const string RelativeAssetsPath = @"..\..\..\Assets";

        /// <summary>
        /// Gets the absolute path to the root Assets directory.
        /// </summary>
        public static string GetAssetsRootPath()
        {
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, RelativeAssetsPath));
        }

        /// <summary>
        /// Gets the path to a specific subfolder or file in the Assets directory.
        /// </summary>
        /// <param name="paths">Additional subfolder(s) or filename.</param>
        public static string GetPath(params string[] paths)
        {
            string root = GetAssetsRootPath();
            return Path.Combine(new[] { root }.Concat(paths).ToArray());
        }

        public static string GetPortraitPath(string file)
        {
            string root = GetAssetsRootPath();
            return Path.Combine( root, "Portraits/", file);
        }

        public static string GetIconsPath(string file)
        {
            string root = GetAssetsRootPath();
            return Path.Combine(root, "Icons/", file);
        }

        public static string GetSpritesPath(string file)
        {
            string root = GetAssetsRootPath();
            return Path.Combine(root, "Sprites/", file);
        }
    }
}
