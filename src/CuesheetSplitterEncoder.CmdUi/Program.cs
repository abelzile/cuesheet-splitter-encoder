using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuesheetSplitterEncoder.Core.CueSheet;
using CuesheetSplitterEncoder.Core.CueSheet.Parsers;
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
                                       .AppendLine("lame")
                                       .AppendLine("nero")
                                       .AppendLine("oggvorbis")
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

                var cueSheetParser = new CueSheetParser(optionValues.CueFilePath);

                CueSheet cueSheet = cueSheetParser.Parse().ToTitleCase();
                
                var splitterFactory = new SplitterFactory(cueSheet, optionValues.CueFilePath);
                var encoderFactory = new EncoderFactory(optionValues.EncoderQuality);
                var taggerFactory = new TaggerFactory(cueSheet, optionValues.CoverPath);
                var stopwatch = new Stopwatch();

                IEncoder encoder = encoderFactory.Build(optionValues.EncoderType);
                ITagger tagger = taggerFactory.Build(optionValues.EncoderType);
                using (ISplitter splitter = splitterFactory.Build())
                {
                    Console.WriteLine("Starting...");
                    Console.WriteLine("Splitting {0} cue sheet into WAV files...", BuildCuesheetTypeStr(cueSheet));

                    stopwatch.Start();

                    splitter.Split();

                    DirectoryInfo encodedOutputDirInfo = Directory.CreateDirectory(Path.Combine(optionValues.OutputPath, encoder.FileType));

                    int trackCountWidth = cueSheet.IsStandard
                        ? cueSheet.Files[0].Tracks.Count.ToString().Length
                        : cueSheet.Files.Count.ToString().Length;

                    Parallel.ForEach(
                        splitter.Results,
                        trackWavPair =>
                        {
                            Track track = trackWavPair.Track;
                            string wavFilePath = trackWavPair.FilePath;
                            string title = track.Title.Trim();

                            Console.WriteLine(
                                "Encoding '{0}' to {1} (Thread {2})...",
                                title,
                                encoder.FileType,
                                Thread.CurrentThread.ManagedThreadId);

                            string tempEncodedFilePath = encoder.Encode(wavFilePath, track, tagger);

                            string encodedOutputPath = BuildEncodedFileOutputPath(
                                encodedOutputDirInfo.FullName,
                                title,
                                track.TrackNum,
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

        static string BuildCuesheetTypeStr(CueSheet cueSheet)
        {
            string msg;

            if (cueSheet.IsStandard)
            {
                msg = "standard";
            }
            else
            {
                msg = "nonstandard";

                if (cueSheet.IsNoncompliant)
                {
                    msg += ", noncompliant";
                }
            }

            return msg;
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
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentNullException("outputPath");

            if (string.IsNullOrWhiteSpace(cueFilePath))
                throw new ArgumentNullException("cueFilePath");

            if (sheet == null)
                throw new ArgumentNullException("sheet");

            if (!string.IsNullOrWhiteSpace(coverPath))
            {
                IOUtils.FileCopy(coverPath, Path.Combine(outputPath, "cover" + Path.GetExtension(coverPath)));
            }

            DirectoryInfo outputDirInfo = Directory.CreateDirectory(Path.Combine(outputPath, GetOriginalFileDirName(sheet)));

            string cueOutputPath = Path.Combine(outputDirInfo.FullName, Path.GetFileName(cueFilePath));

            IOUtils.FileCopy(cueFilePath, cueOutputPath);

            foreach (var file in sheet.Files)
            {
                string cueDirName = Path.GetDirectoryName(cueFilePath);

                if (cueDirName == null)
                    throw new Exception("Cue file path is missing directory.");

                IOUtils.FileCopy(
                    Path.Combine(cueDirName, file.FileName), 
                    Path.Combine(outputDirInfo.FullName, file.FileName));
            }
        }

        static string GetOriginalFileDirName(CueSheet sheet)
        {
            string ext = Path.GetExtension(sheet.Files[0].FileName);

            return (ext != null) ? ext.ToLowerInvariant().Substring(1) : "original";
        }
    }
}
