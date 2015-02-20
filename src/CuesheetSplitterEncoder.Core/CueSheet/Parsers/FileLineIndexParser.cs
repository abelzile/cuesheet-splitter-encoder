using System;
using System.Linq;


namespace CuesheetSplitterEncoder.Core.CueSheet.Parsers
{
    public class FileLineIndexParser : IParser<Index>
    {
        readonly FileLine _line;
        
        public FileLineIndexParser(FileLine line)
        {
            if (line == null)
                throw new ArgumentNullException("line");

            _line = line;
        }

        public Index Parse()
        {
            return new Index
            {
                IndexNum = byte.Parse(_line.RawParts[1]),
                IndexTime = new IndexTime(_line.RawParts.Last())
            };
        }
    }
}