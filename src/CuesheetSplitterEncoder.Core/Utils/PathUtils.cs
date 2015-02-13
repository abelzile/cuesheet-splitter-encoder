using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace CuesheetSplitterEncoder.Core.Utils
{
    public static class PathUtils
    {
        public static string RemoveInvalidFileNameChars(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            fileName = new string(fileName.Select(c => invalidChars.Contains(c) ? ' ' : c).ToArray());

            return Regex.Replace(fileName, @"\s+", " ");
        }
    }
}