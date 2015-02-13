using System;
using System.IO;
using CuesheetSplitterEncoder.Core.Encoders;
using Mono.Options;


namespace CuesheetSplitterEncoder.CmdUi
{
    public class OptionValues
    {
        public OptionValues()
        {
            EncoderQuality = 0;
        }

        public bool ShowHelp { get; set; }

        public string EncoderString { get; set; }

        public decimal EncoderQuality { get; set; }

        public string CueFilePath { get; set; }

        public string OutputPath { get; set; }

        public string CoverPath { get; set; }

        public EncoderType EncoderType { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(EncoderString))
                throw new OptionException("Missing encoder type.", "e");

            EncoderType encoderType;
            if (!Enum.TryParse(EncoderString, true, out encoderType))
            {
                throw new OptionException(string.Format("Encoder '{0}' is not recognized.", EncoderString), "e");
            }

            EncoderType = encoderType;

            if (string.IsNullOrWhiteSpace(CueFilePath))
                throw new OptionException("Missing cue file path arg.", "i");

            if (!File.Exists(CueFilePath))
                throw new Exception(string.Format("Cue file at '{0}' not found.", CueFilePath));

            if (string.IsNullOrWhiteSpace(OutputPath))
                throw new OptionException("Missing output path arg.", "o");

            if (!string.IsNullOrWhiteSpace(CoverPath) && !File.Exists(CoverPath)) 
                throw new Exception(string.Format("Cover file at '{0}' not found.", CoverPath));
        }
    }
}