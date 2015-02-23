using CuesheetSplitterEncoder.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CuesheetSplitterEncoder.Test
{
    [TestClass]
    public class ToTitleCaseTests
    {
        [TestMethod]
        public void Can_TitleCase_Simple_Single_Word_Title()
        {
            string test = "quick";
            string result = WordUtils.ToTitleCase(test);

            Assert.AreEqual("Quick", result);
        }

        [TestMethod]
        public void Can_TitleCase_Simple_Double_Word_Title()
        {
            string test = "QUICK brown";
            string result = WordUtils.ToTitleCase(test);

            Assert.AreEqual("Quick Brown", result);
        }

        [TestMethod]
        public void Can_TitleCase_Simple_Multi_Word_Title()
        {
            string test = "QUICK brown FOX JuMps DoG";
            string result = WordUtils.ToTitleCase(test);

            Assert.AreEqual("Quick Brown Fox Jumps Dog", result);
        }

        [TestMethod]
        public void Can_TitleCase_Title_Beginning_With_Article_Conjunction_Preposition()
        {
            string test = "the quick brown fox";
            string result = WordUtils.ToTitleCase(test);

            Assert.AreEqual("The Quick Brown Fox", result);
        }

        [TestMethod]
        public void Can_TitleCase_Title_With_Article_Conjunction_Preposition_After_Punctuation()
        {
            string test = "quick brown fox: a jumper";
            string result = WordUtils.ToTitleCase(test);

            Assert.AreEqual("Quick Brown Fox: A Jumper", result);
        }

        [TestMethod]
        public void Can_TitleCase_Title_Ending_With_Article_Conjunction_Preposition()
        {
            string test = "quick brown fox where from";
            string result = WordUtils.ToTitleCase(test);

            Assert.AreEqual("Quick Brown Fox Where From", result);
        }

        [TestMethod]
        public void Can_TitelCase_Title_With_Article_Conjunction_Preposition()
        {
            string test = "QUICK BROWN foX IS A dog JUMPER";
            string result = WordUtils.ToTitleCase(test);

            Assert.AreEqual("Quick Brown Fox Is a Dog Jumper", result);
        }

        //TODO
    }
}