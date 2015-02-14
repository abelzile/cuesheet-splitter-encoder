using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;


namespace CuesheetSplitterEncoder.Core.Utils
{
    public static class WordUtils
    {
        // This class is inspired by the Grammartron plugin for Mp3tag (see http://forums.mp3tag.de/index.php?showtopic=13185&hl=grammartron).

        static readonly char[] Punctuation =
        {
            '!', '"', '#', '%', '&', '\'', '(', ')', '*', ',', '-', '.', '/', ':',
            ';', '?', '@', '[', '\\', ']', '_', '{', '}',
        };

        static readonly string[] ArticlesConjunctionsPrepositions =
        {
            "a", "an", "and", "as", "at", "but", "by", "de",
            "et", "for", "from", "in", "into", "le", "nor", "of", "off", "on", "onto", "or", "so", "than", "the", "to",
            "upon", "von", "with",
        };

        static readonly string[] CommonVerbs =
        {
            "add", "allow", "appear", "ask", "be", "become", "begin", "believe",
            "bring", "build", "buy", "call", "can", "change", "come", "consider", "continue", "could", "create", "cut",
            "die", "do", "expect", "fall", "feel", "find", "follow", "get", "give", "go", "grow", "happen", "have",
            "hear", "help", "hold", "include", "keep", "kill", "know", "lead", "learn", "leave", "let", "like", "live",
            "look", "lose", "love", "make", "may", "mean", "meet", "might", "move", "must", "need", "offer", "open",
            "pay", "play", "provide", "put", "reach", "read", "remain", "remember", "run", "say", "see", "seem", "send",
            "serve", "set", "should", "show", "sit", "speak", "spend", "stand", "start", "stay", "stop", "take", "talk",
            "tell", "think", "try", "turn", "understand", "use", "wait", "walk", "want", "watch", "will", "win", "work",
            "would", "write",
        };

        static readonly string[] RomanNumeralWordMatch =
        {
            "i'll", "i'm", "i'd", "mild", "dim", "lid", "mid", "mil", "mix",
            "vim", "id", "li", "mi",
        };

        static readonly Regex ModernRomanNumeralsFlexible = new Regex(
            "^(?=[MDCLXVI])M*(C[MD]|D?C*)(X[CL]|L?X*)(I[XV]|V?I*)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ToTitleCase(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) 
                return text;

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
            if (string.IsNullOrWhiteSpace(word)) 
                return word;

            char[] cs = word.ToCharArray();

            for (int i = 0; i < cs.Length; i++)
            {
                if (char.IsPunctuation(cs[i])) continue;

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
            if (string.IsNullOrWhiteSpace(prevWord) || char.IsPunctuation(prevWord.Last())) 
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
                return word;

            if (IsRomanNumeralWordMatch(word)) 
                return word;

            if (!ContainsPunctuation(word)) 
                return (ModernRomanNumeralsFlexible.IsMatch(word)) ? word.ToUpperInvariant() : word;

            if (!DoesStartOrEndWithPunctuation(word)) 
                return (ModernRomanNumeralsFlexible.IsMatch(word)) ? word.ToUpperInvariant() : word;

            // Strip punctuation from the start and end of the word and then check if a Roman Numeral.

            char[] origChars = word.ToCharArray();
            char[] bareChars = new char[origChars.Length];
            string preChars = "";
            string postChars = "";

            int start = 0;
            while (start < origChars.Length && char.IsPunctuation(origChars[start]))
            {
                preChars += origChars[start];
                bareChars[start] = ' ';
                ++start;
            }

            int end = origChars.Length - 1;
            while (end >= 0 && char.IsPunctuation(origChars[end]))
            {
                postChars += origChars[end];
                bareChars[end] = ' ';
                --end;
            }

            int len = end - start + 1;

            if (len <= 0) 
                return word;

            Array.Copy(origChars, start, bareChars, start, len);

            string newWord = new string(bareChars).Trim();

            return ModernRomanNumeralsFlexible.IsMatch(newWord)
                ? preChars + newWord.ToUpperInvariant() + postChars
                : word;
        }

        static bool DoesStartOrEndWithPunctuation(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            char[] chars = word.ToCharArray();

            return char.IsPunctuation(chars[0]) || char.IsPunctuation(chars[chars.Length - 1]);
        }

        static bool IsArticleConjunctionOrPreposition(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            return ArticlesConjunctionsPrepositions.Any(s => string.Equals(word, s, StringComparison.OrdinalIgnoreCase));
        }

        static bool IsCommonVerb(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            return CommonVerbs.Any(s => string.Equals(word, s, StringComparison.OrdinalIgnoreCase));
        }

        static bool ContainsPunctuation(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            return word.ToCharArray().Any(char.IsPunctuation);
        }

        static bool IsRomanNumeralWordMatch(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            return RomanNumeralWordMatch.Any(s => string.Equals(word, s, StringComparison.OrdinalIgnoreCase));
        }
    }
}