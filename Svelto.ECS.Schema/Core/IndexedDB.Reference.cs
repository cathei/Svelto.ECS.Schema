using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexedDB
    {
        // EGIDMapper is struct, avoid boxing
        internal EGIDMapper<RowIdentityComponent> GetEGIDMapper(in ExclusiveGroupStruct groupID)
        {
            return entitiesDB.QueryMappedEntities<RowIdentityComponent>(groupID);
        }

        public bool Exists(IEntityTable table, uint entityID)
        {
            return entitiesDB.Exists<RowIdentityComponent>(entityID, table.ExclusiveGroup);
        }

        public bool TryGetEGID<TRow>(EntityReference entityReference, out IEntityTable<TRow> table, out uint entityID)
            where TRow : class, IEntityRow
        {
            if (entitiesDB.TryGetEGID(entityReference, out var egid))
            {
                table = FindTable<TRow>(egid.groupID);
                entityID = egid.entityID;
                return table != null;
            }

            table = null;
            entityID = default;
            return false;
        }

        public bool TryGetEntityIndex(IEntityTable table, uint entityID, out uint entityIndex)
        {
            var mapper = GetEGIDMapper(table.ExclusiveGroup);
            return mapper.FindIndex(entityID, out entityIndex);
        }

        public bool TryGetEntityIndex<TRow>(EntityReference entityReference, out IEntityTable<TRow> table, out uint entityIndex)
            where TRow : class, IEntityRow
        {
            if (!TryGetEGID(entityReference, out table, out var entityID))
            {
                entityIndex = default;
                return false;
            }

            return TryGetEntityIndex(table, entityID, out entityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(IEntityTable table, uint entityID)
            => entitiesDB.GetEntityReference(new EGID(entityID, table.ExclusiveGroup));
    }
}
