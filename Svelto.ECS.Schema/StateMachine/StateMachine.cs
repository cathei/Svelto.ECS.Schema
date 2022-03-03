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

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// State machine on Svelto ECS
    /// </summary>
    /// <typeparam name="TState">Value type representing a state. Usually enum type.</typeparam>
    /// <typeparam name="TTag">Type to ensure uniqueness. It can be done with TSelf, but IsUnmanagedEx is on the way</typeparam>
    public abstract partial class StateMachine<TState, TTag> : IEntityStateMachine,
            IIndexQueryable<StateMachine<TState, TTag>.IIndexedRow, TState, StateMachine<TState, TTag>.Component>
        where TState : unmanaged
        where TTag : unmanaged, StateMachine<TState, TTag>.ITag
    {
        public interface ITag {}

        internal static IStateMachineConfig Config = null;

        public TransitionEngine Engine { get; internal set; }

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

            internal abstract void Evaluate<TEnum, TIter>(EntitiesDB entitiesDB,
                    NB<Component> component, in TEnum indexedIndices, in ExclusiveGroupStruct groupID)
                where TEnum : struct, IIndicesEnumerable<TIter>
                where TIter : struct, IIndicesEnumerator;
        }

        internal sealed class ConditionConfigNative<TComponent> : ConditionConfig
            where TComponent : unmanaged, IEntityComponent
        {
            internal readonly PredicateNative<TComponent> _predicate;

            public ConditionConfigNative(PredicateNative<TComponent> predicate)
            {
                _predicate = predicate;
            }

            internal override void Evaluate<TEnum, TIter>(EntitiesDB entitiesDB,
                NB<Component> component, in TEnum indices, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this condition
                var (target, _) = entitiesDB.QueryEntities<TComponent>(groupID);

                // rather loop through indexes multiple times.
                // should be better than fetching buffers per entity.
                foreach (int i in indices)
                {
                    // just skip any component that is not available
                    if (component[i].transitionState != TransitionState.Available)
                        continue;

                    if (!_predicate(ref target[i]))
                        component[i].transitionState = TransitionState.Aborted;
                }
            }
        }

        internal sealed class ConditionConfigManaged<TComponent> : ConditionConfig
            where TComponent : struct, IEntityViewComponent
        {
            internal readonly PredicateManaged<TComponent> _predicate;

            public ConditionConfigManaged(PredicateManaged<TComponent> predicate)
            {
                _predicate = predicate;
            }

            internal override void Evaluate<TEnum, TIter>(EntitiesDB entitiesDB,
                NB<Component> component, in TEnum indices, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this condition
                var (target, _) = entitiesDB.QueryEntities<TComponent>(groupID);

                // rather loop through indexes multiple times.
                // should be better than fetching buffers per entity.
                foreach (int i in indices)
                {
                    // just skip any component that is not available
                    if (component[i].transitionState != TransitionState.Available)
                        continue;

                    if (!_predicate(ref target[i]))
                        component[i].transitionState = TransitionState.Aborted;
                }
            }
        }

        internal abstract class CallbackConfig
        {
            internal CallbackConfig() { }

            internal abstract void Invoke(EntitiesDB entitiesDB,
                IndexedIndices indexedIndices, in ExclusiveGroupStruct groupID);
        }

        internal sealed class CallbackConfigNative<TComponent> : CallbackConfig
            where TComponent : unmanaged, IEntityComponent
        {
            internal readonly CallbackNative<TComponent> _callback;

            public CallbackConfigNative(CallbackNative<TComponent> callback)
            {
                _callback = callback;
            }

            internal override void Invoke(EntitiesDB entitiesDB,
                IndexedIndices indexedIndices, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this callback
                var (target, _) = entitiesDB.QueryEntities<TComponent>(groupID);

                foreach (int i in indexedIndices)
                {
                    _callback(ref target[i]);
                }
            }
        }

        internal sealed class CallbackConfigManaged<TComponent> : CallbackConfig
            where TComponent : struct, IEntityViewComponent
        {
            internal readonly CallbackManaged<TComponent> _callback;

            public CallbackConfigManaged(CallbackManaged<TComponent> callback)
            {
                _callback = callback;
            }

            internal override void Invoke(EntitiesDB entitiesDB,
                IndexedIndices indexedIndices, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this callback
                var (target, _) = entitiesDB.QueryEntities<TComponent>(groupID);

                foreach (int i in indexedIndices)
                {
                    _callback(ref target[i]);
                }
            }
        }

        internal sealed class TransitionConfig
        {
            internal readonly TState _next;
            internal readonly FasterList<ConditionConfig> _conditions;

            internal TransitionConfig(in TState next)
            {
                _next = next;
                _conditions = new FasterList<ConditionConfig>();
            }

            internal void Evaluate<TEnum, TIter, TR>(StateMachineConfig<TR> config,
                    IndexedDB indexedDB, NB<Component> component, in TEnum indices, in ExclusiveGroupStruct groupID)
                where TEnum : struct, IIndicesEnumerable<TIter>
                where TIter : struct, IIndicesEnumerator
                where TR : class, IIndexedRow
            {
                // rather loop through indexes multiple times.
                // should be better than fetching buffers per entity.
                foreach (int i in indices)
                {
                    if (component[i].transitionState == TransitionState.Aborted)
                        component[i].transitionState = TransitionState.Available;
                }

                // evaluate each condition, they will check with components
                for (int i = 0; i < _conditions.count; ++i)
                {
                    _conditions[i].Evaluate<TEnum, TIter>(indexedDB, component, indices, groupID);
                }

                // now if transitionState is still available, this transition is selected
                foreach (int i in indices)
                {
                    if (component[i].transitionState == TransitionState.Available)
                    {
                        component[i].transitionState = TransitionState.Confirmed;

                        var currentState = new KeyWrapper<TState>(component[i].State);
                        var nextState = new KeyWrapper<TState>(_next);

                        indexedDB.Memo(config._states[currentState]._exitCandidates).Add(component[i]);
                        indexedDB.Memo(config._states[nextState]._enterCandidates).Add(component[i]);
                    }
                }
            }
        }

        internal sealed class StateConfig
        {
            internal readonly TState _state;
            internal readonly FasterList<TransitionConfig> _transitions;

            internal readonly Memo _exitCandidates;
            internal readonly Memo _enterCandidates;

            internal readonly FasterList<CallbackConfig> _onExit;
            internal readonly FasterList<CallbackConfig> _onEnter;

            internal StateConfig(in TState state)
            {
                _state = state;
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
                    .Select<IIndexedRow>().From(table).Where(config._index, _state).Indices();

                // higher priority if added first
                for (int i = 0; i < _transitions.count; ++i)
                {
                    _transitions[i].Evaluate<IndexedIndices, IndexedIndicesEnumerator, TR>
                        (config, indexedDB, component, indices, table.ExclusiveGroup);
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
                {
                    _onExit[i].Invoke(indexedDB.entitiesDB, indices, table.ExclusiveGroup);
                }
            }

            internal void ProcessEnter(IndexedDB indexedDB, NB<Component> component, IEntityTable<IIndexedRow> table)
            {
                var indices = indexedDB
                    .Select<IIndexedRow>().From(table).Where(_enterCandidates).Indices();

                if (indices.Count() == 0)
                    return;

                foreach (int i in indices)
                {
                    // this group will not be visited again in this step
                    // see you next step
                    component[i].transitionState = TransitionState.Available;
                    component[i].Update(indexedDB, _state);
                }

                for (int i = 0; i < _onEnter.count; ++i)
                {
                    _onEnter[i].Invoke(indexedDB.entitiesDB, indices, table.ExclusiveGroup);
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
                    NB<Component> state, int count, IEntityTable<TR> table)
                where TR : class, IIndexedRow
            {
                var indices = new RangeIndiceEnumerable(count);

                for (int i = 0; i < _transitions.count; ++i)
                {
                    _transitions[i].Evaluate<RangeIndiceEnumerable, RangeIndicesEnumerator, TR>
                        (config, indexedDB, state, indices, table.ExclusiveGroup);
                }
            }
        }
    }
}
