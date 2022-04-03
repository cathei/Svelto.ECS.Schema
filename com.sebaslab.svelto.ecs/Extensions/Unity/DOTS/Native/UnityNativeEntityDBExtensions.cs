#if UNITY_NATIVE
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Native
{
    public static class UnityNativeEntityDBExtensions
    {
        internal static NativeEGIDMapper<T> ToNativeEGIDMapper<T>(this TypeSafeDictionary<T> dic,
            ExclusiveGroupStruct groupStructId) where T : unmanaged, IEntityComponent
        {
            var mapper = new NativeEGIDMapper<T>(groupStructId, dic.implUnmgd);

            return mapper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeEGIDMapper<T>  QueryNativeMappedEntities<T>(this EntitiesDB entitiesDb, ExclusiveGroupStruct groupStructId)
            where T : unmanaged, IEntityComponent
        {
            if (entitiesDb.SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(typeof(T), groupStructId.ToName());

            return (typeSafeDictionary as TypeSafeDictionary<T>).ToNativeEGIDMapper(groupStructId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryNativeMappedEntities<T>(this EntitiesDB entitiesDb, ExclusiveGroupStruct groupStructId,
                                                           out NativeEGIDMapper<T> mapper)
            where T : unmanaged, IEntityComponent
        {
            mapper = default;
            if (entitiesDb.SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false ||
                typeSafeDictionary.count == 0)
                return false;

            mapper = (typeSafeDictionary as TypeSafeDictionary<T>).ToNativeEGIDMapper(groupStructId);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeEGIDMultiMapper<T> QueryNativeMappedEntities<T>(this EntitiesDB entitiesDb,
                                                                    LocalFasterReadOnlyList<ExclusiveGroupStruct> groups, Allocator allocator)
            where T : unmanaged, IEntityComponent
        {
            var dictionary = new SveltoDictionary<ExclusiveGroupStruct, //key 
                    SveltoDictionary<uint, T, 
                        NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>, NativeStrategy<int>>, //value 
                        NativeStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>, //strategy to store the key
                    NativeStrategy<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>, NativeStrategy<int>>>, NativeStrategy<int>> //strategy to store the value 
                    ((uint) groups.count, allocator);
        
            foreach (var group in groups)
            {
                if (entitiesDb.SafeQueryEntityDictionary<T>(group, out var typeSafeDictionary) == true)
                    if (typeSafeDictionary.count > 0)
                        dictionary.Add(group, ((TypeSafeDictionary<T>)typeSafeDictionary).implUnmgd);
            }
            
            return new NativeEGIDMultiMapper<T>(dictionary);
        }
    }
}
#endif