using System;
using Microsoft.Build.Framework;

namespace ReplacingLogger
{
    public class ReplacingLogger : ILogger, INodeLogger
    {
        public LoggerVerbosity Verbosity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Parameters { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Initialize(IEventSource eventSource, int nodeCount)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IEventSource eventSource)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
