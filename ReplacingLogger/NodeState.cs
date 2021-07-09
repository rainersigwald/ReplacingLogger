using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

using static System.IO.Path;

namespace ReplacingLogger
{
    internal class NodeState
    {
        private static IRenderable Empty = new Markup(string.Empty);

        public string Project = string.Empty;

        public string Path = string.Empty;
        public string Disambiguator = string.Empty;
        public string Target = string.Empty;

        public IRenderable RenderablePath
        {
            get
            {
                if (string.IsNullOrEmpty(Project))
                {
                    return Empty;
                }
                string filename = GetFileNameWithoutExtension(Path);
                string extension = GetExtension(Path);
                string directory = GetDirectoryName(Path);

                return new Markup($"{directory}{System.IO.Path.DirectorySeparatorChar}[bold]{filename}[/]{extension}");
            }
        }

        public IRenderable RenderableDisambiguator
        {
            get
            {
                if (string.IsNullOrEmpty(Project))
                {
                    return Empty;
                }

                return new Markup($"[italic]{Disambiguator}[/]");
            }
        }

    }
}