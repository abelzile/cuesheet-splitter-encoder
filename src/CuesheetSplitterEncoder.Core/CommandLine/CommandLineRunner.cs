using System;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace CuesheetSplitterEncoder.Core.CommandLine
{
    public class CommandLineRunner
    {
        const int DefaultTimeout = 300000;
                
        readonly string _args;
        readonly int _timeout;
        readonly StringBuilder _output = new StringBuilder();
        readonly StringBuilder _error = new StringBuilder();
        int _exitCode;
        
        public CommandLineRunner(string args, int timeout)
        {
            if (string.IsNullOrWhiteSpace(args)) 
                throw new ArgumentNullException("args");

            if (timeout < 0)
            {
                timeout = DefaultTimeout;
            }

            _args = args;
            _timeout = timeout;
        }

        public CommandLineRunner(string args)
            : this(args, DefaultTimeout)
        {
        }

        public string StandardOutput
        {
            get { return _output.ToString(); }
        }

        public string StandardError
        {
            get { return _error.ToString(); }
        }

        public int ExitCode
        {
            get { return _exitCode; }
        }

        public void Run()
        {
            _exitCode = -1;
            _output.Clear();
            _error.Clear();

            using (var process = new Process())
            {
                process.StartInfo.FileName = "cmd";
                process.StartInfo.Arguments = _args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, eventArgs) =>
                    {
                        if (eventArgs.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            _output.AppendLine(eventArgs.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, eventArgs) =>
                    {
                        if (eventArgs.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            _error.AppendLine(eventArgs.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    
                    if (process.WaitForExit(_timeout) && outputWaitHandle.WaitOne(_timeout) && errorWaitHandle.WaitOne(_timeout))
                    {
                        _exitCode = process.ExitCode;
                    }
                    else
                    {
                        throw new TimeoutException("Command timed out.");
                    }
                }
            }
        } 
    }
}