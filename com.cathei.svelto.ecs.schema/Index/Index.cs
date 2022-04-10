using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Index<TComponent> : IIndexDefinition, IEntityIndex<TComponent>
        where TComponent : unmanaged, IKeyComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly FilterContextID _indexerID = EntitiesDB.SveltoFilters.GetNewContextID();

        RefWrapperType IIndexDefinition.ComponentType => TypeRefWrapper<TComponent>.wrapper;

        FilterContextID IIndexDefinition.IndexerID => _indexerID;

        static Index()
        {
            // must register and trigger reflection
            default(TComponent).Warmup<TComponent>();
        }

        public Index() { }

        void IIndexDefinition.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            enginesRoot.AddEngine(new TableIndexingEngine<IIndexableRow<TComponent>, TComponent>(indexedDB));
        }

        // be aware of key type when using this method
        void IWhereQueryable.Apply<TKey>(ResultSetQueryConfig config, TKey key)
        {
            config.filters.Add(GetFilter(config.indexedDB, key));
        }

        // be aware of key type when using this method
        internal ref EntityFilterCollection GetFilter<TKey>(IndexedDB indexedDB, TKey key)
            where TKey : unmanaged, IEquatable<TKey>
        {
            return ref indexedDB.GetFilter(_indexerID, key);
        }
    }
}
