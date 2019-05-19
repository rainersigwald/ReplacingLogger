using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace ReplacingLogger
{
    internal class NodeState
    {
        public string Project;
        public string Target;

        internal IObservable<NodeState> Observe()
        {
            return Observable.ToObservable(GetTime()).Delay(TimeSpan.FromMilliseconds(50)).Repeat();

            IEnumerable<NodeState> GetTime()
            {
                yield return new NodeState
                {
                    Project = this.Project,
                    Target = this.Target
                };
            }
        }
    }
}