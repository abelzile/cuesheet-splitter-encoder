using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CueSharp;
using CuesheetSplitterEncoder.Core.Encoders;
using CuesheetSplitterEncoder.Core.Splitters;
using CuesheetSplitterEncoder.Core.Taggers;
using CuesheetSplitterEncoder.Core.Utils;
using Mono.Options;


namespace CuesheetSplitterEncoder.CmdUi
{
    class Program
    {
        static void Main(string[] args)
        {
            var optionValues = new OptionValues();
            var options = new OptionSet
            {
                { "h|help", "Show this message and exit.", s => optionValues.ShowHelp = s != null },
                {
                    "e={:}|encoder={:}",
                    new StringBuilder().AppendLine("The {0:ENCODER TYPE} and {1:QUALITY} value to use. ")
                                       .AppendLine()
                                       .AppendLine("Valid {0:ENCODER TYPE} values are: ")
                                       .AppendLine()
                                       .AppendLine("fhgaacenc")
                                       .AppendLine("nero")
                                       .AppendLine("qaac")
                                       .AppendLine("qaac64")
                                       .AppendLine()
                                       .Append("Numeric {1:QUALITY} values are specific to each encoder ")
                                       .AppendLine("(fhgaacenc uses a value between 1 and 6, qaac uses 0 to 127, etc.) ")
                                       .AppendLine()
                                       .AppendLine("VBR mode is always used.")
                                       .AppendLine()
                                       .Append("These encoders are not distributed with this program. ")
                                       .Append("They must be installed separately and copied to the executable directory ")
                                       .Append("or made accessible via the System PATH environment variable. ")
                                       .Append("Visit http://wiki.hydrogenaud.io/index.php?title=AAC_encoders ")
                                       .Append("to learn more about them.")
                                       .ToString(),
                    (string encStr, decimal encQual) =>
                    {
                        optionValues.EncoderString = encStr;
                        optionValues.EncoderQuality = encQual;
                    }
                },
                {
                    "i=|input=",
                    new StringBuilder().AppendLine("The {PATH} to the cue sheet file.")
                                       .AppendLine()
                                       .Append("FLAC, WavPack and Monkey's Audio* files can be split. ")
                                       .Append("Decoders for these files are not distributed with this program. ")
                                       .Append("They must be installed separately and copied to the executable directory ")
                                       .Append("or made accessible via the System PATH environment variable. ")
                                       .AppendLine()
                                       .AppendLine()
                                       .AppendLine("Non-standard cue sheets are not supported.")
                                       .AppendLine()
                                       .Append("*Ensure the FLAC decoder is installed if splitting Monkey's Audio files. ")
                                       .Append("MAC.exe does not provide any splitting functionality so a transcode to FLAC is required.")
                                       .ToString(),
                    s => optionValues.CueFilePath = s
                },
                { "o=|output=", "The output {PATH}.", s => optionValues.OutputPath = s },
                { "c=|cover=", "The {PATH} to a front cover image.", s => optionValues.CoverPath = s }
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);

                if (optionValues.ShowHelp)
                {
                    options.WriteOptionDescriptions(Console.Out);
                    return;
                }

                optionValues.Validate();

                var cueSheet = new CueSheet(optionValues.CueFilePath);

                string nonStandardReason;
                if (!cueSheet.IsStandard(out nonStandardReason))
                {
                    throw new Exception("Cue sheet appears to be non-standard: " + nonStandardReason);
                }

                cueSheet.ToTitleCase();

                var splitterFactory = new SplitterFactory(cueSheet);
                var encoderFactory = new EncoderFactory(optionValues.EncoderQuality);
                var taggerFactory = new TaggerFactory(cueSheet, optionValues.CoverPath);
                var stopwatch = new Stopwatch();

                IEncoder encoder = encoderFactory.Build(optionValues.EncoderType);
                ITagger tagger = taggerFactory.Build(optionValues.EncoderType);
                using (ISplitter splitter = splitterFactory.Build())
                {
                    Console.WriteLine("Starting...");
                    Console.WriteLine("Splitting cue sheet...");

                    stopwatch.Start();

                    splitter.Split();

                    DirectoryInfo encodedOutputDirInfo = Directory.CreateDirectory(Path.Combine(optionValues.OutputPath, encoder.FileType));

                    int trackCountWidth = cueSheet.Tracks.Count.ToString().Length;

                    Parallel.ForEach(
                        splitter.Results,
                        trackWavPair =>
                        {
                            Track track = trackWavPair.Track;
                            string wavFilePath = trackWavPair.FilePath;
                            string title = track.Title.Trim();

                            Console.WriteLine(
                                "Processing '{0}' (Thread {1})...",
                                title,
                                Thread.CurrentThread.ManagedThreadId);

                            string tempEncodedFilePath = encoder.Encode(wavFilePath, track, tagger);

                            string encodedOutputPath = BuildEncodedFileOutputPath(
                                encodedOutputDirInfo.FullName,
                                title,
                                track.TrackNumber,
                                trackCountWidth,
                                encoder.FileExtension);

                            IOUtils.FileMove(tempEncodedFilePath, encodedOutputPath);
                        });
                }

                Console.WriteLine("Copying original files to output directory...");

                CopyOriginalsToOutputPath(optionValues.OutputPath, optionValues.CueFilePath, cueSheet, optionValues.CoverPath);

                stopwatch.Stop();

                Console.WriteLine("Done. Time elapsed: {0}", stopwatch.Elapsed);

            }
            catch (OptionException e)
            {
                Console.WriteLine("{0} {1}", e.OptionName, e.Message);
                Console.WriteLine("Try '--help' for more information.");
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Click any key to exit.");
                Console.ReadKey();
            }
        }

        static string BuildEncodedFileOutputPath(
            string outputPath,
            string title,
            int trackNum,
            int trackCountWidth,
            string fileExt)
        {
            return Path.Combine(
                outputPath,
                string.Format("{0} - {1}{2}", trackNum.ToString("D" + trackCountWidth), PathUtils.RemoveInvalidFileNameChars(title), fileExt));
        }

        static void CopyOriginalsToOutputPath(string outputPath, string cueFilePath, CueSheet sheet, string coverPath)
        {
            if (!string.IsNullOrWhiteSpace(coverPath))
            {
                IOUtils.FileCopy(coverPath, Path.Combine(outputPath, "cover" + Path.GetExtension(coverPath)));
            }

            DirectoryInfo outputDirInfo = Directory.CreateDirectory(Path.Combine(outputPath, GetOriginalFileDirName(sheet)));

            string cueOutputPath = Path.Combine(outputDirInfo.FullName, Path.GetFileName(cueFilePath));

            IOUtils.FileCopy(cueFilePath, cueOutputPath);

            string sourceFilePath = Path.Combine(Path.GetDirectoryName(cueFilePath), sheet.File);
            string destFilePath = Path.Combine(outputDirInfo.FullName, sheet.File);

            IOUtils.FileCopy(sourceFilePath, destFilePath);
        }

        static string GetOriginalFileDirName(CueSheet sheet)
        {
            string ext = sheet.GetPathExtension();

            return (ext != null) ? ext.ToLowerInvariant().Substring(1) : "original";
        }
    }
}
