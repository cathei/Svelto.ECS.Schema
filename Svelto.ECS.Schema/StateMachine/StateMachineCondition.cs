using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public delegate bool PredicateNative<TComponent>(ref TComponent component)
        where TComponent : unmanaged, IEntityComponent;

    public delegate bool PredicateManaged<TComponent>(ref TComponent component)
        where TComponent : struct, IEntityViewComponent;

    internal abstract class ConditionConfig
    {
        internal ConditionConfig() { }

        internal abstract void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID);

        internal abstract bool Evaluate(uint index);
    }

    internal sealed class ConditionConfigNative<TCondition> : ConditionConfig
        where TCondition : unmanaged, IEntityComponent
    {
        internal readonly PredicateNative<TCondition> _predicate;

        public ConditionConfigNative(PredicateNative<TCondition> predicate)
        {
            _predicate = predicate;
        }

        private readonly ThreadLocal<NB<TCondition>> threadStorage = new ThreadLocal<NB<TCondition>>();

        internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            // this is calling per group here, for this condition
            (threadStorage.Value, _) = entitiesDB.QueryEntities<TCondition>(groupID);
        }

        internal override bool Evaluate(uint index)
        {
            return _predicate(ref threadStorage.Value[index]);
        }
    }

    internal sealed class ConditionConfigManaged<TCondition> : ConditionConfig
        where TCondition : struct, IEntityViewComponent
    {
        internal readonly PredicateManaged<TCondition> _predicate;

        public ConditionConfigManaged(PredicateManaged<TCondition> predicate)
        {
            _predicate = predicate;
        }

        private readonly ThreadLocal<MB<TCondition>> threadStorage = new ThreadLocal<MB<TCondition>>();

        internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            // this is calling per group here, for this condition
            (threadStorage.Value, _) = entitiesDB.QueryEntities<TCondition>(groupID);
        }

        internal override bool Evaluate(uint index)
        {
            return _predicate(ref threadStorage.Value[index]);
        }
    }
}