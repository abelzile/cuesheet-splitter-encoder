using CuesheetSplitterEncoder.Core.CueSheet;
using CuesheetSplitterEncoder.Core.Taggers;


namespace CuesheetSplitterEncoder.Core.Encoders
{
    public interface IEncoder
    {
        string FileType { get; }

        string FileExtension { get; }

        string Encode(string inputFilePath, Track track, ITagger tagger);
    }
}