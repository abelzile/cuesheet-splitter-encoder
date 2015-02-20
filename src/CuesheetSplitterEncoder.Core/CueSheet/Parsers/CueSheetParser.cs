using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CuesheetSplitterEncoder.Core.Utils;


namespace CuesheetSplitterEncoder.Core.CueSheet.Parsers
{
    public class CueSheetParser : IParser<CueSheet>
    {
        static readonly Dictionary<string, Action<CueSheet, string>> CueSheetSingleValueCommands =
            new Dictionary<string, Action<CueSheet, string>>
            {
                { "CATALOG", (sheet, val) => { sheet.Catalog = val; } },
                { "CDTEXTFILE", (sheet, val) => { sheet.CdTextFile = val; } },
                { "PERFORMER", (sheet, val) => { sheet.Performer = val; } },
                { "SONGWRITER", (sheet, val) => { sheet.SongWriter = val; } },
                { "TITLE", (sheet, val) => { sheet.Title = val; } },
            };

        static readonly Dictionary<string, Action<Track, Index>> TrackIndexCommands =
            new Dictionary<string, Action<Track, Index>>
            {
                { "INDEX", (track, index) => track.Indexes.Add(index) },
                { "POSTGAP", (track, index) => { track.PostGap = index; } },
                { "PREGAP", (track, index) => { track.PreGap = index; } },
            };
        static readonly Dictionary<string, Action<Track, string>> TrackSingleValueCommands =
            new Dictionary<string, Action<Track, string>>
            {
                { "FLAGS", (sheet, val) => { sheet.Flags = val; } },
                { "ISRC", (sheet, val) => { sheet.Isrc = val; } },
                { "PERFORMER", (sheet, val) => { sheet.Performer = val; } },
                { "SONGWRITER", (sheet, val) => { sheet.SongWriter = val; } },
                { "TITLE", (sheet, val) => { sheet.Title = val; } },
            };

        readonly List<string> _cueLines;
        CueSheet _cueSheet;

        public CueSheetParser(string fileName, Encoding encoding)
        {
            _cueLines = new List<string>(System.IO.File.ReadAllLines(fileName, encoding));
        }

        public CueSheetParser(string fileName)
            : this(fileName, Encoding.Default)
        {
        }

        public CueSheetParser(Stream stream)
        {
            _cueLines = new List<string>(new StreamReader(stream).ReadAllLines());
        }

        public CueSheet Parse()
        {
            CleanLines();

            _cueSheet = new CueSheet { IsStandard = IsStandard() };

            if (_cueSheet.IsStandard)
            {
                ParseStandardFile();
            }
            else
            {
                _cueSheet.IsNoncompliant = IsNoncompliant();

                ParseNonstandardFile();
            }

            return _cueSheet;
        }

        void CleanLines()
        {
            for (int i = _cueLines.Count; i-- > 0;)
            {
                if (string.IsNullOrWhiteSpace(_cueLines[i]))
                {
                    _cueLines.RemoveAt(i);
                }
            }
        }

        bool IsStandard()
        {
            return _cueLines.Select(l => new FileLine(l)).Count(line => line.Command == "FILE") == 1;
        }

        bool IsNoncompliant()
        {
            for (int i = 0; i < _cueLines.Count; i++)
            {
                var line = new FileLine(_cueLines[i]);

                int fileCount = 0;

                if (line.Command != "FILE") continue;

                ++fileCount;

                int trackCount = 0;

                for (int j = i + 1; j < _cueLines.Count; ++j)
                {
                    line = new FileLine(_cueLines[j]);

                    if (line.Command == "TRACK")
                    {
                        ++trackCount;
                    }

                    if (trackCount > fileCount) return true;

                    if (line.Command == "FILE")
                    {
                        ++fileCount;
                    }

                    if (fileCount > 1) return false;
                }
            }

            return false;
        }

        void ParseNonstandardFile()
        {
            ParseRootCommands();

            var tracks = new List<Track>();

            ParseTrackCommands(tracks);

            if (_cueSheet.Files.Count != tracks.Count) 
                throw new Exception("File count and track count don't match.");

            for (int i = 0; i < _cueSheet.Files.Count; ++i)
            {
                _cueSheet.Files[i].Tracks.Add(tracks[i]);
            }
        }

        void ParseRootCommands()
        {
            bool atFileSection = false;

            foreach (FileLine fileLine in _cueLines.Select(line => new FileLine(line)))
            {
                switch (fileLine.Command)
                {
                    case "REM":
                    {
                        _cueSheet.Comments.Add(new FileLineCommentParser(fileLine).Parse());
                        break;
                    }
                    case "CATALOG":
                    case "CDTEXTFILE":
                    case "PERFORMER":
                    case "SONGWRITER":
                    case "TITLE":
                    {
                        if (atFileSection) break;

                        CueSheetSingleValueCommands[fileLine.Command](
                            _cueSheet,
                            new FileLineSingleValueParser(fileLine).Parse());
                        break;
                    }
                    case "FILE":
                    {
                        atFileSection = true;
                        _cueSheet.Files.Add(new FileLineFileParser(fileLine).Parse());
                        break;
                    }
                }
            }
        }

        void ParseStandardFile()
        {
            ParseRootCommands();

            File file = _cueSheet.Files[0];

            ParseTrackCommands(file.Tracks);
        }

        void ParseTrackCommands(ICollection<Track> tracks)
        {
            for (int i = 0; i < _cueLines.Count; ++i)
            {
                var fileLine = new FileLine(_cueLines[i]);

                switch (fileLine.Command)
                {
                    case "TRACK":
                    {
                        Track track = new FileLineTrackParser(fileLine).Parse();
                        tracks.Add(track);
                        
                        foreach (var trackLine in _cueLines.Skip(i + 1)
                                                      .Select(line => new FileLine(line))
                                                      .TakeWhile(line => line.Command != "TRACK"))
                        {
                            switch (trackLine.Command)
                            {
                                case "FLAGS":
                                case "ISRC":
                                case "PERFORMER":
                                case "SONGWRITER":
                                case "TITLE":
                                {
                                    TrackSingleValueCommands[trackLine.Command](
                                        track,
                                        new FileLineSingleValueParser(trackLine).Parse());
                                    break;
                                }
                                case "INDEX":
                                case "POSTGAP":
                                case "PREGAP":
                                {
                                    TrackIndexCommands[trackLine.Command](
                                        track,
                                        new FileLineIndexParser(trackLine).Parse());
                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}