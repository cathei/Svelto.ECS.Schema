using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    partial class StateMachineConfig<TRow, TComponent, TState>
    {
        internal sealed class State
        {
            internal readonly StateMachineConfig<TRow, TComponent, TState> _config;
            internal readonly TState _key;
            internal readonly FasterList<TransitionConfig<TState>> _transitions;

            internal readonly Memo _exitCandidates;
            internal readonly Memo _enterCandidates;

            internal readonly FasterList<CallbackConfig> _onExit;
            internal readonly FasterList<CallbackConfig> _onEnter;

            internal sealed class Memo : MemoBase<TRow, TComponent> { }

            internal State(StateMachineConfig<TRow, TComponent, TState> config, in TState state)
            {
                _config = config;
                _key = state;
                _transitions = new FasterList<TransitionConfig<TState>>();

                _exitCandidates = new Memo();
                _enterCandidates = new Memo();

                _onExit = new FasterList<CallbackConfig>();
                _onEnter = new FasterList<CallbackConfig>();
            }

            internal void Evaluate(IndexedDB indexedDB, in IndexedQueryResult<IndexableResultSet<TComponent>> result)
            {
                var keyData = _config._index.Is(_key).GetIndexerKeyData(indexedDB);

                if (!keyData.groups.TryGetValue(result.group, out var groupData))
                    return;

                var indices = new IndexedIndices(groupData.filter.filteredIndices);

                // nothing to check
                if (indices.Count() == 0)
                    return;

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
                        ref var egid = ref result.set.egid[index];

                        // register to execute transition
                        var currentState = current.key;
                        var nextState = transition._next;

                        indexedDB.Memo(_config._states[currentState]._exitCandidates).Add(egid.ID.entityID, result.group);
                        indexedDB.Memo(_config._states[nextState]._enterCandidates).Add(egid.ID.entityID, result.group);
                        break;
                    }
                }
            }

            internal void ProcessExit(IndexedDB indexedDB, in IndexedQueryResult<IndexableResultSet<TComponent>> result)
            {
                if (_onExit.count == 0)
                    return;

                var keyData = _exitCandidates.GetIndexerKeyData(indexedDB);

                if (!keyData.groups.TryGetValue(result.group, out var groupData))
                    return;

                var indices = new IndexedIndices(groupData.filter.filteredIndices);

                if (indices.Count() == 0)
                    return;

                for (int i = 0; i < _onExit.count; ++i)
                    _onExit[i].Ready(indexedDB.entitiesDB, result.group);

                foreach (uint index in indices)
                {
                    for (int i = 0; i < _onExit.count; ++i)
                        _onExit[i].Invoke(index);
                }
            }

            internal void ProcessEnter(IndexedDB indexedDB, in IndexedQueryResult<IndexableResultSet<TComponent>> result)
            {
                var keyData = _enterCandidates.GetIndexerKeyData(indexedDB);

                if (!keyData.groups.TryGetValue(result.group, out var groupData))
                    return;

                var indices = new IndexedIndices(groupData.filter.filteredIndices);

                if (indices.Count() == 0)
                    return;

                for (int i = 0; i < _onEnter.count; ++i)
                    _onEnter[i].Ready(indexedDB.entitiesDB, result.group);

                foreach (uint index in indices)
                {
                    ref var current = ref result.set.component[index];
                    ref var egid = ref result.set.egid[index];

                    // this group will not be visited again in this step
                    // updating indexes
                    indexedDB.Update(ref current, egid.ID, _key);

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

            internal void Evaluate(IndexedDB indexedDB, in IndexedQueryResult<IndexableResultSet<TComponent>> result)
            {
                for (int i = 0; i < _transitions.count; ++i)
                    _transitions[i].Ready(indexedDB.entitiesDB, result.group);

                foreach (var index in result.indices)
                {
                    for (int i = 0; i < _transitions.count; ++i)
                    {
                        var transition = _transitions[i];
                        ref var current = ref result.set.component[index];
                        ref var egid = ref result.set.egid[index];

                        // component is already in this state
                        if (current.key.Equals(transition._next))
                            continue;

                        if (!transition.Evaluate(index))
                            continue;

                        // register to execute transition
                        var currentState = current.key;
                        var nextState = transition._next;

                        indexedDB.Memo(_config._states[currentState]._exitCandidates).Add(egid.ID.entityID, result.group);
                        indexedDB.Memo(_config._states[nextState]._enterCandidates).Add(egid.ID.entityID, result.group);
                        break;
                    }
                }
            }
        }
    }
}