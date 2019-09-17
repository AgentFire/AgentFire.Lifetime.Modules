using System;
using System.Collections.Generic;

namespace AgentFire.Lifetime.Modules.Components
{
    internal sealed class DependencyItem<T>
    {
        public T Value { get; }
        public IReadOnlyList<DependencyItem<T>> DependsOn { get; }

        public DependencyItem(T value, IReadOnlyList<DependencyItem<T>> dependsOn)
        {
            Value = value;
            DependsOn = dependsOn;
        }
    }
}
