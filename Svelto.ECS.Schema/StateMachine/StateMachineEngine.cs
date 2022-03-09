using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public partial class StateMachine<TComponent>
    {
        void IEntityStateMachine.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            Engine = Config.AddEngines(enginesRoot, indexedDB);
        }
    }

    partial class StateMachineConfig<TRow, TComponent, TState>
    {
        internal class TransitionEngine : IStepEngine
        {
            private readonly IndexedDB _indexedDB;

            public string name { get; } = $"{typeof(TComponent).FullName} StateMachineEngine";

            internal TransitionEngine(IndexedDB indexedDB)
            {
                _indexedDB = indexedDB;
            }

            public void Step()
            {
                Default.Process(_indexedDB);
            }
        }
    }
}
