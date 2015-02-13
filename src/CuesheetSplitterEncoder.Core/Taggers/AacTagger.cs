using System;
using CueSharp;
using CuesheetSplitterEncoder.Core.Utils;
using TagLib;


namespace CuesheetSplitterEncoder.Core.Taggers
{
    public class AacTagger : ITagger
    {
        readonly CueSheet _cueSheet;
        readonly string _coverFilePath;

        public AacTagger(CueSheet cueSheet, string coverFilePath)
        {
            if (cueSheet == null)
                throw new ArgumentNullException("cueSheet");

            _cueSheet = cueSheet;
            _coverFilePath = coverFilePath;
        }

        public void Tag(string inputFilePath, Track track)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath))
                throw new ArgumentNullException("inputFilePath");

            if (track == null)
                throw new ArgumentNullException("track");

            string date;
            bool hasDate = _cueSheet.TryGetComment("DATE", out date);
            bool hasTrackNum = track.TrackNumber != 0;

            using (File file = File.Create(inputFilePath))
            {
                file.Tag.Title = track.Title;
                file.Tag.Performers = new[] { _cueSheet.Performer };
                if (hasDate)
                {
                    file.Tag.Year = uint.Parse(date);
                }
                file.Tag.Album = _cueSheet.Title;
                if (hasTrackNum)
                {
                    file.Tag.Track = (uint)track.TrackNumber;
                }
                file.Tag.TrackCount = (uint)_cueSheet.Tracks.Count;

                if (!string.IsNullOrWhiteSpace(_coverFilePath))
                {
                    file.Tag.Pictures = new IPicture[] { new Picture(_coverFilePath) };
                }

                file.Save();
            }
        }
    }
}