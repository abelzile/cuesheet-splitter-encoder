using System;
using CuesheetSplitterEncoder.Core.CueSheet;
using TagLib;


namespace CuesheetSplitterEncoder.Core.Taggers
{
    public class AacTagger : ITagger
    {
        readonly CueSheet.CueSheet _cueSheet;
        readonly string _coverFilePath;

        public AacTagger(CueSheet.CueSheet cueSheet, string coverFilePath)
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
            bool hasDate = _cueSheet.TryGetCommentValue("DATE", out date);
            bool hasTrackNum = track.TrackNum != 0;

            using (TagLib.File file = TagLib.File.Create(inputFilePath))
            {
                Tag tag = file.Tag;

                tag.Title = track.Title;
                tag.Performers = new[] { _cueSheet.Performer };
                if (hasDate)
                {
                    tag.Year = uint.Parse(date);
                }
                tag.Album = _cueSheet.Title;
                if (hasTrackNum)
                {
                    tag.Track = (uint)track.TrackNum;
                }

                tag.TrackCount = _cueSheet.IsStandard
                    ? (uint)_cueSheet.Files[0].Tracks.Count
                    : (uint)_cueSheet.Files.Count;

                if (!string.IsNullOrWhiteSpace(_coverFilePath))
                {
                    tag.Pictures = new IPicture[] { new Picture(_coverFilePath) };
                }

                file.Save();
            }
        }
    }
}