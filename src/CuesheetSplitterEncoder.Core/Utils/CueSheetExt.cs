using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CueSharp;


namespace CuesheetSplitterEncoder.Core.Utils
{
    public static class CueSheetExt
    {
        const decimal FrameInMs = 13.333333333333333333333333333M;

        public static CueSheet ToTitleCase(this CueSheet cueSheet)
        {
            cueSheet.Performer = WordUtils.ToTitleCase(cueSheet.Performer);
            cueSheet.Songwriter = WordUtils.ToTitleCase(cueSheet.Songwriter);
            cueSheet.Title = WordUtils.ToTitleCase(cueSheet.Title);
            cueSheet.Tracks.ForEach(t => t.ToTitleCase());

            return cueSheet;
        }

        public static bool TryGetComment(this CueSheet cueSheet, string commentName, out string value)
        {
            value = (from comment in cueSheet.Comments
                     where comment.StartsWith(commentName, StringComparison.OrdinalIgnoreCase)
                     select comment.Substring(commentName.Length).Trim()).FirstOrDefault() ?? "";

            return (value != "");
        }

        public static string GetPathExtension(this CueSheet cueSheet)
        {
            return Path.GetExtension(cueSheet.File);
        }

        public static bool IsStandard(this CueSheet cueSheet, out string nonStandardReason)
        {
            nonStandardReason = "";

            if (cueSheet.CueLines.Count(s => s.StartsWith("FILE")) > 1)
            {
                nonStandardReason = "Multiple FILE lines found.";
            }
            
            return nonStandardReason == "";
        }

        public static Track ToTitleCase(this Track track)
        {
            track.Performer = WordUtils.ToTitleCase(track.Performer);
            track.Songwriter = WordUtils.ToTitleCase(track.Songwriter);
            track.Title = WordUtils.ToTitleCase(track.Title);

            return track;
        }
        
        public static Track Next<T>(this T tracks, Track currentTrack) where T : List<Track>, IList<Track>
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                Track t = tracks[i];

                if (t != currentTrack) continue;

                if (i + 1 < tracks.Count)
                {
                    return tracks[i + 1];
                }
            }

            return null;
        }

        public static Index FindIndexByNumber(this Track track, int indexNumber)
        {
            return (from index in track.Indices
                    where index.Number == indexNumber
                    select index).FirstOrDefault();
        }

        public static int Samples(this Index index)
        {
            return ((((60 * index.Minutes) + index.Seconds) * 75) + index.Frames) * 588;
        }

        public static int Milliseconds(this Index index)
        {
            return (int)Math.Truncate(index.Frames * FrameInMs);
        }
    }
}