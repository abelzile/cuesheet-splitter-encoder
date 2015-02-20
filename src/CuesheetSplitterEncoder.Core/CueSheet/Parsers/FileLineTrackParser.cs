using System;


namespace CuesheetSplitterEncoder.Core.CueSheet.Parsers
{
    public class FileLineTrackParser : IParser<Track>
    {
        readonly FileLine _line;

        public FileLineTrackParser(FileLine line)
        {
            if (line == null) 
                throw new ArgumentNullException("line");

            _line = line;
        }

        public Track Parse()
        {
            return new Track
            {
                TrackNum = int.Parse(_line.RawParts[1]), 
                TrackType = _line.RawParts[2]
            };
        }
    }
}