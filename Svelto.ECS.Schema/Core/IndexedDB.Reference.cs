using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;
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

        internal NativeEGIDMapper<RowIdentityComponent> GetNativeEGIDMapper(in ExclusiveGroupStruct groupID)
        {
            return entitiesDB.QueryNativeMappedEntities<RowIdentityComponent>(groupID);
        }

        public bool Exists(uint entityID, in ExclusiveGroup groupID)
        {
            return entitiesDB.Exists<RowIdentityComponent>(entityID, groupID);
        }

        public bool TryGetEntityIndex(uint entityID, in ExclusiveGroupStruct groupID, out uint entityIndex)
        {
            var mapper = GetEGIDMapper(groupID);
            return mapper.FindIndex(entityID, out entityIndex);
        }

        public bool TryGetEntityIndex<TRow>(EntityReference entityReference, out uint entityIndex)
            where TRow : class, IEntityRow
        {
            if (!TryGetEGID(entityReference, out var egid))
            {
                entityIndex = default;
                return false;
            }

            return TryGetEntityIndex(egid.entityID, egid.groupID, out entityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEGID(in EntityReference entityReference, out EGID egid)
            => entitiesDB.TryGetEGID(entityReference, out egid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(uint entityID, in ExclusiveGroupStruct groupID)
            => entitiesDB.GetEntityReference(new EGID(entityID, groupID));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(in EGID egid)
            => entitiesDB.GetEntityReference(egid);

        public (NativeEntityIDs, int) QueryEntityIDs(in ExclusiveGroupStruct groupID)
        {
            var (_, entityIDs, count) = entitiesDB.QueryEntities<RowIdentityComponent>(groupID);
            return (entityIDs, count);
        }
    }
}
