using System;
using CueSharp;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public class SplitResult
    {
        readonly Track _track;
        readonly string _filePath;

        public SplitResult(Track track, string filePath)
        {
            if (track == null)
                throw new ArgumentNullException("track");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("filePath");

            _track = track;
            _filePath = filePath;
        }

        public Track Track
        {
            get { return _track; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }
    }
}