using System;
using System.IO;
using System.Reflection;
using CuesheetSplitterEncoder.Core.CueSheet;
using CuesheetSplitterEncoder.Core.CueSheet.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using File = CuesheetSplitterEncoder.Core.CueSheet.File;


namespace CuesheetSplitterEncoder.Test
{
    [TestClass]
    public class CueSheetParserTests
    {
        //string[] manifestResourceNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
        const string StandardCueSheetFileName = "CuesheetSplitterEncoder.Test.CueSheetFiles.Standard.cue";
        const string NonstandardCueSheetFileName = "CuesheetSplitterEncoder.Test.CueSheetFiles.Nonstandard.cue";
        const string NonstandardNoncompliantCueSheetFileName = "CuesheetSplitterEncoder.Test.CueSheetFiles.NonstandardNoncompliant.cue";

        [TestMethod]
        public void Can_Parse_Standard_CueSheet_File()
        {
            using (Stream stream = GetStandardCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.IsNotNull(cueSheet);
            }
        }

        [TestMethod]
        public void Can_Parse_Standard_CueSheet_Comment_Commands()
        {
            using (Stream stream = GetStandardCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.AreEqual(4, cueSheet.Comments.Count);

                Assert.AreEqual("GENRE", cueSheet.Comments[0].Item1);
                Assert.AreEqual("Death Metal", cueSheet.Comments[0].Item2);

                Assert.AreEqual("DATE", cueSheet.Comments[1].Item1);
                Assert.AreEqual("1994", cueSheet.Comments[1].Item2);

                Assert.AreEqual("DISCID", cueSheet.Comments[2].Item1);
                Assert.AreEqual("1F034504", cueSheet.Comments[2].Item2);

                Assert.AreEqual("COMMENT", cueSheet.Comments[3].Item1);
                Assert.AreEqual("ExactAudioCopy v1.0b3", cueSheet.Comments[3].Item2);
            }
        }

        [TestMethod]
        public void Can_Parse_Standard_CueSheet_File_Level_Commands()
        {
            using (Stream stream = GetStandardCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.AreEqual("AMORPHIS", cueSheet.Performer);
                Assert.AreEqual("Black winter day", cueSheet.Title);

                Assert.AreEqual(1, cueSheet.Files.Count);

                Assert.AreEqual("AMORPHIS - Black winter day.flac", cueSheet.Files[0].FileName);
                Assert.AreEqual("WAVE", cueSheet.Files[0].FileType);
            }
        }

        [TestMethod]
        public void Can_Parse_Standard_CueSheet_Track_Level_Commands()
        {
            using (Stream stream = GetStandardCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                File file = cueSheet.Files[0];

                Assert.AreEqual(4, file.Tracks.Count);

                Track track1 = file.Tracks[0];

                Assert.AreEqual(1, track1.TrackNum);
                Assert.AreEqual("AUDIO", track1.TrackType);
                Assert.AreEqual("Black winter day", track1.Title);
                Assert.AreEqual("AMORPHIS", track1.Performer);

                Assert.AreEqual(1, track1.Indexes.Count);

                Index index1 = track1.Indexes[0];

                Assert.AreEqual(1, index1.IndexNum);
                Assert.AreEqual(0, index1.IndexTime.Minutes);
                Assert.AreEqual(0, index1.IndexTime.Seconds);
                Assert.AreEqual(0, index1.IndexTime.Frames);

                Track track4 = file.Tracks[3];

                Assert.AreEqual(4, track4.TrackNum);
                Assert.AreEqual("AUDIO", track4.TrackType);
                Assert.AreEqual("Moon and sun Part II: North's son", track4.Title);
                Assert.AreEqual("AMORPHIS", track4.Performer);

                Assert.AreEqual(2, track4.Indexes.Count);

                Index index0 = track4.Indexes[0];

                Assert.AreEqual(0, index0.IndexNum);
                Assert.AreEqual(8, index0.IndexTime.Minutes);
                Assert.AreEqual(45, index0.IndexTime.Seconds);
                Assert.AreEqual(55, index0.IndexTime.Frames);

                index1 = track4.Indexes[1];

                Assert.AreEqual(1, index1.IndexNum);
                Assert.AreEqual(8, index1.IndexTime.Minutes);
                Assert.AreEqual(47, index1.IndexTime.Seconds);
                Assert.AreEqual(0, index1.IndexTime.Frames);
            }
        }

        [TestMethod]
        public void Can_Parse_Nonstandard_CueSheet_File()
        {
            using (Stream stream = GetNonstandardCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.IsNotNull(cueSheet);
            }
        }

        [TestMethod]
        public void Can_Parse_Nonstandard_CueSheet_Comment_Commands()
        {
            using (Stream stream = GetNonstandardCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.AreEqual(4, cueSheet.Comments.Count);

                Assert.AreEqual("GENRE", cueSheet.Comments[0].Item1);
                Assert.AreEqual("Metal", cueSheet.Comments[0].Item2);

                Assert.AreEqual("DATE", cueSheet.Comments[1].Item1);
                Assert.AreEqual("2000", cueSheet.Comments[1].Item2);

                Assert.AreEqual("DISCID", cueSheet.Comments[2].Item1);
                Assert.AreEqual("55090619", cueSheet.Comments[2].Item2);

                Assert.AreEqual("COMMENT", cueSheet.Comments[3].Item1);
                Assert.AreEqual("ExactAudioCopy v0.95b4", cueSheet.Comments[3].Item2);
            }
        }

        [TestMethod]
        public void Can_Parse_Nonstandard_CueSheet_File_Level_Commands()
        {
            using (Stream stream = GetNonstandardCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.AreEqual("Nasum", cueSheet.Performer);
                Assert.AreEqual("Human 2.0", cueSheet.Title);

                Assert.AreEqual(25, cueSheet.Files.Count);

                Assert.AreEqual("01 Nasum - Mass Hypnosis.flac", cueSheet.Files[0].FileName);
                Assert.AreEqual("WAVE", cueSheet.Files[0].FileType);

                Assert.AreEqual("25 Nasum - Sometimes Dead is Better.flac", cueSheet.Files[24].FileName);
                Assert.AreEqual("WAVE", cueSheet.Files[24].FileType);
            }
        }

        [TestMethod]
        public void Can_Parse_Nonstandard_CueSheet_Track_Level_Commands()
        {
            using (Stream stream = GetNonstandardCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                File file1 = cueSheet.Files[0];

                Assert.AreEqual(1, file1.Tracks.Count);

                Track track1 = file1.Tracks[0];

                Assert.AreEqual(1, track1.TrackNum);
                Assert.AreEqual("AUDIO", track1.TrackType);
                Assert.AreEqual("Mass Hypnosis", track1.Title);
                Assert.AreEqual("Nasum", track1.Performer);

                Assert.AreEqual(1, track1.Indexes.Count);

                Index index1 = track1.Indexes[0];

                Assert.AreEqual(1, index1.IndexNum);
                Assert.AreEqual(0, index1.IndexTime.Minutes);
                Assert.AreEqual(0, index1.IndexTime.Seconds);
                Assert.AreEqual(0, index1.IndexTime.Frames);

                File file25 = cueSheet.Files[24];

                track1 = file25.Tracks[0];

                Assert.AreEqual(25, track1.TrackNum);
                Assert.AreEqual("AUDIO", track1.TrackType);
                Assert.AreEqual("Sometimes Dead is Better", track1.Title);
                Assert.AreEqual("Nasum", track1.Performer);

                Assert.AreEqual(1, track1.Indexes.Count);

                index1 = track1.Indexes[0];

                Assert.AreEqual(1, index1.IndexNum);
                Assert.AreEqual(0, index1.IndexTime.Minutes);
                Assert.AreEqual(0, index1.IndexTime.Seconds);
                Assert.AreEqual(0, index1.IndexTime.Frames);
            }
        }




        [TestMethod]
        public void Can_Parse_Nonstandard_Noncompliant_CueSheet_File()
        {
            using (Stream stream = GetNonstandardNoncompliantCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.IsNotNull(cueSheet);
            }
        }

        [TestMethod]
        public void Can_Parse_Nonstandard__Noncompliant_CueSheet_Comment_Commands()
        {
            using (Stream stream = GetNonstandardNoncompliantCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.AreEqual(4, cueSheet.Comments.Count);

                Assert.AreEqual("GENRE", cueSheet.Comments[0].Item1);
                Assert.AreEqual("Death Metal", cueSheet.Comments[0].Item2);

                Assert.AreEqual("DATE", cueSheet.Comments[1].Item1);
                Assert.AreEqual("1995", cueSheet.Comments[1].Item2);

                Assert.AreEqual("DISCID", cueSheet.Comments[2].Item1);
                Assert.AreEqual("870B3B0B", cueSheet.Comments[2].Item2);

                Assert.AreEqual("COMMENT", cueSheet.Comments[3].Item1);
                Assert.AreEqual("ExactAudioCopy v0.99pb4", cueSheet.Comments[3].Item2);
            }
        }

        [TestMethod]
        public void Can_Parse_Nonstandard_Noncompliant_CueSheet_File_Level_Commands()
        {
            using (Stream stream = GetNonstandardNoncompliantCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                Assert.AreEqual("Dark Tranquillity", cueSheet.Performer);
                Assert.AreEqual("The Gallery", cueSheet.Title);

                Assert.AreEqual(11, cueSheet.Files.Count);

                Assert.AreEqual("01 Punish My Heaven.flac", cueSheet.Files[0].FileName);
                Assert.AreEqual("WAVE", cueSheet.Files[0].FileType);

                Assert.AreEqual("11 ...Of Melancholy Burning.flac", cueSheet.Files[10].FileName);
                Assert.AreEqual("WAVE", cueSheet.Files[10].FileType);
            }
        }

        [TestMethod]
        public void Can_Parse_Nonstandard_Noncompliant_CueSheet_Track_Level_Commands()
        {
            using (Stream stream = GetNonstandardNoncompliantCueSheet())
            {
                var parser = new CueSheetParser(stream);
                CueSheet cueSheet = parser.Parse();

                File file1 = cueSheet.Files[0];

                Assert.AreEqual(1, file1.Tracks.Count);

                Track track1 = file1.Tracks[0];

                Assert.AreEqual(1, track1.TrackNum);
                Assert.AreEqual("AUDIO", track1.TrackType);
                Assert.AreEqual("Punish My Heaven", track1.Title);
                Assert.AreEqual("Dark Tranquillity", track1.Performer);
                Assert.AreEqual("DCP", track1.Flags);

                Assert.AreEqual(1, track1.Indexes.Count);

                Index index1 = track1.Indexes[0];

                Assert.AreEqual(1, index1.IndexNum);
                Assert.AreEqual(0, index1.IndexTime.Minutes);
                Assert.AreEqual(0, index1.IndexTime.Seconds);
                Assert.AreEqual(0, index1.IndexTime.Frames);

                File file9 = cueSheet.Files[9];

                track1 = file9.Tracks[0];

                Assert.AreEqual(10, track1.TrackNum);
                Assert.AreEqual("AUDIO", track1.TrackType);
                Assert.AreEqual("Mine Is The Grandeur...", track1.Title);
                Assert.AreEqual("Dark Tranquillity", track1.Performer);
                Assert.AreEqual("DCP", track1.Flags);

                Assert.AreEqual(2, track1.Indexes.Count);

                Index index0 = track1.Indexes[0];

                Assert.AreEqual(0, index0.IndexNum);
                Assert.AreEqual(5, index0.IndexTime.Minutes);
                Assert.AreEqual(41, index0.IndexTime.Seconds);
                Assert.AreEqual(65, index0.IndexTime.Frames);

                index1 = track1.Indexes[1];

                Assert.AreEqual(1, index1.IndexNum);
                Assert.AreEqual(0, index1.IndexTime.Minutes);
                Assert.AreEqual(0, index1.IndexTime.Seconds);
                Assert.AreEqual(0, index1.IndexTime.Frames);
            }
        }

        static Stream GetStandardCueSheet()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(StandardCueSheetFileName);
        }

        static Stream GetNonstandardCueSheet()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(NonstandardCueSheetFileName);
        }

        static Stream GetNonstandardNoncompliantCueSheet()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(NonstandardNoncompliantCueSheetFileName);
        }
    }
}
