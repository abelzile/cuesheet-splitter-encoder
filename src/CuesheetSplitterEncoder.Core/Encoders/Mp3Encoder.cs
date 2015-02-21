using System;


namespace CuesheetSplitterEncoder.Core.Encoders
{
    public class Mp3Encoder : Encoder
    {
        public Mp3Encoder(decimal quality, Func<string, string, decimal, string> buildArgsFunc)
            : base(quality, buildArgsFunc)
        {
        }

        public override string FileType
        {
            get { return "mp3"; }
        }

        public override string FileExtension
        {
            get { return ".mp3"; }
        }
    }
}