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

        private NB<TCondition> _target;

        public ConditionConfigNative(PredicateNative<TCondition> predicate)
        {
            _predicate = predicate;
        }

        internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            // this is calling per group here, for this condition
            (_target, _) = entitiesDB.QueryEntities<TCondition>(groupID);
        }

        internal override bool Evaluate(uint index)
        {
            return _predicate(ref _target[index]);
        }
    }

    internal sealed class ConditionConfigManaged<TCondition> : ConditionConfig
        where TCondition : struct, IEntityViewComponent
    {
        internal readonly PredicateManaged<TCondition> _predicate;

        private MB<TCondition> _target;

        public ConditionConfigManaged(PredicateManaged<TCondition> predicate)
        {
            _predicate = predicate;
        }

        internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            // this is calling per group here, for this condition
            (_target, _) = entitiesDB.QueryEntities<TCondition>(groupID);
        }

        internal override bool Evaluate(uint index)
        {
            return _predicate(ref _target[index]);
        }
    }
}