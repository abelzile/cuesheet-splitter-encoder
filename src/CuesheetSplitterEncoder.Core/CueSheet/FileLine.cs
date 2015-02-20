using System;
using System.Collections.ObjectModel;


namespace CuesheetSplitterEncoder.Core.CueSheet
{
    public class FileLine
    {
        readonly string _command;
        readonly string _line;
        readonly ReadOnlyCollection<string> _rawParts;

        public FileLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) 
                throw new ArgumentNullException("line");

            _line = line.Trim();
            _rawParts = Array.AsReadOnly(_line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            _command = _rawParts[0].ToUpperInvariant();
        }

        public string Command
        {
            get { return _command; }
        }

        public string Line
        {
            get { return _line; }
        }

        public ReadOnlyCollection<string> RawParts
        {
            get { return _rawParts; }
        }
    }
}