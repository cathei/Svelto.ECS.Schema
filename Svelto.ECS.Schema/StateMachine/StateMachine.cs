using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public delegate bool PredicateNative<TComponent>(ref TComponent component)
        where TComponent : unmanaged, IEntityComponent;

    public delegate bool PredicateManaged<TComponent>(ref TComponent component)
        where TComponent : struct, IEntityViewComponent;

    public delegate void CallbackNative<TComponent>(ref TComponent component)
        where TComponent : unmanaged, IEntityComponent;

    public delegate void CallbackManaged<TComponent>(ref TComponent component)
        where TComponent : struct, IEntityViewComponent;
}

namespace Svelto.ECS.Schema.Definition
{
    /// <summary>
    /// State machine on Svelto ECS
    /// </summary>
    /// <typeparam name="TKey">Value type representing a state. Usually wraps enum type.</typeparam>
    public abstract partial class StateMachine<TKey> : IEntityStateMachine,
            IIndexQueryable<StateMachine<TKey>.IIndexedRow, TKey>
        where TKey : unmanaged, IStateMachineKey<TKey>
    {
        public interface ITag {}

        internal static StateMachineConfigBase Config = null;

        public IStepEngine Engine { get; internal set; }

        protected StateMachine()
        {
            if (Config != null)
                return;

            lock (EntitySchemaLock.Lock)
            {
                if (Config != null)
                    return;

                OnConfigure();
            }

            if (Config == null)
                throw new ECSException("StateMachine is not properly configured!");
        }

        protected abstract void OnConfigure();

        internal abstract class ConditionConfig
        {
            internal ConditionConfig() { }

            internal abstract void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID);

            internal abstract bool Evaluate(uint index);
        }

        internal sealed class ConditionConfigNative<TComponent> : ConditionConfig
            where TComponent : unmanaged, IEntityComponent
        {
            internal readonly PredicateNative<TComponent> _predicate;

            private NB<TComponent> _target;

            public ConditionConfigNative(PredicateNative<TComponent> predicate)
            {
                _predicate = predicate;
            }

            internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this condition
                (_target, _) = entitiesDB.QueryEntities<TComponent>(groupID);
            }

            internal override bool Evaluate(uint index)
            {
                return _predicate(ref _target[index]);
            }
        }

        internal sealed class ConditionConfigManaged<TComponent> : ConditionConfig
            where TComponent : struct, IEntityViewComponent
        {
            internal readonly PredicateManaged<TComponent> _predicate;

            private MB<TComponent> _target;

            public ConditionConfigManaged(PredicateManaged<TComponent> predicate)
            {
                _predicate = predicate;
            }

            internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this condition
                (_target, _) = entitiesDB.QueryEntities<TComponent>(groupID);
            }

            internal override bool Evaluate(uint index)
            {
                return _predicate(ref _target[index]);
            }
        }

        internal abstract class CallbackConfig
        {
            internal CallbackConfig() { }

            internal abstract void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID);

            internal abstract void Invoke(uint index);
        }

        internal sealed class CallbackConfigNative<TComponent> : CallbackConfig
            where TComponent : unmanaged, IEntityComponent
        {
            internal readonly CallbackNative<TComponent> _callback;

            private NB<TComponent> _target;

            public CallbackConfigNative(CallbackNative<TComponent> callback)
            {
                _callback = callback;
            }

            internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
            {
                (_target, _) = entitiesDB.QueryEntities<TComponent>(groupID);
            }

            internal override void Invoke(uint index)
            {
                _callback(ref _target[index]);
            }
        }

        internal sealed class CallbackConfigManaged<TComponent> : CallbackConfig
            where TComponent : struct, IEntityViewComponent
        {
            internal readonly CallbackManaged<TComponent> _callback;

            private MB<TComponent> _target;

            public CallbackConfigManaged(CallbackManaged<TComponent> callback)
            {
                _callback = callback;
            }

            internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
            {
                (_target, _) = entitiesDB.QueryEntities<TComponent>(groupID);
            }

            internal override void Invoke(uint index)
            {
                _callback(ref _target[index]);
            }
        }

        internal sealed class TransitionConfig
        {
            internal readonly TKey _next;
            internal readonly FasterList<ConditionConfig> _conditions;

            internal TransitionConfig(in TKey next)
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

        internal sealed class StateConfig
        {
            internal readonly TKey _key;
            internal readonly FasterList<TransitionConfig> _transitions;

            internal readonly Memo _exitCandidates;
            internal readonly Memo _enterCandidates;

            internal readonly FasterList<CallbackConfig> _onExit;
            internal readonly FasterList<CallbackConfig> _onEnter;

            internal StateConfig(in TKey state)
            {
                _key = state;
                _transitions = new FasterList<TransitionConfig>();

                _exitCandidates = new Memo();
                _enterCandidates = new Memo();

                _onExit = new FasterList<CallbackConfig>();
                _onEnter = new FasterList<CallbackConfig>();
            }

            internal void Evaluate<TR>(StateMachineConfig<TR> config, IndexedDB indexedDB,
                    NB<Component> component, IEntityTable<TR> table)
                where TR : class, IIndexedRow
            {
                var indices = indexedDB
                    .Select<IIndexedRow>().From(table).Where(config._index, _key).Indices();

                // nothing to check
                if (indices.Count() == 0)
                    return;

                for (int i = 0; i < _transitions.count; ++i)
                    _transitions[i].Ready(indexedDB.entitiesDB, table.ExclusiveGroup);

                foreach (uint index in indices)
                {
                    // transition has higher priority if added first
                    for (int i = 0; i < _transitions.count; ++i)
                    {
                        if (!_transitions[i].Evaluate(index))
                            continue;

                        // register to execute transition
                        var currentState = new KeyWrapper<TKey>(component[index]._key);
                        var nextState = new KeyWrapper<TKey>(_transitions[i]._next);

                        indexedDB.Memo(config._states[currentState]._exitCandidates).Add(component[i]);
                        indexedDB.Memo(config._states[nextState]._enterCandidates).Add(component[i]);
                        break;
                    }
                }
            }

            internal void ProcessExit(IndexedDB indexedDB, IEntityTable<IIndexedRow> table)
            {
                if (_onExit.count == 0)
                    return;

                var indices = indexedDB
                    .Select<IIndexedRow>().From(table).Where(_exitCandidates).Indices();

                if (indices.Count() == 0)
                    return;

                for (int i = 0; i < _onExit.count; ++i)
                    _onExit[i].Ready(indexedDB.entitiesDB, table.ExclusiveGroup);

                foreach (uint index in indices)
                {
                    for (int i = 0; i < _onExit.count; ++i)
                        _onExit[i].Invoke(index);
                }
            }

            internal void ProcessEnter(IndexedDB indexedDB, NB<Component> component, IEntityTable<IIndexedRow> table)
            {
                var indices = indexedDB
                    .Select<IIndexedRow>().From(table).Where(_enterCandidates).Indices();

                if (indices.Count() == 0)
                    return;

                for (int i = 0; i < _onEnter.count; ++i)
                    _onEnter[i].Ready(indexedDB.entitiesDB, table.ExclusiveGroup);

                foreach (uint index in indices)
                {
                    // this group will not be visited again in this step
                    // updating indexes
                    var oldState = component[index]._key;
                    component[index]._key = _key;

                    indexedDB.NotifyKeyUpdate(ref component[index], oldState, _key);

                    for (int i = 0; i < _onEnter.count; ++i)
                        _onEnter[i].Invoke(index);
                }
            }
        }

        internal sealed class AnyStateConfig
        {
            internal readonly FasterList<TransitionConfig> _transitions;

            internal AnyStateConfig()
            {
                _transitions = new FasterList<TransitionConfig>();
            }

            internal void Evaluate<TR>(StateMachineConfig<TR> config, IndexedDB indexedDB,
                    NB<Component> component, int count, IEntityTable<TR> table)
                where TR : class, IIndexedRow
            {
                for (int i = 0; i < _transitions.count; ++i)
                    _transitions[i].Ready(indexedDB.entitiesDB, table.ExclusiveGroup);

                for (uint index = 0; index < count; ++index)
                {
                    for (int i = 0; i < _transitions.count; ++i)
                    {
                        if (!_transitions[i].Evaluate(index))
                            continue;

                        // register to execute transition
                        var currentState = new KeyWrapper<TKey>(component[index]._key);
                        var nextState = new KeyWrapper<TKey>(_transitions[i]._next);

                        indexedDB.Memo(config._states[currentState]._exitCandidates).Add(component[i]);
                        indexedDB.Memo(config._states[nextState]._enterCandidates).Add(component[i]);
                        break;
                    }
                }
            }
        }
    }
}
