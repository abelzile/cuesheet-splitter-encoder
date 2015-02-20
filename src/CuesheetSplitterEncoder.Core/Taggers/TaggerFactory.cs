using System;
using CuesheetSplitterEncoder.Core.Encoders;


namespace CuesheetSplitterEncoder.Core.Taggers
{
    public class TaggerFactory
    {
        readonly CueSheet.CueSheet _cueSheet;
        readonly string _coverFilePath;

        public TaggerFactory(CueSheet.CueSheet cueSheet, string coverFilePath)
        {
            _cueSheet = cueSheet;
            _coverFilePath = coverFilePath;
        }

        public TaggerFactory(CueSheet.CueSheet cueSheet)
            : this(cueSheet, null)
        {
        }

        public ITagger Build(EncoderType encoderType)
        {
            switch (encoderType)
            {
                case EncoderType.Fhgaacenc:
                case EncoderType.Nero:
                case EncoderType.Qaac:
                case EncoderType.Qaac64:
                {
                    return new AacTagger(_cueSheet, _coverFilePath);
                }
            }

            throw new Exception(string.Format("No tagger is associated with '{0}'", encoderType));
        }
    }
}