using System;
using System.Collections.Generic;
using System.Linq;


namespace CuesheetSplitterEncoder.Core.CueSheet
{
    public class CueSheet
    {
        readonly List<Tuple<string, string>> _comments = new List<Tuple<string, string>>();
        readonly List<File> _files = new List<File>();

        public string Catalog { get; set; }

        public string CdTextFile { get; set; }

        public List<Tuple<string, string>> Comments
        {
            get { return _comments; }
        }

        public List<File> Files
        {
            get { return _files; }
        }

        public string Performer { get; set; }

        public string SongWriter { get; set; }

        public string Title { get; set; }

        public bool IsStandard { get; set; }

        public bool IsNoncompliant { get; set; }

        public bool TryGetCommentValue(string name, out string value)
        {
            value = null;

            foreach (var comment in _comments.Where(comment => comment.Item1 == name))
            {
                value = comment.Item2;
                return true;
            }

            return false;
        }
    }
}