using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Build.Framework;

namespace ReplacingLogger
{
    public class ReplacingLogger : ILogger, INodeLogger
    {
        private static readonly TimeSpan ConsoleRedrawInterval = TimeSpan.FromSeconds(0.25);

        private Thread loggingThread;
        private List<NodeState> Nodes;
        private CancellationTokenSource CancellationTokenSource;
        private int NodeCount = 1;

        private uint TotalRequests = 0;
        private uint CompletedRequests = 0;

        public LoggerVerbosity Verbosity
        {
            get => LoggerVerbosity.Minimal;
            set { return; }
        }

        public string Parameters { get; set; }

        public void Initialize(IEventSource eventSource, int nodeCount)
        {
            NodeCount = nodeCount;
            Initialize(eventSource);
        }

        public void Initialize(IEventSource eventSource)
        {
            Nodes = new List<NodeState>(NodeCount + 1);
            for (int i = 0; i <= NodeCount; i++)
            {
                Nodes.Add(new NodeState());
            }

            CancellationTokenSource = new CancellationTokenSource();

            eventSource.ProjectStarted += IncrementRequestCount;
            eventSource.ProjectFinished += IncrementCompleted;

            eventSource.TargetStarted += TargetStartedHandler;
            eventSource.TargetFinished += TargetFinishedHandler;

            loggingThread = new Thread(LoggingThreadProc)
            {
                Name = nameof(ReplacingLogger) + " logging thread"
            };
            loggingThread.Start(CancellationTokenSource.Token);
        }

        private void IncrementCompleted(object sender, ProjectFinishedEventArgs e)
        {
            CompletedRequests++;
        }

        private void IncrementRequestCount(object sender, ProjectStartedEventArgs e)
        {
            TotalRequests++;
        }

        private void TargetStartedHandler(object sender, TargetStartedEventArgs e)
        {
            var node = Nodes[e.BuildEventContext.NodeId];

            node.Project = e.ProjectFile;
            node.Target = e.TargetName;
        }

        private void TargetFinishedHandler(object sender, TargetFinishedEventArgs e)
        {
            var node = Nodes[e.BuildEventContext.NodeId];

            node.Project = string.Empty;
            node.Target = string.Empty;
        }


        public void Shutdown()
        {
            CancellationTokenSource.Cancel();

            Thread.Sleep(ConsoleRedrawInterval);

            Console.WriteLine();
        }

        private void LoggingThreadProc(object obj)
        {
            CancellationToken ct = (CancellationToken)obj;

            Console.CursorVisible = false;

            int column = Console.CursorLeft;
            int row = Console.CursorTop;

            int furthestColumn = Console.CursorLeft;
            int furthestRow = Console.CursorTop;

            Console.WriteLine("Starting logger");

            while (!ct.IsCancellationRequested)
            {
                column = Console.CursorLeft;
                row = Console.CursorTop;

                var startLine = new string(' ', Console.WindowWidth - 1).ToCharArray();

                string initialLine = $"======================== {(CompletedRequests == 0 ? 0f : (float)CompletedRequests / TotalRequests),3:#0%}";
                initialLine.CopyTo(0, startLine, 0, Math.Min(initialLine.Length, startLine.Length));

                Console.WriteLine(startLine);
                for (int i = 1; i < Nodes.Count; i++)
                {
                    var line = new string(' ', Console.WindowWidth - 1).ToCharArray();

                    string logMessage = $"{i:00}: {Nodes[i].Project} - {Nodes[i].Target}";
                    logMessage.CopyTo(0, line, 0, Math.Min(logMessage.Length, line.Length));

                    Console.WriteLine(line);
                }

                furthestColumn = Console.CursorLeft;
                furthestRow = Console.CursorTop;

                Console.SetCursorPosition(column, row);

                Thread.Sleep(ConsoleRedrawInterval);
            }

            Console.SetCursorPosition(furthestColumn, furthestRow);
            Console.CursorVisible = true;
        }
    }
}