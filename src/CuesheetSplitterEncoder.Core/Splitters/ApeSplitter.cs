using System;
using System.Collections.Generic;
using System.IO;
using CuesheetSplitterEncoder.Core.CommandLine;
using CuesheetSplitterEncoder.Core.Exceptions;
using CuesheetSplitterEncoder.Core.Utils;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public class ApeSplitter : ISplitter
    {
        readonly CueSheet.CueSheet _cueSheet;
        readonly SplitterFactory _factory;
        IEnumerable<SplitResult> _results;
        readonly string _apeFilePath;
        string _tempWavPath;
        string _tempFlacPath;

        public ApeSplitter(CueSheet.CueSheet cueSheet, string cueFilePath, SplitterFactory factory)
        {
            if (cueSheet == null)
                throw new ArgumentNullException("cueSheet");

            _cueSheet = cueSheet;
            _factory = factory;

            string cueDir = Path.GetDirectoryName(cueFilePath);

            if (cueDir == null)
                throw new Exception("Cue file directory is null.");
            
            _apeFilePath = Path.Combine(cueDir, _cueSheet.Files[0].FileName);
        }

        public CueSheet.CueSheet CueSheet
        {
            get { return _cueSheet; }
        }

        public IEnumerable<SplitResult> Results
        {
            get { return _results; }
        }

        public void Split()
        {
            var apeToWavCmd = new CommandLineRunner(BuildApeToWavArgs(out _tempWavPath));
            apeToWavCmd.Run();

            if (apeToWavCmd.ExitCode != 0)
                throw new CommandLineOperationException(apeToWavCmd.StandardError, apeToWavCmd.ExitCode);

            var wavToFlacCmd = new CommandLineRunner(BuildWavToFlacCmd(_tempWavPath, out _tempFlacPath));
            wavToFlacCmd.Run();

            if (wavToFlacCmd.ExitCode != 0)
                throw new CommandLineOperationException(wavToFlacCmd.StandardError, wavToFlacCmd.ExitCode);
            
            string orig = _cueSheet.Files[0].FileName;
            _cueSheet.Files[0].FileName = _tempFlacPath;

            var splitter = _factory.Build();
            splitter.Split();

            _cueSheet.Files[0].FileName = orig;

            _results = splitter.Results;
        }

        string BuildWavToFlacCmd(string tempApeWavPath, out string tempFlacPath)
        {
            tempFlacPath = Path.Combine(Path.GetTempPath(), string.Format("{0}.flac", Guid.NewGuid().ToString("N")));

            return new CommandLineBuilder("flac.exe")
                .AppendDash("0")
                .AppendDoubleDash("delete-input-file")
                .AppendDash(
                    "o",
                    tempFlacPath,
                    CommandLineBuilder.SeparatorType.Space)
                .AppendValue(tempApeWavPath)
                .ToString();
        }

        string BuildApeToWavArgs(out string tempApeWavPath)
        {
            tempApeWavPath = Path.Combine(Path.GetTempPath(), string.Format("{0}.wav", Guid.NewGuid().ToString("N")));

            return new CommandLineBuilder("mac.exe")
                .AppendValue(_apeFilePath)
                .AppendValue(tempApeWavPath)
                .AppendDash("d")
                .ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (SplitResult result in _results)
                {
                    IOUtils.FileDelete(result.FilePath);
                }

                IOUtils.FileDelete(_tempWavPath);

                IOUtils.FileDelete(_tempFlacPath);
            }
        }
    }
}