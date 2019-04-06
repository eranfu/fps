using System.IO;

namespace Utils
{
    public static class FileUtils
    {
        public static string FindNewFilename(string pattern)
        {
            for (var i = 0; i < 10000; i++)
            {
                string fileName = string.Format(pattern, i);
                if (File.Exists(fileName))
                    continue;
                return fileName;
            }

            return null;
        }
    }
}