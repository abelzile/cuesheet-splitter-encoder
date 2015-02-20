using System;


namespace CuesheetSplitterEncoder.Core.CueSheet
{
    public class IndexTime
    {
        const decimal FrameInMs = 13.333333333333333333333333333M;

        readonly byte _minutes;
        readonly byte _seconds;
        readonly byte _frames;

        public IndexTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                throw new ArgumentNullException("time");

            string[] parts = time.Split(':');

            if (parts.Length != 3)
                throw new Exception("Time format must be MM:SS:FF");

            if (!byte.TryParse(parts[0], out _minutes))
            {
                throw new Exception("Minutes value must be a number between 0 and 99");
            }

            if (!(_minutes <= 99))
                throw new Exception("Minutes value must be a number between 0 and 99");

            if (!byte.TryParse(parts[1], out _seconds))
            {
                throw new Exception("Seconds value must be a number between 0 and 99");
            }

            if (!(_seconds <= 99))
                throw new Exception("Seconds value must be a number between 0 and 99");

            if (!byte.TryParse(parts[2], out _frames))
            {
                throw new Exception("Frames value must be a number between 0 and 75");
            }

            if (!(_frames <= 75))
                throw new Exception("Frames value must be a number between 0 and 75");
        }

        public byte Minutes
        {
            get { return _minutes; }
        }

        public byte Seconds
        {
            get { return _seconds; }
        }

        public byte Frames
        {
            get { return _frames; }
        }

        public uint Samples
        {
            get { return (uint)(((((60 * Minutes) + Seconds) * 75) + Frames) * 588); }
        }

        public uint Milliseconds
        {
            get { return (uint)Math.Truncate(Frames * FrameInMs); }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", Minutes.ToString("D2"), Seconds.ToString("D2"), Frames.ToString("D2"));
        }
    }
}