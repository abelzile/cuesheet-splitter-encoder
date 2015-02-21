using System;


namespace CuesheetSplitterEncoder.Core.Exceptions
{
    public class CommandLineOperationException : Exception
    {
        readonly int _exitCode;

        public CommandLineOperationException()
        {
        }

        public CommandLineOperationException(string message)
            : base(message)
        {
        }

        public CommandLineOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public CommandLineOperationException(string message, int exitCode)
            : base(message)
        {
            _exitCode = exitCode;
        }

        public int ExitCode
        {
            get { return _exitCode; }
        }

        public override string ToString()
        {
            return string.Format("Exit Code: {0}{1}{2}", _exitCode, Environment.NewLine, base.ToString());
        }
    }
}