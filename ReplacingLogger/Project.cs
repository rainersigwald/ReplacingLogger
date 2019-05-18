using Microsoft.Build.Framework;

namespace ReplacingLogger
{
    internal class Project
    {
        public string Disambiguator { get; set; } = null;
        public Project(ProjectStartedEventArgs e)
        {
        }
    }
}