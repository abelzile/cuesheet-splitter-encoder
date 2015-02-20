namespace CuesheetSplitterEncoder.Core.CueSheet.Parsers
{
    public interface IParser<T>
    {
        T Parse();
    }
}