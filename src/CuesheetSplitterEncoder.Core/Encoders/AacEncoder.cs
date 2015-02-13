using System;


namespace CuesheetSplitterEncoder.Core.Encoders
{
    public class AacEncoder : Encoder
    {
        public AacEncoder(decimal quality, Func<string, string, decimal, string> buildArgsFunc)
            : base(quality, buildArgsFunc)
        {
        }

        public override string FileType
        {
            get { return "aac"; }
        }

        public override string FileExtension
        {
            get { return ".m4a"; }
        }
    }
}