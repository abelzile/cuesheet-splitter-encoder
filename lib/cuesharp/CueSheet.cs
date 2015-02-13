/*
Title:    CueSharp
Version:  0.5
Released: March 24, 2007

Author:   Wyatt O'Day
Website:  wyday.com/cuesharp
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;

namespace CueSharp
{
    /// <summary>
    /// A CueSheet class used to create, open, edit, and save cuesheets.
    /// </summary>
    public class CueSheet
    {
        string _cueFileName = "";
        string _catalog = "";
        string _cdTextFile = "";
        string _performer = "";
        string _songwriter = "";
        string _title = "";
        string _file = "";
        List<string> _cueLines;
        List<string> _comments = new List<string>();
        List<Track> _tracks = new List<Track>();
        readonly List<string> _garbage = new List<string>();

        /// <summary>
        /// Parse a cue sheet string.
        /// </summary>
        /// <param name="cueString">A string containing the cue sheet data.</param>
        /// <param name="lineDelims">Line delimeters; set to "(char[])null" for default delimeters.</param>
        public CueSheet(string cueString, char[] lineDelims)
        {
            if (lineDelims == null)
            {
                lineDelims = new[] { '\n' };
            }

            _cueLines = new List<string>(cueString.Split(lineDelims));
            RemoveEmptyLines(_cueLines);
            ParseCue(_cueLines);
        }

        /// <summary>
        /// Parses a cue sheet file.
        /// </summary>
        /// <param name="cuefilename">The filename for the cue sheet to open.</param>
        public CueSheet(string cuefilename)
        {
            ReadCueSheet(cuefilename, Encoding.Default);
        }

        /// <summary>
        /// Parses a cue sheet file.
        /// </summary>
        /// <param name="cuefilename">The filename for the cue sheet to open.</param>
        /// <param name="encoding">The encoding used to open the file.</param>
        public CueSheet(string cuefilename, Encoding encoding)
        {
            ReadCueSheet(cuefilename, encoding);
        }

        /// <summary>
        /// Create a cue sheet from scratch.
        /// </summary>
        public CueSheet()
        {
        }

        /// <summary>
        /// Returns/Sets track in this cuefile.
        /// </summary>
        /// <param name="tracknumber">The track in this cuefile.</param>
        /// <returns>Track at the tracknumber.</returns>
        public Track this[int tracknumber]
        {
            get { return _tracks[tracknumber]; }
            set { _tracks[tracknumber] = value; }
        }
        
        /// <summary>
        /// The catalog number must be 13 digits long and is encoded according to UPC/EAN rules.
        /// Example: CATALOG 1234567890123
        /// </summary>
        public string Catalog
        {
            get { return _catalog; }
            set { _catalog = value; }
        }

        /// <summary>
        /// This command is used to specify the name of the file that contains the encoded CD-TEXT information for the disc. This command is only used with files that were either created with the graphical CD-TEXT editor or generated automatically by the software when copying a CD-TEXT enhanced disc.
        /// </summary>
        public string CDTextFile
        {
            get { return _cdTextFile; }
            set { _cdTextFile = value; }
        }

        /// <summary>
        /// This command is used to put comments in your CUE SHEET file.
        /// </summary>
        public List<string> Comments
        {
            get { return _comments; }
            set { _comments = value; }
        }

        /// <summary>
        /// Lines in the cue file that don't belong or have other general syntax errors.
        /// </summary>
        public List<string> Garbage
        {
            get { return _garbage; }
        }

        /// <summary>
        /// This command is used to specify the name of a perfomer for a CD-TEXT enhanced disc.
        /// </summary>
        public string Performer
        {
            get { return _performer; }
            set { _performer = value; }
        }

        /// <summary>
        /// This command is used to specify the name of a songwriter for a CD-TEXT enhanced disc.
        /// </summary>
        public string Songwriter
        {
            get { return _songwriter; }
            set { _songwriter = value; }
        }

        /// <summary>
        /// The title of the entire disc as a whole.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>
        /// The array of tracks on the cuesheet.
        /// </summary>
        public List<Track> Tracks
        {
            get { return _tracks; }
            set { _tracks = value; }
        }

        public string File
        {
            get { return _file; }
            set { _file = value; }
        }

        public string CueFileName
        {
            get { return _cueFileName; }
        }

        public IEnumerable<string> CueLines
        {
            get { return _cueLines.AsReadOnly(); }
        }

        private void ReadCueSheet(string filename, Encoding encoding)
        {
            _cueLines = new List<string>(System.IO.File.ReadAllLines(filename, encoding));

            RemoveEmptyLines(_cueLines);

            ParseCue(_cueLines);

            _cueFileName = filename;
        }

        /// <summary>
        /// Removes any empty lines, elimating possible trouble.
        /// </summary>
        /// <param name="file"></param>
        private void RemoveEmptyLines(List<string> file)
        {
            for (int i = file.Count; i-- > 0; )
            {
                if (string.IsNullOrWhiteSpace(file[i]))
                {
                    file.RemoveAt(i);
                }
            }
        }

        private void ParseCue(List<string> file)
        {
            //-1 means still global, 
            //all others are track specific
            int trackOn = -1;
            var currentFile = new AudioFile();

            for (int i = 0; i < file.Count; i++)
            {
                file[i] = file[i].Trim();

                switch (file[i].Substring(0, file[i].IndexOf(' ')).ToUpper())
                {
                    case "CATALOG":
                    case "CDTEXTFILE":
                    case "ISRC":
                    case "PERFORMER":
                    case "SONGWRITER":
                    case "TITLE":
                        ParseString(file[i], trackOn);
                        break;
                    case "FILE":
                        ParseString(file[i], trackOn);
                        currentFile = ParseFile(file[i], trackOn);
                        break;
                    case "FLAGS":
                        ParseFlags(file[i], trackOn);
                        break;
                    case "INDEX":
                    case "POSTGAP":
                    case "PREGAP":
                        ParseIndex(file[i], trackOn);
                        break;
                    case "REM":
                        ParseComment(file[i], trackOn);
                        break;
                    case "TRACK":
                        trackOn++;
                        ParseTrack(file[i], trackOn);
                        if (currentFile.FileName != "") //if there's a file
                        {
                            _tracks[trackOn].DataFile = currentFile;
                            currentFile = new AudioFile();
                        }
                        break;
                    default:
                        ParseGarbage(file[i], trackOn);
                        //save discarded junk and place string[] with track it was found in
                        break;
                }
            }

        }

        private void ParseComment(string line, int trackOn)
        {
            //remove "REM" (we know the line has already been .Trim()'ed)
            line = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();

            if (trackOn == -1)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _comments.Add(line);
                }
            }
            else
            {
                _tracks[trackOn].AddComment(line);
            }
        }

        private AudioFile ParseFile(string line, int trackOn)
        {
            line = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();

            string fileType = line.Substring(line.LastIndexOf(' '), line.Length - line.LastIndexOf(' ')).Trim();

            line = line.Substring(0, line.LastIndexOf(' ')).Trim();

            //if quotes around it, remove them.
            if (line[0] == '"')
            {
                line = line.Substring(1, line.LastIndexOf('"') - 1);
            }

            return new AudioFile(line, fileType);
        }

        private void ParseFlags(string line, int trackOn)
        {
            if (trackOn == -1) return;

            line = line.Trim();

            if (line == "") return;

            string temp;

            try
            {
                temp = line.Substring(0, line.IndexOf(' ')).ToUpper();
            }
            catch (Exception)
            {
                temp = line.ToUpper();
            }

            switch (temp)
            {
                case "FLAGS":
                    _tracks[trackOn].AddFlag(temp);
                    break;
                case "DATA":
                    _tracks[trackOn].AddFlag(temp);
                    break;
                case "DCP":
                    _tracks[trackOn].AddFlag(temp);
                    break;
                case "4CH":
                    _tracks[trackOn].AddFlag(temp);
                    break;
                case "PRE":
                    _tracks[trackOn].AddFlag(temp);
                    break;
                case "SCMS":
                    _tracks[trackOn].AddFlag(temp);
                    break;
            }

            //processing for a case when there isn't any more spaces
            //i.e. avoiding the "index cannot be less than zero" error
            //when calling line.IndexOf(' ')
            try
            {
                temp = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' '));
            }
            catch (Exception)
            {
                temp = line.Substring(0, line.Length);
            }

            //if the flag hasn't already been processed
            if (temp.ToUpper().Trim() != line.ToUpper().Trim())
            {
                ParseFlags(temp, trackOn);
            }
        }

        private void ParseGarbage(string line, int trackOn)
        {
            if (trackOn == -1)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _garbage.Add(line);
                }
            }
            else
            {
                _tracks[trackOn].AddGarbage(line);
            }
        }

        private void ParseIndex(string line, int trackOn)
        {
            string indexType = line.Substring(0, line.IndexOf(' ')).ToUpper();
            string tempString = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();
            int number = 0;

            if (indexType == "INDEX")
            {
                //read the index number
                number = Convert.ToInt32(tempString.Substring(0, tempString.IndexOf(' ')));
                tempString = tempString.Substring(tempString.IndexOf(' '), tempString.Length - tempString.IndexOf(' ')).Trim();
            }

            //extract the minutes, seconds, and frames
            int minutes = Convert.ToInt32(tempString.Substring(0, tempString.IndexOf(':')));
            int seconds = Convert.ToInt32(tempString.Substring(tempString.IndexOf(':') + 1, tempString.LastIndexOf(':') - tempString.IndexOf(':') - 1));
            int frames = Convert.ToInt32(tempString.Substring(tempString.LastIndexOf(':') + 1, tempString.Length - tempString.LastIndexOf(':') - 1));

            switch (indexType)
            {
                case "INDEX":
                    _tracks[trackOn].AddIndex(number, minutes, seconds, frames);
                    break;
                case "PREGAP":
                    _tracks[trackOn].PreGap = new Index(0, minutes, seconds, frames);
                    break;
                case "POSTGAP":
                    _tracks[trackOn].PostGap = new Index(0, minutes, seconds, frames);
                    break;
            }
        }

        private void ParseString(string line, int trackOn)
        {
            string category = line.Substring(0, line.IndexOf(' ')).ToUpper();

            line = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();

            //get rid of the quotes
            if (line[0] == '"')
            {
                line = line.Substring(1, line.LastIndexOf('"') - 1);
            }

            switch (category)
            {
                case "CATALOG":
                    if (trackOn == -1)
                    {
                        _catalog = line;
                    }
                    break;
                case "CDTEXTFILE":
                    if (trackOn == -1)
                    {
                        _cdTextFile = line;
                    }
                    break;
                case "ISRC":
                    if (trackOn != -1)
                    {
                        _tracks[trackOn].ISRC = line;
                    }
                    break;
                case "PERFORMER":
                    if (trackOn == -1)
                    {
                        _performer = line;
                    }
                    else
                    {
                        _tracks[trackOn].Performer = line;
                    }
                    break;
               case "SONGWRITER":
                   if (trackOn == -1)
                   {
                       _songwriter = line;
                   }
                   else
                   {
                       _tracks[trackOn].Songwriter = line;
                   }
                    break;
                case "TITLE":
                    if (trackOn == -1)
                    {
                        _title = line;
                    }
                    else
                    {
                        _tracks[trackOn].Title = line;
                    }
                    break;
                case "FILE":
                    if (trackOn == -1)
                    {
                        _file = line;
                    }
                    else
                    {
                        _tracks[trackOn].Title = line;
                    }
                    break;
            }
        }

        /// <summary>
        /// Parses the TRACK command. 
        /// </summary>
        /// <param name="line">The line in the cue file that contains the TRACK command.</param>
        /// <param name="trackOn">The track currently processing.</param>
        private void ParseTrack(string line, int trackOn)
        {
            string tempString = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();
            int trackNumber = Convert.ToInt32(tempString.Substring(0, tempString.IndexOf(' ')));

            //find the data type.
            tempString = tempString.Substring(tempString.IndexOf(' '), tempString.Length - tempString.IndexOf(' ')).Trim();

            AddTrack(trackNumber, tempString);
        }
        
        /// <summary>
        /// Add a track to the current cuesheet.
        /// </summary>
        /// <param name="tracknumber">The number of the said track.</param>
        /// <param name="datatype">The datatype of the track.</param>
        private void AddTrack(int tracknumber, string datatype)
        {
            _tracks.Add(new Track(tracknumber, datatype));
        }

        /// <summary>
        /// Add a track to the current cuesheet
        /// </summary>
        /// <param name="title">The title of the track.</param>
        /// <param name="performer">The performer of this track.</param>
        public void AddTrack(string title, string performer)
        {
            _tracks.Add(new Track(_tracks.Count, "") { Performer = performer, Title = title });
        }
        
        public void AddTrack(string title, string performer, string filename, FileType fType)
        {
            _tracks.Add(new Track(_tracks.Count, "")
            {
                Performer = performer,
                Title = title,
                DataFile = new AudioFile(filename, fType)
            });
        }        

        /// <summary>
        /// Add a track to the current cuesheet
        /// </summary>
        /// <param name="title">The title of the track.</param>
        /// <param name="performer">The performer of this track.</param>
        /// <param name="datatype">The datatype for the track (typically DataType.Audio)</param>
        public void AddTrack(string title, string performer, DataType datatype)
        {
            _tracks.Add(new Track(_tracks.Count, datatype) { Performer = performer, Title = title });
        }

        /// <summary>
        /// Add a track to the current cuesheet
        /// </summary>
        /// <param name="track">Track object to add to the cuesheet.</param>
        public void AddTrack(Track track)
        {
            _tracks.Add(track);
        }

        /// <summary>
        /// Remove a track from the cuesheet.
        /// </summary>
        /// <param name="trackIndex">The index of the track you wish to remove.</param>
        public void RemoveTrack(int trackIndex)
        {
            _tracks.RemoveAt(trackIndex);
        }

        /// <summary>
        /// Add index information to an existing track.
        /// </summary>
        /// <param name="trackIndex">The array index number of track to be modified</param>
        /// <param name="indexNum">The index number of the new index</param>
        /// <param name="minutes">The minute value of the new index</param>
        /// <param name="seconds">The seconds value of the new index</param>
        /// <param name="frames">The frames value of the new index</param>
        public void AddIndex(int trackIndex, int indexNum, int minutes, int seconds, int frames)
        {
            _tracks[trackIndex].AddIndex(indexNum, minutes, seconds, frames);
        }

        /// <summary>
        /// Remove an index from a track.
        /// </summary>
        /// <param name="trackIndex">The array-index of the track.</param>
        /// <param name="indexIndex">The index of the Index you wish to remove.</param>
        public void RemoveIndex(int trackIndex, int indexIndex)
        {
            //Note it is the index of the Index you want to delete, 
            //which may or may not correspond to the number of the index.
            _tracks[trackIndex].RemoveIndex(indexIndex);
        }

        /// <summary>
        /// Save the cue sheet file to specified location.
        /// </summary>
        /// <param name="filename">Filename of destination cue sheet file.</param>
        public void SaveCue(string filename)
        {
            SaveCue(filename, Encoding.Default);
        }

        /// <summary>
        /// Save the cue sheet file to specified location.
        /// </summary>
        /// <param name="filename">Filename of destination cue sheet file.</param>
        /// <param name="encoding">The encoding used to save the file.</param>
        public void SaveCue(string filename, Encoding encoding)
        {
            TextWriter tw = new StreamWriter(filename, false, encoding);

            tw.WriteLine(this.ToString());

            //close the writer stream
            tw.Close();
        }

        /// <summary>
        /// Method to output the cuesheet into a single formatted string.
        /// </summary>
        /// <returns>The entire cuesheet formatted to specification.</returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            foreach (string comment in _comments)
            {
                output.Append("REM " + comment + Environment.NewLine);
            }

            if (_catalog.Trim() != "")
            {
                output.Append("CATALOG " + _catalog + Environment.NewLine);
            }

            if (_performer.Trim() != "")
            {
                output.Append("PERFORMER \"" + _performer + "\"" + Environment.NewLine);
            }

            if (_songwriter.Trim() != "")
            {
                output.Append("SONGWRITER \"" + _songwriter + "\"" + Environment.NewLine);
            }

            if (_title.Trim() != "")
            {
                output.Append("TITLE \"" + _title + "\"" + Environment.NewLine);
            }

            if (_cdTextFile.Trim() != "")
            {
                output.Append("CDTEXTFILE \"" + _cdTextFile.Trim() + "\"" + Environment.NewLine);
            }

            for (int i = 0; i < _tracks.Count; i++)
            {
                output.Append(_tracks[i].ToString());

                if (i != _tracks.Count - 1)
                {
                    //add line break for each track except last
                    output.Append(Environment.NewLine);
                }
            }

            return output.ToString();
        }

        //TODO: Fix calculation bugs; currently generates erroneous IDs.
        //For complete CDDB/freedb discID calculation, see:
        //http://www.freedb.org/modules.php?name=Sections&sop=viewarticle&artid=6

        public string CalculateCDDBdiscID()
        {
            int i, t = 0, n = 0;

            /* For backward compatibility this algorithm must not change */

            i = 0;

            while (i < _tracks.Count)
            {
                n = n + cddb_sum((lastTrackIndex(_tracks[i]).Minutes * 60) + lastTrackIndex(_tracks[i]).Seconds);
                i++;
            }

            Console.WriteLine(n.ToString());

            t = ((lastTrackIndex(_tracks[_tracks.Count - 1]).Minutes * 60) + lastTrackIndex(_tracks[_tracks.Count - 1]).Seconds) -
                ((lastTrackIndex(_tracks[0]).Minutes * 60) + lastTrackIndex(_tracks[0]).Seconds);

            ulong lDiscId = (((uint)n % 0xff) << 24 | (uint)t << 8 | (uint)_tracks.Count);
            return String.Format("{0:x8}", lDiscId);
        }

        private Index lastTrackIndex(Track track)
        {
            return track.Indices[track.Indices.Count - 1];
        }

        private int cddb_sum(int n)
        {
            int ret;

            /* For backward compatibility this algorithm must not change */

            ret = 0;

            while (n > 0)
            {
                ret = ret + (n % 10);
                n = n / 10;
            }

            return (ret);
        }
    }

    /// <summary>
    /// This command is used to specify a data/audio file that will be written to the recorder.
    /// </summary>
    public class AudioFile
    {
        public AudioFile(string fileName, string fileType)
        {
            FileName = fileName;

            switch (fileType.Trim().ToUpper())
            {
                case "BINARY":
                    FileType = FileType.BINARY;
                    break;
                case "MOTOROLA":
                    FileType = FileType.MOTOROLA;
                    break;
                case "AIFF":
                    FileType = FileType.AIFF;
                    break;
                case "WAVE":
                    FileType = FileType.WAVE;
                    break;
                case "MP3":
                    FileType = FileType.MP3;
                    break;
                default:
                    FileType = FileType.BINARY;
                    break;
            }
        }

        public AudioFile(string fileName, FileType fileType)
        {
            FileName = fileName;
            FileType = fileType;
        }

        public AudioFile()
        {
        }

        public string FileName { get; private set; }

        /// <summary>
        /// BINARY - Intel binary file (least significant byte first)
        /// MOTOROLA - Motorola binary file (most significant byte first)
        /// AIFF - Audio AIFF file
        /// WAVE - Audio WAVE file
        /// MP3 - Audio MP3 file
        /// </summary>
        public FileType FileType { get; private set; }
    }

    /// <summary>
    /// This command is used to specify indexes (or subindexes) within a track.
    /// Syntax:
    ///  INDEX [number] [mm:ss:ff]
    /// </summary>
    public class Index
    {
        //0-99
        int _number;
        int _minutes;
        int _seconds;
        int _frames;

        /// <summary>
        /// The Index of a track.
        /// </summary>
        /// <param name="number">Index number 0-99</param>
        /// <param name="minutes">Minutes (0-99)</param>
        /// <param name="seconds">Seconds (0-59)</param>
        /// <param name="frames">Frames (0-74)</param>
        public Index(int number, int minutes, int seconds, int frames)
        {
            _number = number;
            _minutes = minutes;
            _seconds = seconds;
            _frames = frames;
        }

        /// <summary>
        /// Index number (0-99)
        /// </summary>
        public int Number
        {
            get { return _number; }
            set
            {
                if (value > 99)
                {
                    _number = 99;
                }
                else if (value < 0)
                {
                    _number = 0;
                }
                else
                {
                    _number = value;
                }
            }
        }

        /// <summary>
        /// Possible values: 0-99
        /// </summary>
        public int Minutes
        {
            get { return _minutes; }
            set
            {
                if (value > 99)
                {
                    _minutes = 99;
                }
                else if (value < 0)
                {
                    _minutes = 0;
                }
                else
                {
                    _minutes = value;
                }
            }
        }

        /// <summary>
        /// Possible values: 0-59
        /// There are 60 seconds/minute
        /// </summary>
        public int Seconds
        {
            get { return _seconds; }
            set
            {
                if (value >= 60)
                {
                    _seconds = 59;
                }
                else if (value < 0)
                {
                    _seconds = 0;
                }
                else
                {
                    _seconds = value;
                }
            }
        }

        /// <summary>
        /// Possible values: 0-74
        /// There are 75 frames/second
        /// </summary>
        public int Frames
        {
            get { return _frames; }
            set
            {
                if (value >= 75)
                {
                    _frames = 74;
                }
                else if (value < 0)
                {
                    _frames = 0;
                }
                else
                {
                    _frames = value;
                }
            }
        }
    }

    /// <summary>
    /// Track that contains either data or audio. It can contain Indices and comment information.
    /// </summary>
    public class Track
    {
        private List<string> _comments;
        // strings that don't belong or were mistyped in the global part of the cue
        private AudioFile _dataFile;
        private List<string> _garbage;
        private List<Index> _indices;
        private string _ISRC;
        private string _performer;
        private Index _postGap;
        private Index _preGap;
        private string _songwriter;
        private string _title;
        private List<Flags> _trackFlags;
        private DataType _trackDataType;
        private int _trackNumber;
        string _file;

        /// <summary>
        /// Returns/Sets Index in this track.
        /// </summary>
        /// <param name="indexnumber">Index in the track.</param>
        /// <returns>Index at indexnumber.</returns>
        public Index this[int indexnumber]
        {
            get { return _indices[indexnumber]; }
            set { _indices[indexnumber] = value; }
        }

        public List<string> Comments
        {
            get { return _comments; }
            set { _comments = value; }
        }

        public AudioFile DataFile
        {
            get { return _dataFile; }
            set { _dataFile = value; }
        }

        /// <summary>
        /// Lines in the cue file that don't belong or have other general syntax errors.
        /// </summary>
        public List<string> Garbage
        {
            get { return _garbage; }
            set { _garbage = value; }
        }

        public List<Index> Indices
        {
            get { return _indices; }
            set { _indices = value; }
        }

        public string ISRC
        {
            get { return _ISRC; }
            set { _ISRC = value; }
        }

        public string Performer
        {
            get { return _performer; }
            set { _performer = value; }
        }

        public Index PostGap
        {
            get { return _postGap; }
            set { _postGap = value; }
        }

        public Index PreGap
        {
            get { return _preGap; }
            set { _preGap = value; }
        }

        public string Songwriter
        {
            get { return _songwriter; }
            set { _songwriter = value; }
        }

        /// <summary>
        /// If the TITLE command appears before any TRACK commands, then the string will be encoded as the title of the entire disc.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public DataType TrackDataType
        {
            get { return _trackDataType; }
            set { _trackDataType = value; }
        }

        public List<Flags> TrackFlags
        {
            get { return _trackFlags; }
            set { _trackFlags = value; }
        }

        public int TrackNumber
        {
            get { return _trackNumber; }
            set { _trackNumber = value; }
        }

        public string File
        {
            get { return _file; }
            set { _file = value; }
        }

        public Track(int tracknumber, string datatype)
        {
            _trackNumber = tracknumber;

            switch (datatype.Trim().ToUpper())
            {
                case "AUDIO":
                    _trackDataType = DataType.AUDIO;
                    break;
                case "CDG":
                    _trackDataType = DataType.CDG;
                    break;
                case "MODE1/2048":
                    _trackDataType = DataType.MODE1_2048;
                    break;
                case "MODE1/2352":
                    _trackDataType = DataType.MODE1_2352;
                    break;
                case "MODE2/2336":
                    _trackDataType = DataType.MODE2_2336;
                    break;
                case "MODE2/2352":
                    _trackDataType = DataType.MODE2_2352;
                    break;
                case "CDI/2336":
                    _trackDataType = DataType.CDI_2336;
                    break;
                case "CDI/2352":
                    _trackDataType = DataType.CDI_2352;
                    break;
                default:
                    _trackDataType = DataType.AUDIO;
                    break;
            }

            _trackFlags = new List<Flags>();
            _songwriter = "";
            _title = "";
            _ISRC = "";
            _performer = "";
            _indices = new List<Index>();
            _garbage = new List<string>();
            _comments = new List<string>();
            _preGap = new Index(-1, 0, 0, 0);
            _postGap = new Index(-1, 0, 0, 0);
            _dataFile = new AudioFile();
        }

        public Track(int tracknumber, DataType datatype)
        {
            _trackNumber = tracknumber;
            _trackDataType = datatype;
            _trackFlags = new List<Flags>();
            _songwriter = "";
            _title = "";
            _ISRC = "";
            _performer = "";
            _indices = new List<Index>();
            _garbage = new List<string>();
            _comments = new List<string>();
            _preGap = new Index(-1, 0, 0, 0);
            _postGap = new Index(-1, 0, 0, 0);
            _dataFile = new AudioFile();
        }

        public void AddFlag(Flags flag)
        {
            //if it's not a none tag
            //and if the tags hasn't already been added
            if (flag != Flags.NONE && NewFlag(flag))
            {
                _trackFlags.Add(flag);
            }
        }

        public void AddFlag(string flag)
        {
            switch (flag.Trim().ToUpper())
            {
                case "DATA":
                    AddFlag(Flags.DATA);
                    break;
                case "DCP":
                    AddFlag(Flags.DCP);
                    break;
                case "4CH":
                    AddFlag(Flags.CH4);
                    break;
                case "PRE":
                    AddFlag(Flags.PRE);
                    break;
                case "SCMS":
                    AddFlag(Flags.SCMS);
                    break;
                default:
                    return;
            }
        }

        public void AddGarbage(string garbage)
        {
            if (!string.IsNullOrWhiteSpace(garbage))
            {
                _garbage.Add(garbage);
            }
        }

        public void AddComment(string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                _comments.Add(comment);
            }
        }

        public void AddIndex(int number, int minutes, int seconds, int frames)
        {
            _indices.Add(new Index(number, minutes, seconds, frames));
        }

        public void RemoveIndex(int indexIndex)
        {
            _indices.RemoveAt(indexIndex);
        }

        /// <summary>
        /// Checks if the flag is indeed new in this track.
        /// </summary>
        /// <param name="newFlag">The new flag to be added to the track.</param>
        /// <returns>True if this flag doesn't already exist.</returns>
        private bool NewFlag(Flags newFlag)
        {
            return _trackFlags.All(flag => flag != newFlag);
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            //write file
            if (_dataFile.FileName != null && _dataFile.FileName.Trim() != "")
            {
                output.Append("FILE \"" + _dataFile.FileName.Trim() + "\" " + _dataFile.FileType.ToString() + Environment.NewLine);
            }

            output.Append("  TRACK " + _trackNumber.ToString().PadLeft(2, '0') + " " + _trackDataType.ToString().Replace('_', '/'));

            //write comments
            foreach (string comment in _comments)
            {
                output.Append(Environment.NewLine + "    REM " + comment);
            }

            if (_performer.Trim() != "")
            {
                output.Append(Environment.NewLine + "    PERFORMER \"" + _performer + "\"");
            }

            if (_songwriter.Trim() != "")
            {
                output.Append(Environment.NewLine + "    SONGWRITER \"" + _songwriter + "\"");
            }

            if (_title.Trim() != "")
            {
                output.Append(Environment.NewLine + "    TITLE \"" + _title + "\"");
            }

            //write flags
            if (_trackFlags.Count > 0)
            {
                output.Append(Environment.NewLine + "    FLAGS");
            }

            foreach (Flags flag in _trackFlags)
            {
                output.Append(" " + flag.ToString().Replace("CH4", "4CH"));
            }

            //write isrc
            if (_ISRC.Trim() != "")
            {
                output.Append(Environment.NewLine + "    ISRC " + _ISRC.Trim());
            }

            //write pregap
            if (_preGap.Number != -1)
            {
                output.Append(Environment.NewLine + "    PREGAP " + _preGap.Minutes.ToString().PadLeft(2, '0') + ":" + _preGap.Seconds.ToString().PadLeft(2, '0') + ":" + _preGap.Frames.ToString().PadLeft(2, '0'));
            }

            //write Indices
            for (int j = 0; j < _indices.Count; j++)
            {
                output.Append(Environment.NewLine + "    INDEX " + this[j].Number.ToString().PadLeft(2, '0') + " " + this[j].Minutes.ToString().PadLeft(2, '0') + ":" + this[j].Seconds.ToString().PadLeft(2, '0') + ":" + this[j].Frames.ToString().PadLeft(2, '0'));
            }

            //write postgap
            if (_postGap.Number != -1)
            {
                output.Append(Environment.NewLine + "    POSTGAP " + _postGap.Minutes.ToString().PadLeft(2, '0') + ":" + _postGap.Seconds.ToString().PadLeft(2, '0') + ":" + _postGap.Frames.ToString().PadLeft(2, '0'));
            }

            return output.ToString();
        }
    }

    /// <summary>
    ///DCP - Digital copy permitted
    ///4CH - Four channel audio
    ///PRE - Pre-emphasis enabled (audio tracks only)
    ///SCMS - Serial copy management system (not supported by all recorders)
    ///There is a fourth subcode flag called "DATA" which is set for all non-audio tracks. This flag is set automatically based on the datatype of the track.
    /// </summary>
    public enum Flags
    {
        DCP, CH4, PRE, SCMS, DATA, NONE
    }

    /// <summary>
    /// BINARY - Intel binary file (least significant byte first)
    /// MOTOROLA - Motorola binary file (most significant byte first)
    /// AIFF - Audio AIFF file
    /// WAVE - Audio WAVE file
    /// MP3 - Audio MP3 file
    /// </summary>
    public enum FileType
    {
        BINARY, MOTOROLA, AIFF, WAVE, MP3
    }

    /// <summary>
    /// <list>
    /// <item>AUDIO - Audio/Music (2352)</item>
    /// <item>CDG - Karaoke CD+G (2448)</item>
    /// <item>MODE1/2048 - CDROM Mode1 Data (cooked)</item>
    /// <item>MODE1/2352 - CDROM Mode1 Data (raw)</item>
    /// <item>MODE2/2336 - CDROM-XA Mode2 Data</item>
    /// <item>MODE2/2352 - CDROM-XA Mode2 Data</item>
    /// <item>CDI/2336 - CDI Mode2 Data</item>
    /// <item>CDI/2352 - CDI Mode2 Data</item>
    /// </list>
    /// </summary>
    public enum DataType
    {
        AUDIO , CDG , MODE1_2048 , MODE1_2352 , MODE2_2336 , MODE2_2352 , CDI_2336 , CDI_2352 
    }
}
