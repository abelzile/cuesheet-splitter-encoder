using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace CuesheetSplitterEncoder.Core.Utils
{
    public static class WordUtils
    {
        // This class is inspired by the Grammartron plugin for Mp3tag (see http://forums.mp3tag.de/index.php?showtopic=13185&hl=grammartron).

        static readonly char[] Punctuation = { '.', ',', '!', '?', ':', ';', '"', '\'', '-', '(', ')' };
        static readonly string[] ArticlesConjunctionsPrepositions =
        {
            "A", "An", "And", "As", "At", 
            "But", "By", 
            "De", 
            "Et", 
            "For", 
            "From", 
            "In", "Into", 
            "Le", 
            "Nor", 
            "Of", "Off", "On", "Onto", "Or", 
            "So", 
            "Than", "The", "To", 
            "Upon", 
            "Von", 
            "With"
        };
        static readonly string[] CommonVerbs =
        {
            "Add", "Allow", "Appear", "Ask", 
            "Be", "Become", "Begin", "Believe", "Bring", "Build", "Buy", 
            "Call", "Can", "Change", "Come", "Consider", "Continue", "Could", "Create", "Cut",
            "Die", "Do", 
            "Expect", 
            "Fall", "Feel", "Find", "Follow", 
            "Get", "Give", "Go", "Grow", 
            "Happen", "Have", "Hear", "Help", "Hold", 
            "Include", 
            "Keep", "Kill", "Know", 
            "Lead", "Learn", "Leave", "Let", "Like", "Live", "Look", "Lose", "Love", 
            "Make", "May", "Mean", "Meet", "Might", "Move", "Must", 
            "Need", 
            "Offer", "Open",
            "Pay", "Play", "Provide", "Put", 
            "Reach", "Read", "Remain", "Remember", "Run", 
            "Say", "See", "Seem", "Send", "Serve", "Set", "Should", "Show", "Sit", "Speak", "Spend", "Stand", "Start", "Stay", "Stop", 
            "Take", "Talk", "Tell", "Think", "Try", "Turn", 
            "Understand", "Use", 
            "Wait", "Walk", "Want", "Watch", "Will", "Win", "Work", "Would", "Write",
        };
        static readonly Regex RomanNumerals = new Regex("^[MDCLXVI" + new string(Punctuation) + "]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ToTitleCase(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string[] words = text.Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var sentence = new List<string>();

            for (int i = 0; i < words.Length; i++)
            {
                string prevWord = (i != 0) ? words[i - 1] : "";
                string nextWord = (i + 1 < words.Length) ? words[i + 1] : "";
                string word = words[i].ToLowerInvariant();
                word = UpperFirstLetter(word);
                word = LowerArticlesConjunctionsPrepositions(word, prevWord, nextWord);
                word = UpperToBeforeCommonVerb(word, nextWord);
                word = UpperRomanNumerals(word);

                sentence.Add(word);
            }

            return string.Join(" ", sentence);
        }

        static string UpperFirstLetter(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return "";

            char[] cs = word.ToCharArray();

            for (int i = 0; i < cs.Length; i++)
            {
                if (IsPunctuation(cs[i])) continue;

                cs[i] = char.ToUpper(cs[i]);
                break;
            }

            return new string(cs);
        }

        static string LowerArticlesConjunctionsPrepositions(string word, string prevWord, string nextWord)
        {
            if (!IsArticleConjunctionOrPreposition(word)) 
                return word;

            // Do not lower if first word or after punctuation.
            if (string.IsNullOrWhiteSpace(prevWord) || IsPunctuation(prevWord.Last())) 
                return word;

            // Do not lower if last word.
            if (string.IsNullOrWhiteSpace(nextWord)) 
                return word;

            return word.ToLowerInvariant();
        }

        static string UpperToBeforeCommonVerb(string word, string nextWord)
        {
            if (!string.Equals(word, "to", StringComparison.OrdinalIgnoreCase)) 
                return word;

            if (string.IsNullOrWhiteSpace(nextWord)) 
                return word;

            return (IsCommonVerb(nextWord)) ? UpperFirstLetter(word) : word;
        }

        static string UpperRomanNumerals(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) 
                return "";

            return (RomanNumerals.IsMatch(word)) ? word.ToUpperInvariant() : word;
        }

        static bool IsPunctuation(char chr)
        {
            return Punctuation.Contains(chr);
        }

        static bool IsArticleConjunctionOrPreposition(string word)
        {
            return ArticlesConjunctionsPrepositions.Any(s => string.Equals(word, s, StringComparison.OrdinalIgnoreCase));
        }

        static bool IsCommonVerb(string word)
        {
            return CommonVerbs.Any(s => string.Equals(word, s, StringComparison.OrdinalIgnoreCase));
        }
    }
}