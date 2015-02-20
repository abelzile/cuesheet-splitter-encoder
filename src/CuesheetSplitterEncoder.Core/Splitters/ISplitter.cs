using System;
using System.Collections.Generic;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public interface ISplitter : IDisposable
    {
        CueSheet.CueSheet CueSheet { get; }

        IEnumerable<SplitResult> Results { get; }
            
        void Split();
    }
}