using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public delegate void CallbackNative<TComponent>(ref TComponent component)
        where TComponent : unmanaged, IEntityComponent;

    public delegate void CallbackManaged<TComponent>(ref TComponent component)
        where TComponent : struct, IEntityViewComponent;

    internal abstract class CallbackConfig
    {
        internal CallbackConfig() { }

        internal abstract void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID);

        internal abstract void Invoke(uint index);
    }

    internal sealed class CallbackConfigNative<TCallback> : CallbackConfig
        where TCallback : unmanaged, IEntityComponent
    {
        internal readonly CallbackNative<TCallback> _callback;

        public CallbackConfigNative(CallbackNative<TCallback> callback)
        {
            _callback = callback;
        }

        private readonly ThreadLocal<NB<TCallback>> threadStorage = new ThreadLocal<NB<TCallback>>();

        internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            (threadStorage.Value, _) = entitiesDB.QueryEntities<TCallback>(groupID);
        }

        internal override void Invoke(uint index)
        {
            _callback(ref threadStorage.Value[index]);
        }
    }

    internal sealed class CallbackConfigManaged<TCallback> : CallbackConfig
        where TCallback : struct, IEntityViewComponent
    {
        internal readonly CallbackManaged<TCallback> _callback;

        public CallbackConfigManaged(CallbackManaged<TCallback> callback)
        {
            _callback = callback;
        }

        private readonly ThreadLocal<MB<TCallback>> threadStorage = new ThreadLocal<MB<TCallback>>();

        internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            (threadStorage.Value, _) = entitiesDB.QueryEntities<TCallback>(groupID);
        }

        internal override void Invoke(uint index)
        {
            _callback(ref threadStorage.Value[index]);
        }
    }
}