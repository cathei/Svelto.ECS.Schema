using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IJoinProvider
    {
        public FilterContextID IndexerID { get; }

        internal bool IsValidGroup(IndexedDB indexedDB, ExclusiveGroupStruct group);
    }

    // supprots contravariance and covariance for type checking.
    // usage: IJoinProvider<ICharacterRow, IQueryableRow<ResultSet>>
    public interface IJoinProvider<TComponent, in TRow, out TReferRow> : IJoinProvider
    { }
}

namespace Svelto.ECS.Schema
{
    public interface IForeignKey<TComponent, out TReferRow> : IJoinProvider<TComponent, IForeignKeyRow<TComponent>, TReferRow>
        where TComponent : unmanaged, IForeignKeyComponent
        where TReferRow : class, IReferenceableRow<TComponent>
    { }
}
