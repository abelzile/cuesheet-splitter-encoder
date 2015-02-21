using TagLib;


namespace CuesheetSplitterEncoder.Core.Taggers
{
    public class SimpleTagger : Tagger
    {
        public SimpleTagger(CueSheet.CueSheet cueSheet, string coverFilePath)
            : base(cueSheet, coverFilePath)
        {
        }

        protected override void SetPicture(File file, Tag tag)
        {
            if (!string.IsNullOrWhiteSpace(CoverFilePath))
            {
                tag.Pictures = new IPicture[] { new Picture(CoverFilePath) };
            }
        }
    }
}