using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public partial class StateMachine<TComponent>
    {
        IStepEngine IEntityStateMachine.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            return config.AddEngines(enginesRoot, indexedDB);
        }
    }

    partial class StateMachineConfig<TRow, TComponent, TState>
    {
        internal class TransitionEngine : IStepEngine
        {
            private readonly IndexedDB _indexedDB;
            private readonly StateMachineConfig<TRow, TComponent, TState> _config;

            public string name { get; } = $"{typeof(TComponent).FullName} StateMachineEngine";

            internal TransitionEngine(IndexedDB indexedDB, StateMachineConfig<TRow, TComponent, TState> config)
            {
                _indexedDB = indexedDB;
                _config = config;
            }

            public void Step()
            {
                _config.Process(_indexedDB);
            }
        }
    }
}
