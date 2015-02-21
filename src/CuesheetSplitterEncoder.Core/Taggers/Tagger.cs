using System;
using CuesheetSplitterEncoder.Core.CueSheet;
using TagLib;
using File = TagLib.File;


namespace CuesheetSplitterEncoder.Core.Taggers
{
    public abstract class Tagger : ITagger
    {
        readonly string _coverFilePath;
        readonly CueSheet.CueSheet _cueSheet;

        protected Tagger(CueSheet.CueSheet cueSheet, string coverFilePath)
        {
            if (cueSheet == null) 
                throw new ArgumentNullException("cueSheet");

            _cueSheet = cueSheet;
            _coverFilePath = coverFilePath;
        }

        protected string CoverFilePath
        {
            get { return _coverFilePath; }
        }

        protected CueSheet.CueSheet CueSheet
        {
            get { return _cueSheet; }
        }

        public void Tag(string inputFilePath, Track track)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath)) 
                throw new ArgumentNullException("inputFilePath");

            if (track == null) 
                throw new ArgumentNullException("track");

            using (File file = File.Create(inputFilePath))
            {
                Tag tag = file.Tag;

                SetCommonFields(track, tag);

                SetPicture(file, tag);

                file.Save();
            }
        }

        protected virtual void SetCommonFields(Track track, Tag tag)
        {
            if (!string.IsNullOrWhiteSpace(track.Title))
            {
                tag.Title = track.Title;
            }

            if (!string.IsNullOrWhiteSpace(CueSheet.Performer))
            {
                tag.Performers = new[] { CueSheet.Performer };
            }

            string date;
            if (CueSheet.TryGetCommentValue("DATE", out date))
            {
                tag.Year = uint.Parse(date);
            }

            if (!string.IsNullOrWhiteSpace(CueSheet.Title))
            {
                tag.Album = CueSheet.Title;
            }

            if (!string.IsNullOrWhiteSpace(track.SongWriter))
            {
                tag.Composers = new[] { track.SongWriter };
            }

            if (track.TrackNum > 0)
            {
                tag.Track = (uint)track.TrackNum;
                
                tag.TrackCount = CueSheet.IsStandard 
                    ? (uint)CueSheet.Files[0].Tracks.Count 
                    : (uint)CueSheet.Files.Count;
            }
        }

        protected abstract void SetPicture(File file, Tag tag);
    }
}