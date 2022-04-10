using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    partial class StateMachineConfig<TRow, TComponent, TState>
    {
        internal sealed class State
        {
            internal readonly StateMachineConfig<TRow, TComponent, TState> _config;
            internal readonly TState _key;
            internal readonly FasterList<TransitionConfig<TState>> _transitions;

            internal readonly Memo<TRow> _exitCandidates;
            internal readonly Memo<TRow> _enterCandidates;

            internal readonly FasterList<CallbackConfig> _onExit;
            internal readonly FasterList<CallbackConfig> _onEnter;

            internal State(StateMachineConfig<TRow, TComponent, TState> config, in TState state)
            {
                _config = config;
                _key = state;
                _transitions = new FasterList<TransitionConfig<TState>>();

                _exitCandidates = new Memo<TRow>();
                _enterCandidates = new Memo<TRow>();

                _onExit = new FasterList<CallbackConfig>();
                _onEnter = new FasterList<CallbackConfig>();
            }

            internal void Evaluate(IndexedDB indexedDB, in QueryResult<StateMachineSet<TComponent>> result)
            {
                ref var filter = ref _config._index.GetFilter(indexedDB, _key);
                var groupFilter = filter.GetGroupFilter(result.group);

                // nothing to check
                if (groupFilter.count == 0)
                    return;

                var indices = new IndexedIndices(groupFilter.indices);

                for (int i = 0; i < _transitions.count; ++i)
                    _transitions[i].Ready(indexedDB.entitiesDB, result.group);

                foreach (uint index in indices)
                {
                    // transition has higher priority if added first
                    for (int i = 0; i < _transitions.count; ++i)
                    {
                        var transition = _transitions[i];

                        if (!transition.Evaluate(index))
                            continue;

                        ref var current = ref result.set.component[index];

                        // register to execute transition
                        var currentState = current.key;
                        var nextState = transition._next;

                        indexedDB.Memo(_config._states[currentState]._exitCandidates).Add(result.egid[index]);
                        indexedDB.Memo(_config._states[nextState]._enterCandidates).Add(result.egid[index]);
                        break;
                    }
                }
            }

            internal void ProcessExit(IndexedDB indexedDB, in QueryResult<StateMachineSet<TComponent>> result)
            {
                if (_onExit.count == 0)
                    return;

                ref var filter = ref _exitCandidates.GetFilter(indexedDB);
                var groupFilter = filter.GetGroupFilter(result.group);

                // nothing to check
                if (groupFilter.count == 0)
                    return;

                var indices = new IndexedIndices(groupFilter.indices);

                for (int i = 0; i < _onExit.count; ++i)
                    _onExit[i].Ready(indexedDB.entitiesDB, result.group);

                foreach (uint index in indices)
                {
                    for (int i = 0; i < _onExit.count; ++i)
                        _onExit[i].Invoke(index);
                }
            }

            internal void ProcessEnter(IndexedDB indexedDB, in QueryResult<StateMachineSet<TComponent>> result)
            {
                ref var filter = ref _enterCandidates.GetFilter(indexedDB);
                var groupFilter = filter.GetGroupFilter(result.group);

                // nothing to check
                if (groupFilter.count == 0)
                    return;

                var indices = new IndexedIndices(groupFilter.indices);

                for (int i = 0; i < _onEnter.count; ++i)
                    _onEnter[i].Ready(indexedDB.entitiesDB, result.group);

                foreach (uint index in indices)
                {
                    ref var current = ref result.set.component[index];

                    // this group will not be visited again in this step
                    // updating indexes
                    indexedDB.Update(ref current, result.egid[index], _key);

                    for (int i = 0; i < _onEnter.count; ++i)
                        _onEnter[i].Invoke(index);
                }
            }
        }

        internal sealed class AnyState
        {
            internal readonly StateMachineConfig<TRow, TComponent, TState> _config;
            internal readonly FasterList<TransitionConfig<TState>> _transitions;

            internal AnyState(StateMachineConfig<TRow, TComponent, TState> config)
            {
                _config = config;
                _transitions = new FasterList<TransitionConfig<TState>>();
            }

            internal void Evaluate(IndexedDB indexedDB, in QueryResult<StateMachineSet<TComponent>> result)
            {
                for (int i = 0; i < _transitions.count; ++i)
                    _transitions[i].Ready(indexedDB.entitiesDB, result.group);

                foreach (var index in result.indices)
                {
                    for (int i = 0; i < _transitions.count; ++i)
                    {
                        var transition = _transitions[i];
                        ref var current = ref result.set.component[index];

                        // component is already in this state
                        if (current.key.Equals(transition._next))
                            continue;

                        if (!transition.Evaluate(index))
                            continue;

                        // register to execute transition
                        var currentState = current.key;
                        var nextState = transition._next;

                        indexedDB.Memo(_config._states[currentState]._exitCandidates).Add(result.egid[index]);
                        indexedDB.Memo(_config._states[nextState]._enterCandidates).Add(result.egid[index]);
                        break;
                    }
                }
            }
        }
    }
}