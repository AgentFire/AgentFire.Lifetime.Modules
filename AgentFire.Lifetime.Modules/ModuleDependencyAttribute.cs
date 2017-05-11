using System;

namespace AgentFire.Lifetime.Modules
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ModuleDependencyAttribute : Attribute
    {
        public Type DependsOn { get; }

        public ModuleDependencyAttribute(Type dependsOn)
        {
            DependsOn = dependsOn;
        }
    }
}
