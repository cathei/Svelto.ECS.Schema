using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState>
    {
        void IEntityStateMachine.AddEngines(EnginesRoot enginesRoot, IndexesDB indexesDB)
        {
            // this is required to handle added or removed entities
            enginesRoot.AddEngine(new TableIndexingEngine<Key, Component>(indexesDB));

            // this is required to validate and change state
            Engine = new TransitionEngine(this, indexesDB);

            enginesRoot.AddEngine(Engine);
        }

        public sealed class TransitionEngine : IQueryingEntitiesEngine, IStepEngine
        {
            private readonly IndexesDB _indexesDB;

            public string name { get; }

            public EntitiesDB entitiesDB { private get; set; }

            public TransitionEngine(StateMachine<TState> fsm, IndexesDB indexesDB)
            {
                name = $"{fsm.GetType().Name}.Engine";

                _indexesDB = indexesDB;
            }

            public void Ready() { }

            public void Step()
            {
                // TODO: we probably can cache this with inspecting table descriptors or get some hints
                var groups = entitiesDB.FindGroups<Component>();

                var states = Config.States.GetValues(out var stateCount);

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
                    Config.AnyState.Evaluate(_indexesDB, component, count, group);

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
