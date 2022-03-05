using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // empty component that always exists
    internal struct RowIdentityComponent : IEntityComponent { }
}

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// Descriptor Rows are not meant to be extended and child classes should be sealed
    /// Use interface Rows to extend traits!
    /// </summary>
    public abstract class DescriptorRow<TSelf> : IEntityRow
        where TSelf : DescriptorRow<TSelf>
    {
        public class Descriptor : IDynamicEntityDescriptor
        {
            public IComponentBuilder[] componentsToBuild { get; }

            public Descriptor()
            {
                // reflection time!
                // for all interface the row implements
                var interfaceTypes = typeof(TSelf).GetInterfaces();
                var componentBuilderDict = new FasterDictionary<RefWrapperType, IComponentBuilder>();

                componentBuilderDict.Add(
                    TypeRefWrapper<RowIdentityComponent>.wrapper,
                    new ComponentBuilder<RowIdentityComponent>());

                foreach (var interfaceType in interfaceTypes)
                {
                    // we are finding generics only
                    if (!interfaceType.IsGenericType)
                        continue;

                    var genericDefinition = interfaceType.GetGenericTypeDefinition();

                    if (genericDefinition == typeof(IEntityRow<>) ||
                        genericDefinition == typeof(ISelectorRow<,>) ||
                        genericDefinition == typeof(ISelectorRow<,,>) ||
                        genericDefinition == typeof(ISelectorRow<,,,>))
                    {
                        var componentBuildersField = interfaceType.GetField(
                            nameof(IEntityRow<EGIDComponent>.componentBuilders),
                            BindingFlags.Static | BindingFlags.NonPublic);

                        var componentBuilders = (IComponentBuilder[])componentBuildersField.GetValue(null);

                        // prevent duplication with Dictionary
                        foreach (var componentBuilder in componentBuilders)
                        {
                            var wrapper = new RefWrapperType(componentBuilder.GetEntityComponentType());
                            componentBuilderDict[wrapper] = componentBuilder;
                        }
                    }
                }

                componentsToBuild = new IComponentBuilder[componentBuilderDict.count];
                componentBuilderDict.CopyValuesTo(componentsToBuild);
            }
        }
    }
}