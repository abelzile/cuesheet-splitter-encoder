using System;
using System.IO;
using CuesheetSplitterEncoder.Core.CommandLine;
using CuesheetSplitterEncoder.Core.CueSheet;
using CuesheetSplitterEncoder.Core.Taggers;


namespace CuesheetSplitterEncoder.Core.Encoders
{
    public class Encoder : IEncoder
    {
        readonly decimal _quality;
        readonly Func<string, string, decimal, string> _buildArgsFunc;
        
        public Encoder(decimal quality, Func<string, string, decimal, string> buildArgsFunc)
        {
            _quality = quality;
            _buildArgsFunc = buildArgsFunc;
        }

        public virtual string FileType { get; private set; }

        public virtual string FileExtension { get; private set; }

        public string Encode(string inputFilePath, Track track, ITagger tagger)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath))
                throw new ArgumentNullException("inputFilePath");

            if (track == null)
                throw new ArgumentNullException("track");

            if (tagger == null)
                throw new ArgumentNullException("tagger");

            var outputFilePath = BuildOutputFilePath(track);

            string buildArgsFunc = _buildArgsFunc(inputFilePath, outputFilePath, _quality);

            var runner = new CommandLineRunner(buildArgsFunc);
            runner.Run();

            tagger.Tag(outputFilePath, track);

            return outputFilePath;
        }

        string BuildOutputFilePath(Track track)
        {
            return Path.Combine(
                Path.GetTempPath(),
                string.Format("{0}-{1}{2}", track.TrackNum, Guid.NewGuid().ToString("N"), FileExtension));
        }
    }
}