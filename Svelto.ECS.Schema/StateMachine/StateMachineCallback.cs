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

        private NB<TCallback> _target;

        public CallbackConfigNative(CallbackNative<TCallback> callback)
        {
            _callback = callback;
        }

        internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            (_target, _) = entitiesDB.QueryEntities<TCallback>(groupID);
        }

        internal override void Invoke(uint index)
        {
            _callback(ref _target[index]);
        }
    }

    internal sealed class CallbackConfigManaged<TCallback> : CallbackConfig
        where TCallback : struct, IEntityViewComponent
    {
        internal readonly CallbackManaged<TCallback> _callback;

        private MB<TCallback> _target;

        public CallbackConfigManaged(CallbackManaged<TCallback> callback)
        {
            _callback = callback;
        }

        internal override void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            (_target, _) = entitiesDB.QueryEntities<TCallback>(groupID);
        }

        internal override void Invoke(uint index)
        {
            _callback(ref _target[index]);
        }
    }
}