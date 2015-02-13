using System;
using CuesheetSplitterEncoder.Core.CommandLine;


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
            switch (encoderType)
            {
                case EncoderType.Fhgaacenc:
                {
                    return new AacEncoder(
                        (int)decimal.Truncate(_quality),
                        (inputFilePath, outputFilePath, quality) =>
                            new CommandLineBuilder("fhgaacenc.exe")
                                .AppendDoubleDash(
                                    "vbr",
                                    ((int)decimal.Truncate(_quality)).ToString(),
                                    CommandLineBuilder.SeparatorType.Space,
                                    CommandLineBuilder.QuoteValue.No)
                                .AppendValue(inputFilePath)
                                .AppendValue(outputFilePath)
                                .ToString());
                }
                case EncoderType.Nero:
                {
                    return new AacEncoder(
                        _quality,
                        (inputFilePath, outputFilePath, quality) =>
                            new CommandLineBuilder("neroaacenc.exe")
                                .AppendDash(
                                    "q", 
                                    quality.ToString("0.00"), 
                                    CommandLineBuilder.SeparatorType.Space, 
                                    CommandLineBuilder.QuoteValue.No)
                                .AppendDash("if", inputFilePath, CommandLineBuilder.SeparatorType.Space)
                                .AppendDash("of", outputFilePath, CommandLineBuilder.SeparatorType.Space)
                                .ToString());
                }
                case EncoderType.Qaac:
                {
                    return new AacEncoder(
                        (int)decimal.Truncate(_quality),
                        (inputFilePath, outputFilePath, quality) =>
                            new CommandLineBuilder("qaac.exe")
                                .AppendDoubleDash("silent")
                                .AppendDoubleDash(
                                    "tvbr",
                                    ((int)decimal.Truncate(quality)).ToString(),
                                    CommandLineBuilder.SeparatorType.Space,
                                    CommandLineBuilder.QuoteValue.No)
                                .AppendDash("o", outputFilePath, CommandLineBuilder.SeparatorType.Space)
                                .AppendValue(inputFilePath)
                                .ToString());
                }
                case EncoderType.Qaac64:
                {
                    return new AacEncoder(
                        (int)decimal.Truncate(_quality),
                        (inputFilePath, outputFilePath, quality) =>
                            new CommandLineBuilder("qaac64.exe")
                                .AppendDoubleDash("silent")
                                .AppendDoubleDash(
                                    "tvbr",
                                    ((int)decimal.Truncate(quality)).ToString(),
                                    CommandLineBuilder.SeparatorType.Space,
                                    CommandLineBuilder.QuoteValue.No)
                                .AppendDash("o", outputFilePath, CommandLineBuilder.SeparatorType.Space)
                                .AppendValue(inputFilePath)
                                .ToString());
                }
            }

            throw new Exception(string.Format("No encoder is associated with '{0}'", encoderType));
        }
    }
}