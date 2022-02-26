using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState>
    {
        /// <summary>
        /// if you want to control when to evaluate state, set this value to false
        /// and remember to call Engine.Step() by yourself!
        /// </summary>
        public virtual bool RunEngineOnEntitySubmission => true;

        IStepEngine IEntityStateMachine.AddEngines(IndexesDB indexesDB)
        {
            // this is required to handle added entity or removal
            indexesDB.enginesRoot.AddEngine(new TableIndexingEngine<TState, Component>(indexesDB));

            // this is required to validate and change state
            var engine = new Engine(this, indexesDB);

            // order would be important here ...
            // for now we don't want to be in-between of IReactAdd and IReactSubmission
            // TODO: after new filter appplied this can move up to ensure initial state
            indexesDB.enginesRoot.AddEngine(engine);

            return engine;
        }

        public sealed class Engine : IQueryingEntitiesEngine, IStepEngine, IReactOnSubmission
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
                // TODO: we probably can cache this with inspecting descriptors
                var groups = entitiesDB.FindGroups<Component>();

                foreach (var ((component, count), group) in entitiesDB.QueryEntities<Component>(groups))
                {
                    for (int i = 0; i < _fsm._states.count; ++i)
                    {
                        _fsm._states.unsafeValues[i].Evaluate(_indexesDB, component, group);
                    }

                    for (int i = 0; i < count; ++i)
                    {
                        // this means transition condition has met
                        if (component[i].nextTransition >= TransitionConfimed)
                        {
                            var transitionID = component[i].nextTransition - TransitionConfimed;

                            var wrapper = new IKeyEquatable<TState>.Wrapper(component[i].State);
                            var nextState = _fsm._states[wrapper]._transitions[transitionID]._next;

                            // this group will not be visited again
                            // see you next step
                            component[i].nextTransition = TransitionAvailable;
                            component[i].Update(_indexesDB, nextState);
                        }
                    }
                }
            }

            public void EntitiesSubmitted()
            {
                if (_fsm.RunEngineOnEntitySubmission)
                    Step();
            }
        }
    }
}
