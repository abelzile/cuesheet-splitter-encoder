using System;
using System.IO;
using CuesheetSplitterEncoder.Core.CommandLine;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public class SplitterFactory
    {
        readonly CueSheet.CueSheet _cueSheet;
        readonly string _cueFilePath;

        public SplitterFactory(CueSheet.CueSheet cueSheet, string cueFilePath)
        {
            if (cueSheet == null)
                throw new ArgumentNullException("cueSheet");

            if (string.IsNullOrWhiteSpace(cueFilePath))
                throw new ArgumentNullException("cueFilePath");

            _cueSheet = cueSheet;
            _cueFilePath = cueFilePath;
        }

        public ISplitter Build()
        {
            string file = _cueSheet.Files[0].FileName;
            string ext = Path.GetExtension(file);

            if (ext == null)
                throw new Exception(string.Format("A splitter for '{0}' couldn't be determined. File extension didn't match a known splitter.", file));

            switch (ext.ToUpperInvariant())
            {
                case ".FLAC":
                {
                    return new Splitter(
                        _cueSheet, 
                        new CueSheetSplitter(
                            _cueSheet, 
                            _cueFilePath,
                            (filePath, tempWavPath, skip, until) =>
                            {
                                string cmd = new CommandLineBuilder("flac.exe")
                                    .AppendDoubleDash("decode")
                                    .AppendDoubleDash("silent")
                                    .AppendDoubleDash(
                                        "skip",
                                        skip.IndexTime.Samples.ToString(),
                                        CommandLineBuilder.SeparatorType.Equals,
                                        CommandLineBuilder.QuoteValue.No)
                                    .AppendDoubleDashIfNotNull(
                                        "until",
                                        (until != null) ? until.IndexTime.Samples.ToString() : null,
                                        CommandLineBuilder.SeparatorType.Equals,
                                        CommandLineBuilder.QuoteValue.No)
                                    .AppendDash(
                                        "o",
                                        tempWavPath,
                                        CommandLineBuilder.SeparatorType.Space)
                                    .AppendValue(filePath)
                                    .ToString();

                                return cmd;
                            }));
                }
                case ".WV":
                {
                    return new Splitter(
                        _cueSheet, 
                        new CueSheetSplitter(
                            _cueSheet,
                            _cueFilePath,
                            (filePath, tempWavPath, skip, until) =>
                            {
                                string cmd = new CommandLineBuilder("wvunpack.exe")
                                    .AppendDash("z")
                                    .AppendDash("q")
                                    .AppendDoubleDash(
                                        "skip",
                                        skip.IndexTime.Samples.ToString(),
                                        CommandLineBuilder.SeparatorType.Equals,
                                        CommandLineBuilder.QuoteValue.No)
                                    .AppendDoubleDashIfNotNull(
                                        "until",
                                        (until != null) ? until.IndexTime.Samples.ToString() : null,
                                        CommandLineBuilder.SeparatorType.Equals,
                                        CommandLineBuilder.QuoteValue.No)
                                    .AppendValue(filePath)
                                    .AppendValue(tempWavPath)
                                    .ToString();

                                return cmd;
                            }));
                }
                case ".APE":
                {
                    return new ApeSplitter(_cueSheet, _cueFilePath, new SplitterFactory(_cueSheet, _cueFilePath));
                }
            }

            throw new Exception(string.Format("A splitter for '{0}' couldn't be determined. File extension didn't match a known splitter.", file));
        }
    }
}