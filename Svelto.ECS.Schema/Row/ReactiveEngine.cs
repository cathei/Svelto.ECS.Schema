using Svelto.ECS;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IReactRowAddAndRemove<in TR, TC> : IEngine
        where TR : IEntityRow<TC>
        where TC : struct, IEntityComponent
    {
        void Add(ref TC entityComponent, IEntityTable<TR> table, uint entityID);
        void Remove(ref TC entityComponent, IEntityTable<TR> table, uint entityID);
    }

    public interface IReactRowSwap<in TR, TC> : IEngine
        where TR : IEntityRow<TC>
        where TC : struct, IEntityComponent
    {
        void MovedTo(ref TC entityComponent, IEntityTable<TR> previousTable, IEntityTable<TR> table, uint entityID);
    }

    public abstract class ReactiveEngineBase<TR, TC>
        where TR : IEntityRow<TC>
        where TC : struct, IEntityComponent
    {
        protected readonly IndexedDB indexedDB;

        internal ReactiveEngineBase(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }
    }
}

namespace Svelto.ECS.Schema
{
    // TODO: Split this into different engines if necessary
    public abstract class ReactToRowEngine<TR, TC> :
            ReactiveEngineBase<TR, TC>,
            IReactRowAddAndRemove<TR, TC>, IReactOnAddAndRemove<TC>,
            IReactRowSwap<TR, TC>, IReactOnSwap<TC>
        where TR : class, IReactiveRow<TC>
        where TC : struct, IEntityComponent
    {
        internal ReactToRowEngine(IndexedDB indexedDB) : base(indexedDB) { }

        public virtual void Add(ref TC component, IEntityTable<TR> table, uint entityID) { }

        public void Add(ref TC component, EGID egid)
        {
            var table = indexedDB.FindTable<TR>(egid.groupID);
            if (table == null)
                return;

            Add(ref component, table, egid.entityID);
        }

        public virtual void Remove(ref TC component, IEntityTable<TR> table, uint entityID) { }

        public void Remove(ref TC component, EGID egid)
        {
            var table = indexedDB.FindTable<TR>(egid.groupID);
            if (table == null)
                return;

            Remove(ref component, table, egid.entityID);
        }

        public virtual void MovedTo(ref TC component, IEntityTable<TR> previousTable, IEntityTable<TR> table, uint entityID) { }

        public void MovedTo(ref TC component, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            var previousTable = indexedDB.FindTable<TR>(previousGroup);
            var table = indexedDB.FindTable<TR>(egid.groupID);

            if (previousTable == null || table == null)
                return;

            MovedTo(ref component, previousTable, table, egid.entityID);
        }
    }
}
