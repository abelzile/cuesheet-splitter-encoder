using System;


namespace CuesheetSplitterEncoder.Core.Exceptions
{
    public class CueSheetSplitterEncoderException : Exception
    {
        public CueSheetSplitterEncoderException()
        {
        }

        public CueSheetSplitterEncoderException(string message)
            : base(message)
        {
        }

        public CueSheetSplitterEncoderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}