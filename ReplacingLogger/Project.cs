using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ReplacingLogger
{
    internal class Project
    {
        public string Disambiguator => Disambiguations.Any() ? string.Join(", ", Disambiguations) : string.Empty;

        public readonly List<string> Disambiguations = new List<string>();

        public readonly string[] GlobalProperties;
        public Project(ProjectStartedEventArgs e)
        {
            GlobalProperties = e.GlobalProperties.Select(kvp => $"{kvp.Key}={kvp.Value}").ToArray();
        }
    }
}