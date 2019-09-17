using System;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Your main dependency on other module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ModuleDependencyAttribute : Attribute
    {
        internal Type DependsOn { get; }

        /// <summary>
        /// Ctor XML stub.
        /// </summary>
        public ModuleDependencyAttribute(Type dependsOn)
        {
            DependsOn = dependsOn;
        }
    }
}
