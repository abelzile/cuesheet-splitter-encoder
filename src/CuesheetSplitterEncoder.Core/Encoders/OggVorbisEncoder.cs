using System;


namespace CuesheetSplitterEncoder.Core.Encoders
{
    public class OggVorbisEncoder : Encoder
    {
        public OggVorbisEncoder(decimal quality, Func<string, string, decimal, string> buildArgsFunc)
            : base(quality, buildArgsFunc)
        {
        }

        public override string FileType
        {
            get { return "ogg"; }
        }

        public override string FileExtension
        {
            get { return ".ogg"; }
        }
    }
}