using System;
using Builder = CuesheetSplitterEncoder.Core.CommandLine.CommandLineBuilder;


namespace CuesheetSplitterEncoder.Core.Encoders
{
    public class EncoderFactory
    {
        readonly decimal _quality;

        public EncoderFactory(decimal quality)
        {
            _quality = quality;
        }

        public IEncoder Build(EncoderType encoderType)
        {
            int qualityAsInt = (int)decimal.Truncate(_quality);

            switch (encoderType)
            {
                case EncoderType.Fhgaacenc:
                {
                    return new AacEncoder(
                        qualityAsInt,
                        (inputFilePath, outputFilePath, quality) =>
                            new Builder("fhgaacenc.exe")
                                .AppendDoubleDash(
                                    "vbr",
                                    qualityAsInt.ToString(),
                                    Builder.SeparatorType.Space,
                                    Builder.QuoteValue.No)
                                .AppendDoubleDash("quiet")
                                .AppendValue(inputFilePath)
                                .AppendValue(outputFilePath)
                                .ToString());
                }
                case EncoderType.Lame:
                {
                    return new Mp3Encoder(
                        qualityAsInt,
                        (inputFilePath, outputFilePath, quality) => 
                            new Builder("lame.exe")
                                .AppendDash(
                                    "V",
                                    qualityAsInt.ToString(),
                                    Builder.SeparatorType.None,
                                    Builder.QuoteValue.No)
                                .AppendDoubleDash("silent")
                                .AppendValue(inputFilePath)
                                .AppendValue(outputFilePath)
                                .ToString());
                }
                case EncoderType.Nero:
                {
                    return new AacEncoder(
                        _quality,
                        (inputFilePath, outputFilePath, quality) =>
                            new Builder("neroaacenc.exe")
                                .AppendDash(
                                    "q", 
                                    quality.ToString("0.00"), 
                                    Builder.SeparatorType.Space, 
                                    Builder.QuoteValue.No)
                                .AppendDash("if", inputFilePath, Builder.SeparatorType.Space)
                                .AppendDash("of", outputFilePath, Builder.SeparatorType.Space)
                                .ToString());
                }
                case EncoderType.OggVorbis:
                {
                    return new OggVorbisEncoder(
                        _quality,
                        (inputFilePath, outputFilePath, quality) => 
                            new Builder("oggenc2.exe")
                                .AppendDash(
                                    "q", 
                                    quality.ToString("0.00"), 
                                    Builder.SeparatorType.Space, 
                                    Builder.QuoteValue.No)
                                .AppendDash("o", outputFilePath, Builder.SeparatorType.Space)
                                .AppendValue(inputFilePath)
                                .ToString());
                }
                case EncoderType.Qaac:
                case EncoderType.Qaac64:
                {
                    return new AacEncoder(
                        qualityAsInt,
                        (inputFilePath, outputFilePath, quality) =>
                            new Builder((encoderType == EncoderType.Qaac) ? "qaac.exe" : "qaac64.exe")
                                .AppendDoubleDash("silent")
                                .AppendDoubleDash(
                                    "tvbr",
                                    ((int)decimal.Truncate(quality)).ToString(),
                                    Builder.SeparatorType.Space,
                                    Builder.QuoteValue.No)
                                .AppendDash("o", outputFilePath, Builder.SeparatorType.Space)
                                .AppendValue(inputFilePath)
                                .ToString());
                }
                default:
                {
                    throw new Exception(string.Format("No encoder is associated with '{0}'", encoderType));
                }
            }
        }
    }
}