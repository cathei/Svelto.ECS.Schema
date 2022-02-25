using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Definition
{
    public readonly partial struct Tables<T> where T : IEntityDescriptor
    {
        public readonly FasterReadOnlyList<ExclusiveGroupStruct> exclusiveGroups;

        public Tables(FasterList<ExclusiveGroupStruct> exclusiveGroups)
        {
            this.exclusiveGroups = exclusiveGroups;
        }

        public static implicit operator FasterReadOnlyList<ExclusiveGroupStruct>(Tables<T> tables) => tables.exclusiveGroups;
        public static implicit operator LocalFasterReadOnlyList<ExclusiveGroupStruct>(Tables<T> tables) => tables.exclusiveGroups;
    }
}