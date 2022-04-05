using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // internal static class ForeignKeyHelper<TComponent>
    // {
    //     internal interface IHandler
    //     {
    //         void Update(IndexedDB indexedDB, ref TComponent component, in EGID egid);
    //         void Remove(IndexedDB indexedDB, in EGID egid);
    //     }

    //     internal static IHandler Handler;
    // }

    // internal class ForeignKeyHelperImpl<TComponent, TReferRow> : ForeignKeyHelper<TComponent>.IHandler
    //     where TComponent : unmanaged, IForeignKeyComponent
    //     where TReferRow : class, IEntityRow
    // {
    //     static ForeignKeyHelperImpl()
    //     {
    //         ForeignKeyHelper<TComponent>.Handler = new ForeignKeyHelperImpl<TComponent, TReferRow>();
    //     }

    //     // just trigger for static constructor
    //     public static void Warmup() { }

    //     // this can help use reuse index backend without affecting other indexers
    //     private static readonly RefWrapperType ForeignKeyType = TypeRefWrapper<ForeignKey<TComponent, TReferRow>>.wrapper;

    //     public void Update(IndexedDB indexedDB, ref TComponent component, in EGID egid)
    //     {
    //         indexedDB.UpdateForeignKeyComponent<TReferRow>(ForeignKeyType, egid, component.reference);
    //     }

    //     public void Remove(IndexedDB indexedDB, in EGID egid)
    //     {
    //         indexedDB.RemoveIndexableComponent<ExclusiveGroupStruct>(ForeignKeyType, egid);
    //     }
    // }
}
