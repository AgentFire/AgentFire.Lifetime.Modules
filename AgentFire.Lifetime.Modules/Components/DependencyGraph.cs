using System;
using System.Collections.Generic;

namespace AgentFire.Lifetime.Modules.Components
{
    internal sealed class DependencyGraph<T>
    {
        public IReadOnlyList<DependencyItem<T>> Graph => _items;
        private readonly List<DependencyItem<T>> _items = new List<DependencyItem<T>>();

        public IReadOnlyList<DependencyItem<T>> ReversedGraph => _reversed;
        private readonly List<DependencyItem<T>> _reversed = new List<DependencyItem<T>>();

        public DependencyGraph(IEnumerable<T> source, Func<T, IEnumerable<T>> dependencyResolver)
        {
            Dictionary<T, DependencyItem<T>> dic = new Dictionary<T, DependencyItem<T>>();
            DependencyItem<T> DicItem(T item) => dic.GetOrCreate(item, () => new DependencyItem<T>(item, new List<DependencyItem<T>>()));

            Dictionary<T, DependencyItem<T>> dicReversed = new Dictionary<T, DependencyItem<T>>();
            DependencyItem<T> DicReversedItem(T item) => dicReversed.GetOrCreate(item, () => new DependencyItem<T>(item, new List<DependencyItem<T>>()));

            foreach (T module in source)
            {
                DependencyItem<T> dependentModule = DicItem(module);
                DependencyItem<T> reversedDependentModule = DicReversedItem(module);
                var list = (List<DependencyItem<T>>)dependentModule.DependsOn;

                foreach (T dep in dependencyResolver(module))
                {
                    list.Add(DicItem(dep));

                    DependencyItem<T> rDep = DicReversedItem(dep);
                    var reversedList = (List<DependencyItem<T>>)rDep.DependsOn;
                    reversedList.Add(reversedDependentModule);
                }
            }

            _items.AddRange(dic.Values);
            _reversed.AddRange(dicReversed.Values);
        }
    }
}
