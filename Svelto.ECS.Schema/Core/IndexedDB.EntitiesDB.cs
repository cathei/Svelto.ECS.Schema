using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// In some cases, IndexedDB should act same as EntitiesDB
    /// </summary>
    public partial class IndexedDB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEGID(EntityReference entityReference, out EGID egid)
            => entitiesDB.TryGetEGID(entityReference, out egid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGID GetEGID(EntityReference entityReference)
            => entitiesDB.GetEGID(entityReference);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnginesRoot.LocatorMap GetEntityLocator()
            => entitiesDB.GetEntityLocator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(EGID egid)
            => entitiesDB.GetEntityReference(egid);
    }
}
