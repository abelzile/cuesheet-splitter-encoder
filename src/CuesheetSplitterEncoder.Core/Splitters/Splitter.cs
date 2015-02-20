using System;
using System.Collections.Generic;
using CuesheetSplitterEncoder.Core.Utils;


namespace CuesheetSplitterEncoder.Core.Splitters
{
    public class Splitter : ISplitter
    {
        readonly CueSheet.CueSheet _cueSheet;
        readonly CueSheetSplitter _cueSheetSplitter;
        IEnumerable<SplitResult> _results;

        public Splitter(CueSheet.CueSheet cueSheet, CueSheetSplitter cueSheetSplitter)
        {
            if (cueSheet == null)
                throw new ArgumentNullException("cueSheet");

            if (cueSheetSplitter == null)
                throw new ArgumentNullException("cueSheetSplitter");

            _cueSheet = cueSheet;
            _cueSheetSplitter = cueSheetSplitter;
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
            _results = _cueSheetSplitter.Split();
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
            }
        }
    }
}