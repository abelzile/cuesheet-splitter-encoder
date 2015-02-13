using System;
using System.Collections.Generic;
using CueSharp;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public interface ISplitter : IDisposable
    {
        CueSheet CueSheet { get; }

        IEnumerable<SplitResult> Results { get; }
            
        void Split();
    }
}