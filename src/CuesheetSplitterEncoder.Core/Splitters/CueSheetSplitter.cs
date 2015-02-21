using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuesheetSplitterEncoder.Core.CommandLine;
using CuesheetSplitterEncoder.Core.CueSheet;
using CuesheetSplitterEncoder.Core.Exceptions;
using CuesheetSplitterEncoder.Core.Utils;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public class CueSheetSplitter
    {
        readonly CueSheet.CueSheet _cueSheet;
        readonly string _cueFilePath;
        readonly Func<string, string, Index, Index, string> _buildArgsFunc;

        public CueSheetSplitter(CueSheet.CueSheet cueSheet, string cueFilePath, Func<string, string, Index, Index, string> buildArgsFunc)
        {
            if (cueSheet == null)
                throw new ArgumentNullException("cueSheet");

            if (string.IsNullOrWhiteSpace(cueFilePath))
                throw new ArgumentNullException("cueFilePath");

            if (buildArgsFunc == null)
                throw new ArgumentNullException("buildArgsFunc");

            _cueSheet = cueSheet;
            _cueFilePath = cueFilePath;
            _buildArgsFunc = buildArgsFunc;
        }

        public IEnumerable<SplitResult> Split()
        {
            var results = new List<SplitResult>();

            IList<Track> tracks = _cueSheet.IsStandard
                ? _cueSheet.Files[0].Tracks
                : _cueSheet.Files.Select(file => file.Tracks[0]).ToList();

            for (int i = 0; i < tracks.Count; ++i)
            {
                Track track = tracks[i];

                string tempWavPath = Path.Combine(
                    Path.GetTempPath(),
                    string.Format("{0}-{1}.wav", track.TrackNum, Guid.NewGuid().ToString("N")));

                Index skip = track.FindIndexByNumber(1);

                if (skip == null) 
                    throw new Exception(string.Format("Index 1 not found in cue sheet for track '{0}'.", track.Title));

                Index until = null;

                if (_cueSheet.IsStandard)
                {
                    Track nextTrack = CueSheetExt.Next(tracks, track);

                    if (nextTrack != null)
                    {
                        until = nextTrack.FindIndexByNumber(1);

                        if (until == null)
                            throw new Exception(string.Format("Index 1 not found in cue sheet for track '{0}'.", nextTrack.Title));
                    }
                }

                string fileName = _cueSheet.IsStandard ? _cueSheet.Files[0].FileName : _cueSheet.Files[i].FileName;

                string filePath;
                if (Path.IsPathRooted(fileName))
                {
                    filePath = fileName;
                }
                else
                {
                    string cueDirName = Path.GetDirectoryName(_cueFilePath);

                    if (cueDirName == null) 
                        throw new Exception("Cue file directory is null.");

                    filePath = Path.Combine(cueDirName, fileName);
                }

                var splitterCmd = new CommandLineRunner(_buildArgsFunc(filePath, tempWavPath, skip, until));
                splitterCmd.Run();
                
                if (splitterCmd.ExitCode != 0)
                    throw new CommandLineOperationException(splitterCmd.StandardError, splitterCmd.ExitCode);
                
                results.Add(new SplitResult(track, tempWavPath));
            }

            return results;
        }
    }
}