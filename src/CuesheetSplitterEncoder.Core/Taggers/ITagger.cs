using CueSharp;


namespace CuesheetSplitterEncoder.Core.Taggers
{
    public interface ITagger
    {
        void Tag(string inputFilePath, Track track);
    }
}