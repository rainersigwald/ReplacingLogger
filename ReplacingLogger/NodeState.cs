using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

using static System.IO.Path;

namespace ReplacingLogger
{
    internal class NodeState
    {
        public string Project = string.Empty;

        public string Path = string.Empty;
        public string Disambiguator = string.Empty;
        public string Target = string.Empty;

        public IRenderable ToRenderable()
        {
            if (string.IsNullOrEmpty(Project))
            {
                return new Markup(string.Empty);
            }
            string filename = GetFileName(Path);
            string directory = GetDirectoryName(Path);

            return new Markup($"{directory}{System.IO.Path.DirectorySeparatorChar}[bold]{filename}[/] {Disambiguator}");
        }
    }
}