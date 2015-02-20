using System;


namespace CuesheetSplitterEncoder.Core.CueSheet.Parsers
{
    public class FileLineFileParser: IParser<File>
    {
        readonly FileLine _line;
        
        public FileLineFileParser(FileLine line)
        {
            if (line == null) 
                throw new ArgumentNullException("line");

            _line = line;
        }

        public File Parse()
        {
            var file = new File { FileType = _line.RawParts[_line.RawParts.Count - 1] };

            int valStart = (_line.Command + " ").Length;
            int valLen = _line.Line.Length - valStart - file.FileType.Length - 1;

            file.FileName = _line.Line.Substring(valStart, valLen).Trim(' ', '"');

            return file;
        }
    }
}