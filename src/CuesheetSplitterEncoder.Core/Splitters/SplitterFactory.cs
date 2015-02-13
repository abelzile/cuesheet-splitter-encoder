using System;
using System.IO;
using CueSharp;
using CuesheetSplitterEncoder.Core.CommandLine;
using CuesheetSplitterEncoder.Core.Utils;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public class SplitterFactory
    {
        readonly CueSheet _cueSheet;

        public SplitterFactory(CueSheet cueSheet)
        {
            if (cueSheet == null)
                throw new ArgumentNullException("cueSheet");

            _cueSheet = cueSheet;
        }

        public ISplitter Build()
        {
            string file = _cueSheet.File;
            string ext = Path.GetExtension(file);

            if (ext == null)
                throw new Exception(string.Format("A splitter for '{0}' couldn't be determined. No file extension found.", file));

            ext = ext.ToUpperInvariant();

            switch (ext)
            {
                case ".FLAC":
                {
                    return new Splitter(
                        _cueSheet, 
                        new CueSheetSplitter(
                            _cueSheet, 
                            (filePath, tempWavPath, skip, until) => 
                                new CommandLineBuilder("flac.exe")
                                    .AppendDoubleDash("decode")
                                    .AppendDoubleDash("silent")
                                    .AppendDoubleDash(
                                        "skip",
                                        skip.Samples().ToString(),
                                        CommandLineBuilder.SeparatorType.Equals,
                                        CommandLineBuilder.QuoteValue.No)
                                    .AppendDoubleDashIfNotNull(
                                        "until",
                                        (until != null) ? until.Samples().ToString() : null,
                                        CommandLineBuilder.SeparatorType.Equals,
                                        CommandLineBuilder.QuoteValue.No)
                                    .AppendDash(
                                        "o",
                                        tempWavPath,
                                        CommandLineBuilder.SeparatorType.Space)
                                    .AppendValue(filePath)
                                    .ToString()));
                }
                case ".WV":
                {
                    return new Splitter(
                        _cueSheet, 
                        new CueSheetSplitter(
                            _cueSheet, 
                            (filePath, tempWavPath, skip, until) => 
                                new CommandLineBuilder("wvunpack.exe")
                                    .AppendDash("z")
                                    .AppendDash("q")
                                    .AppendDoubleDash(
                                        "skip",
                                        skip.Samples().ToString(),
                                        CommandLineBuilder.SeparatorType.Equals,
                                        CommandLineBuilder.QuoteValue.No)
                                    .AppendDoubleDashIfNotNull(
                                        "until",
                                        (until != null) ? until.Samples().ToString() : null,
                                        CommandLineBuilder.SeparatorType.Equals,
                                        CommandLineBuilder.QuoteValue.No)
                                    .AppendValue(filePath)
                                    .AppendValue(tempWavPath)
                                    .ToString()));
                }
                case ".APE":
                {
                    return new ApeSplitter(_cueSheet, new SplitterFactory(_cueSheet));
                }
            }

            throw new Exception(string.Format("A splitter for '{0}' couldn't be determined. File extension didn't match a known splitter.", file));
        }
    }
}