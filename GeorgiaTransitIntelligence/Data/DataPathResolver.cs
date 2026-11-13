namespace GeorgiaTransitIntelligence.Data
{
    /// <summary>
    /// Resolves repository data paths from development, test, and release environments.
    /// </summary>
    public static class DataPathResolver
    {
        public static string ResolvePath(string relativePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

            string cleanRelative = relativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            if (Path.IsPathRooted(cleanRelative))
            {
                return Path.GetFullPath(cleanRelative);
            }

            // Walk up from the app and working directories so cloned checkouts
            // resolve repository data without a developer-specific absolute path.
            var visitedRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] startingDirectories =
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory()
            };

            foreach (string start in startingDirectories)
            {
                DirectoryInfo? directory = new(Path.GetFullPath(start));
                while (directory is not null)
                {
                    if (visitedRoots.Add(directory.FullName))
                    {
                        string candidate = Path.GetFullPath(Path.Combine(directory.FullName, cleanRelative));
                        if (File.Exists(candidate) || Directory.Exists(candidate))
                        {
                            return candidate;
                        }
                    }

                    directory = directory.Parent;
                }
            }

            return Path.Combine(AppContext.BaseDirectory, cleanRelative);
        }
    }
}
