using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState>
    {
        IStepEngine IEntityStateMachine.AddEngines(EnginesRoot enginesRoot, IndexesDB indexesDB)
        {
            // this is required to handle added entity or removal
            enginesRoot.AddEngine(new TableIndexingEngine<Key, Component>(indexesDB));

            // this is required to validate and change state
            var engine = new Engine(this, indexesDB);

            enginesRoot.AddEngine(engine);

            return engine;
        }

        public sealed class Engine : IQueryingEntitiesEngine, IStepEngine
        {
            private readonly StateMachine<TState> _fsm;
            private readonly IndexesDB _indexesDB;

            public string name { get; }

            public EntitiesDB entitiesDB { private get; set; }

            public Engine(StateMachine<TState> fsm, IndexesDB indexesDB)
            {
                name = $"{fsm.GetType().Name}.Engine";

                _fsm = fsm;
                _indexesDB = indexesDB;
            }

            public void Ready() { }

            public void Step()
            {
                // TODO: we probably can cache this with inspecting table descriptors or get some hints
                var groups = entitiesDB.FindGroups<Component>();

                var states = _fsm._states.GetValues(out var stateCount);

                // clear all filters before proceed
                // maybe not needed with new filter system
                for (int i = 0; i < stateCount; ++i)
                {
                    states[i]._exitCandidates.Clear(_indexesDB);
                    states[i]._enterCandidates.Clear(_indexesDB);
                }

                foreach (var ((component, count), group) in entitiesDB.QueryEntities<Component>(groups))
                {
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].Evaluate(_indexesDB, component, group);
                    }

                    // any state transition has lower priority
                    _fsm._anyState.Evaluate(_indexesDB, component, count, group);

                    // check for exit candidates
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].ProcessExit(_indexesDB, group);
                    }

                    // check for enter candidates
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].ProcessEnter(_indexesDB, component, group);
                    }
                }
            }
        }
    }
}
