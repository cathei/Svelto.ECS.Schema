using System;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TParam, TState>
    {
        public virtual bool RunEngineOnEntitySubmission => false;

        public sealed class Engine : IQueryingEntitiesEngine, IStepEngine, IReactOnSubmission
        {
            private readonly StateMachine<TParam, TState> _fsm;
            private readonly IndexesDB _indexesDB;

            public string name { get; }

            public EntitiesDB entitiesDB { private get; set; }

            public Engine(StateMachine<TParam, TState> fsm, IndexesDB indexesDB)
            {
                name = $"{fsm.GetType().Name}.Engine";

                _fsm = fsm;
                _indexesDB = indexesDB;
            }

            public void Ready() { }

            public void Step()
            {
                var groups = entitiesDB.FindGroups<Component>();

                foreach (var ((component, count), group) in entitiesDB.QueryEntities<Component>(groups))
                {
                    for (int i = 0; i < count; ++i)
                    {
                        // validate transition
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
