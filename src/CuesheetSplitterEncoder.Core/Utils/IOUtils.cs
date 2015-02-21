using System.Collections.Generic;
using System.IO;


namespace CuesheetSplitterEncoder.Core.Utils
{
    public static class IOUtils
    {
        public static void FileCopy(string sourceFileName, string destFileName)
        {
            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }
            File.Copy(sourceFileName, destFileName);
        }

        public static void FileMove(string sourceFileName, string destFileName)
        {
            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }
            File.Move(sourceFileName, destFileName);
        }

        public static void FileDelete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static IEnumerable<string> ReadAllLines(this TextReader textReader)
        {
            var lines = new List<string>();

            string line;
            while ((line = textReader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}