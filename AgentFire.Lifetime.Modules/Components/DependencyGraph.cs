using QuickGraph;
using QuickGraph.Algorithms.TopologicalSort;
using System;
using System.Collections.Generic;

namespace AgentFire.Lifetime.Modules.Components
{
    internal sealed class DependencyGraph<T>
    {
        public IReadOnlyCollection<DependencyItem<T>> Forward { get; }
        public IReadOnlyCollection<DependencyItem<T>> Backward { get; }

        /// <exception cref="NonAcyclicGraphException" />
        /// <exception cref="KeyNotFoundException" />
        public DependencyGraph(IEnumerable<T> source, Func<T, IEnumerable<T>> dependencyResolver)
        {
            var graph = new AdjacencyGraph<T, Edge<T>>();

            var dic = new Dictionary<T, DependencyItem<T>>();
            var dicReversed = new Dictionary<T, DependencyItem<T>>();
            var builders = new Dictionary<DependencyItem<T>, DependencyItemLiveBuilder<T>>();

            DependencyItem<T> DicItem(T item, bool useReversed) => (useReversed ? dicReversed : dic).GetOrCreate(item, () =>
            {
                var builder = new DependencyItemLiveBuilder<T>(item);
                builders[builder.Item] = builder;
                return builder.Item;
            });

            void Add(DependencyItem<T> store, DependencyItem<T> item) => builders[store].AddDependency(item);

            foreach (T module in source)
            {
                graph.AddVertex(module);

                DependencyItem<T> dependentModule = DicItem(module, false);
                DependencyItem<T> reversedDependentModule = DicItem(module, true);

                foreach (T dep in dependencyResolver(module))
                {
                    graph.AddEdge(new Edge<T>(module, dep));

                    // Forward dependency.
                    Add(dependentModule, DicItem(dep, false));

                    // Backward dependency.
                    Add(DicItem(dep, true), reversedDependentModule);
                }
            }

            try
            {
                new TopologicalSortAlgorithm<T, Edge<T>>(graph).Compute();
            }
            catch (NonAcyclicGraphException)
            {
                throw;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }

            Forward = dic.Values;
            Backward = dicReversed.Values;
        }
    }
}
