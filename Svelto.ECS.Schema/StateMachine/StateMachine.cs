using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    /// <summary>
    /// State machine on Svelto ECS
    /// </summary>
    public abstract partial class StateMachine<TComponent> : IEntityStateMachine,
            IIndexQueryable<StateMachine<TComponent>.IIndexableRow, TComponent>
        where TComponent : unmanaged, IStateMachineComponent
    {
        internal StateMachineConfigBase<TComponent> config;

        public interface IIndexableRow : IIndexableRow<TComponent>, IEntityRow<TComponent> { }

        int IIndexQueryable<IIndexableRow, TComponent>.IndexerID => config._index._indexerID;

        IEntityIndex IEntityStateMachine.Index => config._index;

        RefWrapperType IEntityStateMachine.ComponentType => TypeRefWrapper<TComponent>.wrapper;

        protected StateMachineBuilder<TRow, TComponent> Configure<TRow>()
            where TRow : class, IIndexableRow
        {
            if (config != null)
                throw new ECSException("Configure should only called once!");

            return new StateMachineBuilder<TRow, TComponent>(this);
        }
    }
}
