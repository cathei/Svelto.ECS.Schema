using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class IndexQueryEnumerableGenerator : ISourceGenerator
    {
        const string IndexQueryEnumerableTemplate = @"
    public readonly ref struct IndexQueryEnumerable<{1}>
{2}
    {{
        private readonly SchemaContext _context;
        private readonly FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> _dict;

        internal IndexQueryEnumerable(SchemaContext context, FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> dict)
        {{
            _context = context;
            _dict = dict;
        }}

        public RefIterator GetEnumerator() => new RefIterator(_context, _dict);

        public ref struct RefIterator
        {{
            private readonly SchemaContext _context;
            private readonly FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> _dict;

            private EntityCollection<{1}> _collection;
            private int _indexValue;

            internal RefIterator(SchemaContext context, FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> dict) : this()
            {{
                _context = context;
                _dict = dict;
                _indexValue = -1;
            }}

            public bool MoveNext()
            {{
                if (_dict == null)
                    return false;

                while (++_indexValue < _dict.count)
                {{
                    var groupData = _dict.unsafeValues[_indexValue];

                    if (!groupData.group.IsEnabled())
                        continue;

                    EntityCollection<{1}> collection = _context.entitiesDB.QueryEntities<{1}>(groupData.group);

                    if (groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }}

                var moveNext = _indexValue < _dict.count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }}

            public void Reset() {{ _indexValue = -1; }}

            public RefCurrent Current => new RefCurrent(
                _collection, _dict.unsafeValues[_indexValue].filter.filteredIndices, _dict.unsafeValues[_indexValue].group);
        }}

        public readonly ref struct RefCurrent
        {{
            private readonly EntityCollection<{1}> _collection;
            private readonly FilteredIndices _indices;
            private readonly ExclusiveGroupStruct _group;

            public RefCurrent(in EntityCollection<{1}> collection, in FilteredIndices indices, in ExclusiveGroupStruct group)
            {{
                _collection = collection;
                _indices = indices;
                _group = group;
            }}

            public void Deconstruct(out ({0}, FilteredIndices indices) tuple, out ExclusiveGroupStruct group)
            {{
                var ({3}, _) = _collection;

                tuple = ({3}, _indices);
                group = _group;
            }}
        }}
    }}
";

        const string IndexQueryEnumerableWithGroupsTemplate = @"
    public readonly ref struct IndexQueryGroupsEnumerable<{1}>
{2}
    {{
        private readonly SchemaContext _context;
        private readonly FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> _dict;
        private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        internal IndexQueryGroupsEnumerable(SchemaContext context,
            FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> dict,
            in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {{
            _context = context;
            _dict = dict;
            _groups = groups;
        }}

        public RefIterator GetEnumerator() => new RefIterator(_context, _dict, _groups);

        public ref struct RefIterator
        {{
            private readonly SchemaContext _context;
            private readonly FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> _dict;
            private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            private EntityCollection<{1}> _collection;
            private int _indexValue;

            internal RefIterator(SchemaContext context,
                FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> dict,
                in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {{
                _context = context;
                _dict = dict;
                _groups = groups;
                _indexValue = -1;
            }}

            public bool MoveNext()
            {{
                if (_dict == null)
                    return false;

                while (++_indexValue < _groups.count)
                {{
                    if (!_dict.TryGetValue(_groups[_indexValue], out var groupData))
                        continue;

                    var indices = groupData.filter.filteredIndices;

                    if (!groupData.group.IsEnabled() || indices.Count() == 0)
                        continue;

                    EntityCollection<{1}> collection = _context.entitiesDB.QueryEntities<{1}>(groupData.group);

                    if (groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }}

                var moveNext = _indexValue < _groups.count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }}

            public void Reset() {{ _indexValue = -1; }}

            public RefCurrent Current => new RefCurrent(
                _collection, _dict[_groups[_indexValue]].filter.filteredIndices, _groups[_indexValue]);
        }}

        public readonly ref struct RefCurrent
        {{
            private readonly EntityCollection<{1}> _collection;
            private readonly FilteredIndices _indices;
            private readonly ExclusiveGroupStruct _group;

            public RefCurrent(in EntityCollection<{1}> collection, in FilteredIndices indices, in ExclusiveGroupStruct group)
            {{
                _collection = collection;
                _indices = indices;
                _group = group;
            }}

            public void Deconstruct(out ({0}, FilteredIndices indices) tuple, out ExclusiveGroupStruct group)
            {{
                var ({3}, _) = _collection;

                tuple = ({3}, _indices);
                group = _group;
            }}
        }}
    }}
";

        public string GenerateIndexQueryEnumerables(string template)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 1; i <= 4; ++i)
            {
                builder.Append(GenerateIndexQueryEnumerable(template, i));
            }

            return builder.ToString();
        }

        public StringBuilder GenerateIndexQueryEnumerable(string template, int num)
        {
            var nativeBufferTypeList = "NB<T{0}> c{0}".Repeat(", ", num);
            var genericTypeList = "T{0}".Repeat(", ", num);
            var typeConstraintList = "                where T{0} : unmanaged, IEntityComponent".Repeat("\n", num);
            var componentList = "c{0}".Repeat(", ", num);

            var builder = new StringBuilder();
            builder.AppendFormat(template, nativeBufferTypeList, genericTypeList, typeConstraintList, componentList);
            return builder;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            string source = $@" // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{{
{GenerateIndexQueryEnumerables(IndexQueryEnumerableTemplate)}
{GenerateIndexQueryEnumerables(IndexQueryEnumerableWithGroupsTemplate)}
}}";

            context.AddSource("IndexQueryEnumerable.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
