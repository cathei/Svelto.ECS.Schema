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
        internal static StateMachineConfigBase<TComponent> Config;

        protected StateMachine()
        {
            if (Config != null)
                return;

            lock (EntitySchemaLock.Lock)
            {
                if (Config != null)
                    return;

                OnConfigure();
            }

            if (Config == null)
                throw new ECSException("StateMachine is not properly configured!");
        }

        protected abstract void OnConfigure();

        void IEntityStateMachine.OnConfigure() => OnConfigure();

        public interface IIndexableRow : IIndexableRow<TComponent> { }

        int IIndexQueryable<IIndexableRow, TComponent>.IndexerID => Config._index._indexerId;

        IEntityIndex IEntityStateMachine.Index => Config._index;

        protected StateMachineBuilder<TRow, TComponent> Configure<TRow>()
            where TRow : class, IIndexableRow
        {
            if (Config != null)
                throw new ECSException("Configure should only called once!");

            return new StateMachineBuilder<TRow, TComponent>();
        }
    }
}
