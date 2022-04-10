using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal static class GlobalPrimaryKeyCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }
}

namespace Svelto.ECS.Schema.Definition
{
    /***

Primary Key Component is concept of Index that split Tables
Any Index Component or State Machine Component can be used as Primary Key Component as well. 

Since the groups must stay statically - primary key component have to give you the possible range.

Primary key also supports partial query

    ***/
    public sealed class PrimaryKey<TComponent> :
            IPrimaryKey<IPrimaryKeyRow<TComponent>>,
            IPrimaryKeyProvider<IPrimaryKeyRow<TComponent>>,
            IPrimaryKeyQueryable<IPrimaryKeyRow<TComponent>, TComponent>
        where TComponent : unmanaged, IKeyComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate() or SveltoFilters.GetNewContextID()
        internal readonly int _primaryKeyID = GlobalPrimaryKeyCount.Generate();

        internal Func<TComponent, int> _componentToIndex;
        internal Delegate _keyToIndex;

        public int PrimaryKeyID => _primaryKeyID;
        public ushort PossibleKeyCount { get; internal set; }

        Delegate IPrimaryKeyDefinition.KeyToIndex => _keyToIndex;
        Delegate IPrimaryKeyQueryable<IPrimaryKeyRow<TComponent>, TComponent>.KeyToIndex => _keyToIndex;

        private readonly ThreadLocal<NB<TComponent>> threadStorage = new();

        static PrimaryKey()
        {
            // must register and trigger reflection
            default(TComponent).Warmup<TComponent>();
        }

        void IPrimaryKeyDefinition.Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            (threadStorage.Value, _) = entitiesDB.QueryEntities<TComponent>(groupID);
        }

        int IPrimaryKeyDefinition.QueryGroupIndex(uint index)
        {
            return _componentToIndex(threadStorage.Value[index]);
        }
    }
}
