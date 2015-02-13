using System;
using System.Text;


namespace CuesheetSplitterEncoder.Core.CommandLine
{
    public class CommandLineBuilder
    {
        readonly StringBuilder _builder = new StringBuilder();

        public CommandLineBuilder(string exeName)
        {
            if (string.IsNullOrWhiteSpace(exeName)) 
                throw new ArgumentNullException("exeName");

            _builder.Append(exeName.Trim()).Append(" ");
        }

        public enum QuoteValue
        {
            Yes,
            No
        }

        public enum SeparatorType
        {
            Space,
            Equals
        }

        public CommandLineBuilder AppendDash(string arg)
        {
            _builder.Append("-").Append(arg).Append(" ");
            return this;
        }

        public CommandLineBuilder AppendDash(string arg,
            string val,
            SeparatorType separatorType,
            QuoteValue quoteVal = QuoteValue.Yes)
        {
            _builder.Append("-").Append(arg).Append(GetSeparatorString(separatorType));
            AppendQuoted(val, quoteVal);
            _builder.Append(" ");

            return this;
        }

        public CommandLineBuilder AppendDoubleDash(string arg)
        {
            _builder.Append("--").Append(arg).Append(" ");
            return this;
        }

        public CommandLineBuilder AppendDoubleDash(
            string arg,
            string val,
            SeparatorType separatorType,
            QuoteValue quoteVal)
        {
            _builder.Append("--").Append(arg).Append(GetSeparatorString(separatorType));
            AppendQuoted(val, quoteVal);
            _builder.Append(" ");

            return this;
        }

        public CommandLineBuilder AppendDoubleDashIfNotNull(
            string arg,
            string val,
            SeparatorType separatorType,
            QuoteValue quoteVal)
        {
            return (val == null) ? this : AppendDoubleDash(arg, val, separatorType, quoteVal);
        }

        public CommandLineBuilder AppendValue(string val, QuoteValue quoteVal = QuoteValue.Yes)
        {
            AppendQuoted(val, quoteVal);
            _builder.Append(" ");
            return this;
        }

        public override string ToString()
        {
            return "/c " + _builder.ToString().Trim();
        }

        static string GetSeparatorString(SeparatorType separatorType)
        {
            switch (separatorType)
            {
                case SeparatorType.Equals:
                    return "=";
                case SeparatorType.Space:
                    return " ";
                default:
                    return " ";
            }
        }

        void AppendQuoted(string val, QuoteValue quoteVal)
        {
            if (quoteVal == QuoteValue.Yes) _builder.Append(@"""");
            _builder.Append(val);
            if (quoteVal == QuoteValue.Yes) _builder.Append(@"""");
        }
    }
}