using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexedDB
    {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(IEntityTable table, uint entityID)
            => entitiesDB.GetEntityReference(new EGID(entityID, table.ExclusiveGroup));
    }
}
