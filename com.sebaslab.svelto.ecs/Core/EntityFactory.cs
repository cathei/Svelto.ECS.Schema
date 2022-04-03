using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        public static FasterDictionary<RefWrapperType, ITypeSafeDictionary> BuildGroupedEntities
        (EGID egid, EnginesRoot.DoubleBufferedEntitiesToAdd groupEntitiesToAdd, IComponentBuilder[] componentsToBuild
       , IEnumerable<object> implementors
#if DEBUG && !PROFILE_SVELTO
       , System.Type descriptorType
#endif
        )
        {
            var group = groupEntitiesToAdd.currentComponentsToAddPerGroup.GetOrAdd(
                egid.groupID, () => new FasterDictionary<RefWrapperType, ITypeSafeDictionary>());

            //track the number of entities created so far in the group.
            groupEntitiesToAdd.IncrementEntityCount(egid.groupID);

            BuildEntitiesAndAddToGroup(egid, group, componentsToBuild, implementors
#if DEBUG && !PROFILE_SVELTO
                                     , descriptorType
#endif
            );

            return group;
        }

        static void BuildEntitiesAndAddToGroup
        (EGID entityID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> @group
       , IComponentBuilder[] componentBuilders, IEnumerable<object> implementors
#if DEBUG && !PROFILE_SVELTO
       , System.Type descriptorType
#endif
        )
        {
#if DEBUG && !PROFILE_SVELTO
            DBC.ECS.Check.Require(componentBuilders != null, $"Invalid Entity Descriptor {descriptorType}");
#endif
            var numberOfComponents = componentBuilders.Length;

#if DEBUG && !PROFILE_SVELTO
            var types = new HashSet<System.Type>();

            for (var index = 0; index < numberOfComponents; ++index)
            {
                var entityComponentType = componentBuilders[index].GetEntityComponentType();
                if (types.Contains(entityComponentType))
                {
                    throw new ECSException(
                        $"EntityBuilders must be unique inside an EntityDescriptor. Descriptor Type {descriptorType} Component Type: {entityComponentType}");
                }

                types.Add(entityComponentType);
            }
#endif
            for (var index = 0; index < numberOfComponents; ++index)
            {
                var entityComponentBuilder = componentBuilders[index];

                BuildEntity(entityID, @group, entityComponentBuilder, implementors);
            }
        }

        static void BuildEntity
        (EGID entityID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> group
       , IComponentBuilder componentBuilder, IEnumerable<object> implementors)
        {
            var entityComponentType = componentBuilder.GetEntityComponentType();
            ITypeSafeDictionary safeDictionary = group.GetOrAdd(new RefWrapperType(entityComponentType)
                                                 , (ref IComponentBuilder cb) => cb.CreateDictionary(1)
                                                 , ref componentBuilder);

            //   if the safeDictionary hasn't been created yet, it will be created inside this method. 
            componentBuilder.BuildEntityAndAddToList(safeDictionary, entityID, implementors);
        }
    }
}