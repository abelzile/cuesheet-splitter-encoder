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
    }
}