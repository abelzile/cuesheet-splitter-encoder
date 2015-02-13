using System;
using System.Collections.Generic;
using System.IO;
using CueSharp;
using CuesheetSplitterEncoder.Core.CommandLine;
using CuesheetSplitterEncoder.Core.Utils;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public class CueSheetSplitter
    {
        readonly CueSheet _cueSheet;
        readonly string _filePath;
        readonly Func<string, string, Index, Index, string> _buildArgsFunc;

        public CueSheetSplitter(CueSheet cueSheet, Func<string, string, Index, Index, string> buildArgsFunc)
        {
            if (cueSheet == null)
                throw new ArgumentNullException("cueSheet");

            if (buildArgsFunc == null)
                throw new ArgumentNullException("buildArgsFunc");

            _cueSheet = cueSheet;
            _buildArgsFunc = buildArgsFunc;

            if (Path.IsPathRooted(_cueSheet.File))
            {
                _filePath = _cueSheet.File;
            }
            else
            {
                string cueDir = Path.GetDirectoryName(_cueSheet.CueFileName);

                if (cueDir == null)
                    throw new Exception("Cue file directory is null.");

                _filePath = Path.Combine(cueDir, _cueSheet.File);
            }
        }

        public IEnumerable<SplitResult> Split()
        {
            var results = new List<SplitResult>();

            foreach (Track track in _cueSheet.Tracks)
            {
                string tempWavPath = Path.Combine(
                    Path.GetTempPath(),
                    string.Format("{0}-{1}.wav", track.TrackNumber, Guid.NewGuid().ToString("N")));

                Index skip = track.FindIndexByNumber(1);

                if (skip == null)
                    throw new Exception(string.Format("Index 1 not found in cue sheet for track '{0}'.", track.Title));

                Index until = null;
                Track nextTrack = _cueSheet.Tracks.Next(track);

                if (nextTrack != null)
                {
                    until = nextTrack.FindIndexByNumber(1);

                    if (until == null)
                        throw new Exception(string.Format((string)"Index 1 not found in cue sheet for track '{0}'.", (object)nextTrack.Title));
                }

                var runner = new CommandLineRunner(_buildArgsFunc(_filePath, tempWavPath, skip, until));
                runner.Run();

                results.Add(new SplitResult(track, tempWavPath));
            }

            return results;
        }
    }
}