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
        private readonly IndexesDB _indexesDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexesDB.IndexerGroupData> _dict;

        internal IndexQueryEnumerable(IndexesDB indexesDB, FasterDictionary<ExclusiveGroupStruct, IndexesDB.IndexerGroupData> dict)
        {{
            _indexesDB = indexesDB;
            _dict = dict;
        }}

        public RefIterator GetEnumerator() => new RefIterator(_indexesDB, _dict);

        public ref struct RefIterator
        {{
            private readonly IndexesDB _indexesDB;
            private readonly MB<IndexesDB.IndexerGroupData> _dictValues;
            private readonly uint _count;

            private EntityCollection<{1}> _collection;
            private IndexesDB.IndexerGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexesDB indexesDB, FasterDictionary<ExclusiveGroupStruct, IndexesDB.IndexerGroupData> dict) : this()
            {{
                _indexesDB = indexesDB;
                _indexValue = -1;

                if (dict != null)
                    _dictValues = dict.GetValues(out _count);
            }}

            public bool MoveNext()
            {{
                while (++_indexValue < _count)
                {{
                    _groupData = _dictValues[_indexValue];

                    if (!_groupData.group.IsEnabled())
                        continue;

                    EntityCollection<{1}> collection = _indexesDB.entitiesDB.QueryEntities<{1}>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }}

                var moveNext = _indexValue < _count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }}

            public void Reset() {{ _indexValue = -1; }}

            public IndexQueryGroupTuple<{1}> Current => new IndexQueryGroupTuple<{1}>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }}
    }}
";

        const string IndexQueryEnumerableWithGroupsTemplate = @"
    public readonly ref struct IndexQueryGroupsEnumerable<{1}>
{2}
    {{
        private readonly IndexesDB _indexesDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexesDB.IndexerGroupData> _dict;
        private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        internal IndexQueryGroupsEnumerable(IndexesDB indexesDB,
            FasterDictionary<ExclusiveGroupStruct, IndexesDB.IndexerGroupData> dict,
            in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {{
            _indexesDB = indexesDB;
            _dict = dict;
            _groups = groups;
        }}

        public RefIterator GetEnumerator() => new RefIterator(_indexesDB, _dict, _groups);

        public ref struct RefIterator
        {{
            private readonly IndexesDB _indexesDB;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexesDB.IndexerGroupData> _dict;
            private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            private EntityCollection<{1}> _collection;
            private IndexesDB.IndexerGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexesDB indexesDB,
                FasterDictionary<ExclusiveGroupStruct, IndexesDB.IndexerGroupData> dict,
                in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {{
                _indexesDB = indexesDB;
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
                    if (!_dict.TryGetValue(_groups[_indexValue], out _groupData))
                        continue;

                    var indices = _groupData.filter.filteredIndices;

                    if (!_groupData.group.IsEnabled() || indices.Count() == 0)
                        continue;

                    EntityCollection<{1}> collection = _indexesDB.entitiesDB.QueryEntities<{1}>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
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

            public IndexQueryGroupTuple<{1}> Current => new IndexQueryGroupTuple<{1}>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
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
            var genericTypeList = "T{0}".Repeat(", ", num);
            var typeConstraintList = "                where T{0} : struct, IEntityComponent".Repeat("\n", num);

            var builder = new StringBuilder();
            builder.AppendFormat(template, "unused", genericTypeList, typeConstraintList);
            return builder;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            string source = $@" // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

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
