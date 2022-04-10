using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    /// <summary>
    /// State machine on Svelto ECS
    /// </summary>
    public abstract partial class StateMachine<TComponent> :
            IEntityStateMachine, IStepEngine,
            IWhereQueryable<StateMachine<TComponent>.IStateMachineRow, TComponent>
        where TComponent : unmanaged, IKeyComponent
    {
        internal StateMachineConfigBase<TComponent> config;

        private IStepEngine _engine;

        public interface IStateMachineRow :
            IIndexableRow<TComponent>, IQueryableRow<StateMachineSet<TComponent>> { }

        IIndexDefinition IEntityStateMachine.Index => config._index;

        RefWrapperType IEntityStateMachine.ComponentType => TypeRefWrapper<TComponent>.wrapper;

        string IStepEngine.name => _engine.name;

        public void Step() => _engine.Step();

        internal bool IsConfigured => config != null;

        public StateMachine()
        {
            if (IsConfigured)
                return;

            lock (EntitySchemaLock.Lock)
            {
                if (IsConfigured)
                    return;

                OnConfigure();
            }
        }

        void IEntityStateMachine.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            _engine = config.AddEngines(enginesRoot, indexedDB);
        }

        protected abstract void OnConfigure();

        protected StateMachineBuilder<TRow, TComponent> Configure<TRow>()
            where TRow : class, IStateMachineRow
        {
            if (config != null)
                throw new ECSException("Configure should only called once!");

            return new StateMachineBuilder<TRow, TComponent>(this);
        }

        void IWhereQueryable.Apply<TKey>(ResultSetQueryConfig config, TKey key)
        {
            ((IWhereQueryable)this.config._index).Apply(config, key);
        }
    }
}
