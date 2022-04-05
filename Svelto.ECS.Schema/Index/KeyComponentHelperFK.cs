using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // internal static class KeyComponentHelperFK<TComponent, TReferRow>
    //     where TComponent : unmanaged, IForeignKeyComponent
    //     where TReferRow : class, IEntityRow
    // {
    //     static KeyComponentHelperFK()
    //     {
    //         KeyComponentHelper<TComponent>.Handler = new ForeignKeyHandlerImpl();
    //     }

    //     // just trigger for static constructor
    //     public static void Warmup() { }

    //     // this can help use reuse index backend without affecting other indexers
    //     private static readonly RefWrapperType ForeignKeyType = TypeRefWrapper<ForeignKey<TComponent, TReferRow>>.wrapper;

    //     private class ForeignKeyHandlerImpl : KeyComponentHelper<TComponent>.IComponentHandler
    //     {
    //         public void AddEngines<TRow>(EnginesRoot enginesRoot, IndexedDB indexedDB)
    //             where TRow : class, IQueryableRow<ResultSet<TComponent>>
    //         {
    //             enginesRoot.AddEngine(new ForeignKeyEngine<TComponent, TReferRow>(indexedDB));
    //         }

    //         public void Update(IndexedDB indexedDB, ref TComponent component, in EGID egid)
    //         {
    //             indexedDB.UpdateForeignKeyComponent(ForeignKeyType, egid, component.reference);
    //         }

    //         public void Remove(IndexedDB indexedDB, in EGID egid)
    //         {
    //             indexedDB.RemoveIndexableComponent<ExclusiveGroupStruct>(ForeignKeyType, egid);
    //         }
    //     }
    // }
}
