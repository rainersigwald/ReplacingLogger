using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Build.Framework;

namespace ReplacingLogger
{
    public class ReplacingLogger : ILogger, INodeLogger
    {
        private static readonly TimeSpan ConsoleRedrawInterval = TimeSpan.FromSeconds(0.25);

        private Thread loggingThread;
        private List<NodeState> Nodes;
        private Dictionary<int, Project> ProjectsById;
        private ConcurrentDictionary<string, List<Project>> ProjectsByPath;

        private List<string> Messages = new List<string>();

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

            ProjectsById = new Dictionary<int, Project>();
            ProjectsByPath = new ConcurrentDictionary<string, List<Project>>();

            CancellationTokenSource = new CancellationTokenSource();

            eventSource.ProjectStarted += ProjectStartedHandler;
            eventSource.ProjectFinished += IncrementCompleted;

            eventSource.TargetStarted += TargetStartedHandler;
            eventSource.TargetFinished += TargetFinishedHandler;

            eventSource.MessageRaised += MessageRaisedHandler;
            eventSource.WarningRaised += WarningRaisedHandler;
            eventSource.ErrorRaised += ErrorRaisedHandler;

            loggingThread = new Thread(LoggingThreadProc)
            {
                Name = nameof(ReplacingLogger) + " logging thread"
            };
            loggingThread.Start(CancellationTokenSource.Token);
        }

        private void ErrorRaisedHandler(object sender, BuildErrorEventArgs e)
        {
            Messages.Add(e.Message);
        }

        private void MessageRaisedHandler(object sender, BuildMessageEventArgs e)
        {
            if (e.Importance == MessageImportance.High)
            {
                Messages.Add(e.Message);
            }
        }

        private void WarningRaisedHandler(object sender, BuildWarningEventArgs e)
        {
            Messages.Add(e.Message);
        }

        private void IncrementCompleted(object sender, ProjectFinishedEventArgs e)
        {
            CompletedRequests++;
        }

        private void ProjectStartedHandler(object sender, ProjectStartedEventArgs e)
        {
            TotalRequests++;

            if (!ProjectsById.ContainsKey(e.BuildEventContext.ProjectInstanceId))
            {
                var p = new Project(e);
                ProjectsById[e.BuildEventContext.ProjectInstanceId] = p;

                var list = ProjectsByPath.GetOrAdd(e.ProjectFile,
                                                   (_) => new List<Project>());

                list.Add(p);

                if (list.Count > 1)
                {
                    Disambiguate(list);
                }
            }
        }

        private void Disambiguate(List<Project> list)
        {
            int totalProjects = list.Count;

            var d = new ConcurrentDictionary<string, List<Project>>();
            foreach (var project in list)
            {
                // clear current disambiguation; it's completely recalculated
                project.Disambiguations.Clear();

                foreach (var property in project.GlobalProperties)
                {
                    d.AddOrUpdate(property,
                        (_) => new List<Project> { project },
                        (_, projects) =>
                          {
                              projects.Add(project);
                              return projects;
                          });
                }
            }

            var disambiguatingProperties = new List<string>();

            foreach (var globalProperty in d)
            {
                var propertyString = globalProperty.Key;
                var projectsWithThatProperty = globalProperty.Value;

                if (projectsWithThatProperty.Count != totalProjects)
                {
                    disambiguatingProperties.Add(propertyString);
                    foreach (var p in projectsWithThatProperty)
                    {
                        p.Disambiguations.Add(propertyString);
                    }
                }
            }
        }

        private void TargetStartedHandler(object sender, TargetStartedEventArgs e)
        {
            var node = Nodes[e.BuildEventContext.NodeId];

            node.Project = e.ProjectFile + ProjectsById[e.BuildEventContext.ProjectInstanceId].Disambiguator;
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

        }
    }
}