using System;
using System.Collections.Generic;

namespace AgentFire.Lifetime.Modules.Components
{
    /// <summary>
    /// A default resolver. It uses the default constuctor and stores all created instances.
    /// </summary>
    public sealed class DefaultModuleResolver : IModuleResolver
    {
        private readonly Dictionary<Type, IModule> _dic = new Dictionary<Type, IModule>();

        /// <summary>
        /// Either creates and stores the value via reflected default constructor, or fails and returns null.
        /// </summary>
        public T TryGet<T>() where T : IModule
        {
            return (T)TryGet(typeof(T));
        }

        /// <summary>
        /// Either creates and stores the value via reflected default constructor, or fails and returns null.
        /// </summary>
        public IModule TryGet(Type type)
        {
            if (!typeof(IModule).IsAssignableFrom(type))
            {
                return null;
            }

            lock (_dic)
            {
                if (!_dic.TryGetValue(type, out IModule m))
                {
                    m = type.IsAbstract ? null : (IModule)type.GetConstructor(Type.EmptyTypes)?.Invoke(null);

                    _dic[type] = m;
                }

                return m;
            }
        }

        /// <summary>
        /// Removes all cached instances, if any.
        /// </summary>
        public void Reset()
        {
            _dic.Clear();
        }
    }
}
