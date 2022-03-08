using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal sealed class TransitionConfig<TState>
    {
        internal readonly TState _next;
        internal readonly FasterList<ConditionConfig> _conditions;

        internal TransitionConfig(in TState next)
        {
            _next = next;
            _conditions = new FasterList<ConditionConfig>();
        }

        public void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            for (int i = 0; i < _conditions.count; ++i)
                _conditions[i].Ready(entitiesDB, groupID);
        }

        internal bool Evaluate(uint index)
        {
            // evaluate each condition, fail if any of them fails
            for (int i = 0; i < _conditions.count; ++i)
            {
                if (!_conditions[i].Evaluate(index))
                    return false;
            }

            // success, execute transition
            return true;
        }
    }
}