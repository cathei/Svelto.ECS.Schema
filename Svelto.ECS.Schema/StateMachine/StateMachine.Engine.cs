using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TTag, TState>
    {
        void IEntityStateMachine.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            // this is required to handle added or removed entities
            enginesRoot.AddEngine(new TableIndexingEngine<TState, Component>(indexedDB));

            // this is required to validate and change state
            Engine = new TransitionEngine(indexedDB);

            enginesRoot.AddEngine(Engine);
        }

        public sealed class TransitionEngine : IQueryingEntitiesEngine, IStepEngine
        {
            private readonly IndexedDB _indexedDB;

            public string name { get; } = $"{typeof(TTag).FullName}.TransitionEngine";

            public EntitiesDB entitiesDB { private get; set; }

            internal TransitionEngine(IndexedDB indexedDB)
            {
                _indexedDB = indexedDB;
            }

            public void Ready() { }

            public void Step()
            {
                var groups = entitiesDB.FindGroups<Component>();
                var states = Config.States.GetValues(out var stateCount);

                // clear all filters before proceed
                // maybe not needed with new filter system
                for (int i = 0; i < stateCount; ++i)
                {
                    states[i]._exitCandidates.Clear(_indexedDB);
                    states[i]._enterCandidates.Clear(_indexedDB);
                }

                foreach (var ((component, count), group) in entitiesDB.QueryEntities<Component>(groups))
                {
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].Evaluate(_indexedDB, component, group);
                    }

                    // any state transition has lower priority
                    Config.AnyState.Evaluate(_indexedDB, component, count, group);

                    // check for exit candidates
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].ProcessExit(_indexedDB, group);
                    }

                    // check for enter candidates
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].ProcessEnter(_indexedDB, component, group);
                    }
                }
            }
        }
    }
}
