using System.Collections.Generic;

namespace AgentFire.Lifetime.Modules.Components
{
    internal sealed class DependencyItemLiveBuilder<T>
    {
        private readonly List<DependencyItem<T>> _list = new List<DependencyItem<T>>();

        public DependencyItem<T> Item { get; }

        public DependencyItemLiveBuilder(T value)
        {
            Item = new DependencyItem<T>(value, _list);
        }

        public void AddDependency(DependencyItem<T> item)
        {
            _list.Add(item);
        }
    }
}
