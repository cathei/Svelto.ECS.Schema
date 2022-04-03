#if UNITY_NATIVE
using System;
using DBC.ECS;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        NativeEntityRemove ProvideNativeEntityRemoveQueue<T>(string memberName) where T : IEntityDescriptor, new()
        {
            //DBC.ECS.Check.Require(EntityDescriptorTemplate<T>.descriptor.isUnmanaged(), "can't remove entities with not native types");
            //todo: remove operation array and store entity descriptor hash in the return value
            //todo I maybe able to provide a  _nativeSwap.SwapEntity<entityDescriptor>
            _nativeRemoveOperations.Add(new NativeOperationRemove(
                                            EntityDescriptorTemplate<T>.descriptor.componentsToBuild, TypeCache<T>.type
                                          , memberName));

            return new NativeEntityRemove(_nativeRemoveOperationQueue, _nativeRemoveOperations.count - 1);
        }

        NativeEntitySwap ProvideNativeEntitySwapQueue<T>(string memberName) where T : IEntityDescriptor, new()
        {
           // DBC.ECS.Check.Require(EntityDescriptorTemplate<T>.descriptor.isUnmanaged(), "can't swap entities with not native types");
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeSwapOperations.Add(new NativeOperationSwap(EntityDescriptorTemplate<T>.descriptor.componentsToBuild
                                                            , TypeCache<T>.type, memberName));

            return new NativeEntitySwap(_nativeSwapOperationQueue, _nativeSwapOperations.count - 1);
        }

        NativeEntityFactory ProvideNativeEntityFactoryQueue<T>(string memberName) where T : IEntityDescriptor, new()
        {
            DBC.ECS.Check.Require(EntityDescriptorTemplate<T>.descriptor.IsUnmanaged(), "can't build entities with not native types");
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeAddOperations.Add(
                new NativeOperationBuild(EntityDescriptorTemplate<T>.descriptor.componentsToBuild, TypeCache<T>.type, memberName));

            return new NativeEntityFactory(_nativeAddOperationQueue, _nativeAddOperations.count - 1, _entityLocator);
        }

        void FlushNativeOperations(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Remove/Swap Operations"))
            {
                var removeBuffersCount = _nativeRemoveOperationQueue.count;
                //todo, I don't like that this scans all the queues even if they are empty
                for (int i = 0; i < removeBuffersCount; i++)
                {
                    ref var buffer = ref _nativeRemoveOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var                   componentsIndex       = buffer.Dequeue<uint>();
                        var                   entityEGID            = buffer.Dequeue<EGID>();
                        NativeOperationRemove nativeRemoveOperation = _nativeRemoveOperations[componentsIndex];
                        CheckRemoveEntityID(entityEGID, nativeRemoveOperation.entityDescriptorType
                                          , nativeRemoveOperation.caller);
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Remove, entityEGID, entityEGID
                                                     , nativeRemoveOperation.components));
                    }
                }

                var swapBuffersCount = _nativeSwapOperationQueue.count;
                for (int i = 0; i < swapBuffersCount; i++)
                {
                    ref var buffer = ref _nativeSwapOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var entityEGID      = buffer.Dequeue<DoubleEGID>();

                        var componentBuilders = _nativeSwapOperations[componentsIndex].components;

                        CheckRemoveEntityID(entityEGID.@from
                                          , _nativeSwapOperations[componentsIndex].entityDescriptorType
                                          , _nativeSwapOperations[componentsIndex].caller);
                        CheckAddEntityID(entityEGID.to, _nativeSwapOperations[componentsIndex].entityDescriptorType
                                       , _nativeSwapOperations[componentsIndex].caller);

                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Swap, entityEGID.@from, entityEGID.to
                                                     , componentBuilders));
                    }
                }
            }

            using (profiler.Sample("Native Add Operations"))
            {
                var addBuffersCount = _nativeAddOperationQueue.count;
                for (int i = 0; i < addBuffersCount; i++)
                {
                    ref var buffer = ref _nativeAddOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var egid            = buffer.Dequeue<EGID>();
                        var reference       = buffer.Dequeue<EntityReference>();
                        var componentCounts = buffer.Dequeue<uint>();

                        Check.Assert(egid.groupID.isInvalid == false, "invalid group detected, are you using new ExclusiveGroupStruct() instead of new ExclusiveGroup()?");

                        var componentBuilders    = _nativeAddOperations[componentsIndex].components;
#if DEBUG && !PROFILE_SVELTO
                        var entityDescriptorType = _nativeAddOperations[componentsIndex].entityDescriptorType;
                        CheckAddEntityID(egid, entityDescriptorType, _nativeAddOperations[componentsIndex].caller);
#endif

                        _entityLocator.SetReference(reference, egid);
                        var dic = EntityFactory.BuildGroupedEntities(egid, _groupedEntityToAdd, componentBuilders
                                                                   , null
#if DEBUG && !PROFILE_SVELTO
                                                                   , entityDescriptorType
#endif
                                                                     );

                        var init = new EntityInitializer(egid, dic, reference);

                        //only called if Init is called on the initialized (there is something to init)
                        while (componentCounts > 0)
                        {
                            componentCounts--;

                            var typeID = buffer.Dequeue<uint>();

                            IFiller entityBuilder = EntityComponentIDMap.GetTypeFromID(typeID);
                            //after the typeID, I expect the serialized component
                            entityBuilder.FillFromByteArray(init, buffer);
                        }
                    }
                }
            }
        }

        void AllocateNativeOperations()
        {
            _nativeRemoveOperations = new FasterList<NativeOperationRemove>();
            _nativeSwapOperations   = new FasterList<NativeOperationSwap>();
            _nativeAddOperations    = new FasterList<NativeOperationBuild>();
        }

        FasterList<NativeOperationRemove> _nativeRemoveOperations;
        FasterList<NativeOperationSwap>   _nativeSwapOperations;
        FasterList<NativeOperationBuild>  _nativeAddOperations;
        
        //todo: I very likely don't need to create one for each native entity factory, the same can be reused
        readonly AtomicNativeBags _nativeAddOperationQueue;
        readonly AtomicNativeBags _nativeRemoveOperationQueue;
        readonly AtomicNativeBags _nativeSwapOperationQueue;
    }

    readonly struct DoubleEGID
    {
        internal readonly EGID from;
        internal readonly EGID to;

        public DoubleEGID(EGID from1, EGID to1)
        {
            from = from1;
            to   = to1;
        }
    }

    readonly struct NativeOperationBuild
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type                entityDescriptorType;
        internal readonly string              caller;

        public NativeOperationBuild
            (IComponentBuilder[] descriptorComponentsToBuild, Type entityDescriptorType, string caller)
        {
            this.entityDescriptorType = entityDescriptorType;
            components                = descriptorComponentsToBuild;
            this.caller               = caller;
        }
    }

    readonly struct NativeOperationRemove
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type                entityDescriptorType;
        internal readonly string              caller;

        public NativeOperationRemove
            (IComponentBuilder[] descriptorComponentsToRemove, Type entityDescriptorType, string caller)
        {
            this.caller               = caller;
            components                = descriptorComponentsToRemove;
            this.entityDescriptorType = entityDescriptorType;
        }
    }

    readonly struct NativeOperationSwap
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type                entityDescriptorType;
        internal readonly string              caller;

        public NativeOperationSwap
            (IComponentBuilder[] descriptorComponentsToSwap, Type entityDescriptorType, string caller)
        {
            this.caller               = caller;
            components                = descriptorComponentsToSwap;
            this.entityDescriptorType = entityDescriptorType;
        }
    }
}
#endif