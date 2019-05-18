using Microsoft.Build.Framework;
using System.Linq;

namespace ReplacingLogger
{
    internal class Project
    {
        public string Disambiguator { get; set; } = string.Empty;
        public readonly string[] GlobalProperties;
        public Project(ProjectStartedEventArgs e)
        {
            GlobalProperties = e.GlobalProperties.Select(kvp => $"{kvp.Key}={kvp.Value}").ToArray();
        }
    }
}