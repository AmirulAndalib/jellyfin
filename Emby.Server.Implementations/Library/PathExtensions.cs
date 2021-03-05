#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Emby.Server.Implementations.Library
{
    /// <summary>
    /// Class providing extension methods for working with paths.
    /// </summary>
    public static class PathExtensions
    {
        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="attribute">The attrib.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ArgumentException"><paramref name="str" /> or <paramref name="attribute" /> is empty.</exception>
        public static string? GetAttributeValue(this string str, string attribute)
        {
            if (str.Length == 0)
            {
                throw new ArgumentException("String can't be empty.", nameof(str));
            }

            if (attribute.Length == 0)
            {
                throw new ArgumentException("String can't be empty.", nameof(attribute));
            }

            string srch = "[" + attribute + "=";
            int start = str.IndexOf(srch, StringComparison.OrdinalIgnoreCase);
            if (start != -1)
            {
                start += srch.Length;
                int end = str.IndexOf(']', start);
                return str.Substring(start, end - start);
            }

            // for imdbid we also accept pattern matching
            if (string.Equals(attribute, "imdbid", StringComparison.OrdinalIgnoreCase))
            {
                var m = Regex.Match(str, "tt([0-9]{7,8})", RegexOptions.IgnoreCase);
                return m.Success ? m.Value : null;
            }

            return null;
        }

        /// <summary>
        /// Replaces a sub path with another sub path and normalizes the final path.
        /// </summary>
        /// <param name="path">The original path.</param>
        /// <param name="subPath">The original sub path.</param>
        /// <param name="newSubPath">The new sub path.</param>
        /// <param name="newPath">The result of the sub path replacement</param>
        /// <returns>The path after replacing the sub path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path" />, <paramref name="newSubPath" /> or <paramref name="newSubPath" /> is empty.</exception>
        public static bool TryReplaceSubPath(this string path, string subPath, string newSubPath, [NotNullWhen(true)] out string? newPath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(subPath))
            {
                throw new ArgumentNullException(nameof(subPath));
            }

            if (string.IsNullOrWhiteSpace(newSubPath))
            {
                throw new ArgumentNullException(nameof(newSubPath));
            }

            char oldDirectorySeparatorChar;
            char newDirectorySeparatorChar;
            // True normalization is still not possible https://github.com/dotnet/runtime/issues/2162
            // The reasoning behind this is that a forward slash likely means it's a Linux path and
            // so the whole path should be normalized to use / and vice versa for Windows (although Windows doesn't care much).
            if (newSubPath.Contains('/', StringComparison.Ordinal))
            {
                oldDirectorySeparatorChar = '\\';
                newDirectorySeparatorChar = '/';
            }
            else
            {
                oldDirectorySeparatorChar = '/';
                newDirectorySeparatorChar = '\\';
            }

            if (path.Contains(oldDirectorySeparatorChar, StringComparison.Ordinal))
            {
                path = path.Replace(oldDirectorySeparatorChar, newDirectorySeparatorChar);
            }

            if (subPath.Contains(oldDirectorySeparatorChar, StringComparison.Ordinal))
            {
                subPath = subPath.Replace(oldDirectorySeparatorChar, newDirectorySeparatorChar);
            }

            // We have to ensure that the sub path ends with a directory separator otherwise we'll get weird results
            // when the sub path matches a similar but in-complete subpath
            if (!subPath.EndsWith(newDirectorySeparatorChar))
            {
                subPath += newDirectorySeparatorChar;
            }

            if (newSubPath.Contains(oldDirectorySeparatorChar, StringComparison.Ordinal))
            {
                newSubPath = newSubPath.Replace(oldDirectorySeparatorChar, newDirectorySeparatorChar);
            }

            if (!newSubPath.EndsWith(newDirectorySeparatorChar))
            {
                newSubPath += newDirectorySeparatorChar;
            }

            if (!path.Contains(subPath, StringComparison.OrdinalIgnoreCase))
            {
                newPath = null;
                return false;
            }

            newPath = path.Replace(subPath, newSubPath, StringComparison.OrdinalIgnoreCase);
            return true;
        }
    }
}
