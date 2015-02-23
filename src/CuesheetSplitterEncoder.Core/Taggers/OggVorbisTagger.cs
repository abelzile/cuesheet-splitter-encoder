using System;
using System.Reflection;
using TagLib;
using TagLib.Ogg;


namespace CuesheetSplitterEncoder.Core.Taggers
{
    public class OggVorbisTagger : Tagger
    {
        public OggVorbisTagger(CueSheet.CueSheet cueSheet, string coverFilePath)
            : base(cueSheet, coverFilePath)
        {
        }

        protected override void SetPicture(TagLib.File file, Tag tag)
        {
            TagLib.Ogg.File oggFile = (TagLib.Ogg.File)file;
            GroupedComment groupedCommentTag = (GroupedComment)tag;

            if (string.IsNullOrWhiteSpace(CoverFilePath)) return;

            // Is there a way to get Ogg file header using TagLib#?
            PropertyInfo headerProp = oggFile.GetType().GetProperty("LastPageHeader", BindingFlags.Instance | BindingFlags.NonPublic);
            PageHeader header = (PageHeader)headerProp.GetValue(oggFile);

            // Add cover art to Vorbis Comment approved METADATA_BLOCK_PICTURE field.
            TagLib.Flac.Picture pic = new TagLib.Flac.Picture(new Picture(CoverFilePath)) { Description = "" };
            ByteVector picData = pic.Render();

            XiphComment xiphComment = groupedCommentTag.GetComment(header.StreamSerialNumber);
            xiphComment.SetField("METADATA_BLOCK_PICTURE", Convert.ToBase64String(picData.Data));
        }
    }
}