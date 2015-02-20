using System.Collections.Generic;
using System.Linq;
using CuesheetSplitterEncoder.Core.CueSheet;


namespace CuesheetSplitterEncoder.Core.Utils
{
    public static class CueSheetExt
    {
        public static CueSheet.CueSheet ToTitleCase(this CueSheet.CueSheet cueSheet)
        {
            cueSheet.Performer = WordUtils.ToTitleCase(cueSheet.Performer);
            cueSheet.SongWriter = WordUtils.ToTitleCase(cueSheet.SongWriter);
            cueSheet.Title = WordUtils.ToTitleCase(cueSheet.Title);
            cueSheet.Files.SelectMany(file => file.Tracks).ToList().ForEach(x => x.ToTitleCase());

            return cueSheet;
        }

        public static Track ToTitleCase(this Track track)
        {
            track.Performer = WordUtils.ToTitleCase(track.Performer);
            track.SongWriter = WordUtils.ToTitleCase(track.SongWriter);
            track.Title = WordUtils.ToTitleCase(track.Title);

            return track;
        }
        
        public static Track Next(IList<Track> tracks, Track currentTrack)
        {
            int indexOf = tracks.IndexOf(currentTrack);

            return (indexOf + 1 < tracks.Count) ? tracks[indexOf + 1] : null;
        }
    }
}