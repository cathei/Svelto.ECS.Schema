using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    internal partial class SchemaMetadata
    {
        // GroupHashMap.RegisterGroup(ExclusiveGroupStruct exclusiveGroupStruct, string name)
        private static readonly MethodInfo GroupHashMapRegisterGroup;

        // GroupNamesMap.idToName
        private static readonly Dictionary<ExclusiveGroupStruct, string> GroupNamesMapIdToName;

        // ExclusiveGroup._knownGroups
        private static readonly Dictionary<string, ExclusiveGroupStruct> ExclusiveGroupKnownGroups;

        static SchemaMetadata()
        {
            var groupType = typeof(ExclusiveGroup);
            var groupHashMapType = groupType.Assembly.GetType("Svelto.ECS.GroupHashMap");
            var groupNamesMapType = groupType.Assembly.GetType("GroupNamesMap");

            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

            GroupHashMapRegisterGroup = groupHashMapType.GetMethod("RegisterGroup", bindingFlags);

            GroupNamesMapIdToName = groupNamesMapType.GetField("idToName", bindingFlags)?.GetValue(null)
                as Dictionary<ExclusiveGroupStruct, string>;

            ExclusiveGroupKnownGroups = groupType.GetField("_knownGroups", bindingFlags)?.GetValue(null)
                as Dictionary<string, ExclusiveGroupStruct>;
        }
    }
}
