using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public class Table
    {
        public ExclusiveGroup RangedGroup;

        public int Range;


        // how does primary key calcs?
        // i need step and range per pk for this table
        // so any order I loop through, I can get correct group
        internal struct PrimaryKeyInfo
        {
            internal int id;
            internal int possibleKeyCount;
        }

        internal FasterList<PrimaryKeyInfo> primaryKeys;
    }

    public readonly ref struct TableGroupEnumerable
    {
        // internal readonly TableGroupDict tableGroup;

        internal readonly Table table;
        internal readonly FasterDictionary<int, int> pkIdToValue;

        public ref struct RefIterator
        {
            internal readonly Table table;
            internal readonly FasterDictionary<int, int> pkIdToValue;

            internal int loop;
            internal ExclusiveGroupStruct group;

            public RefIterator(Table table, FasterDictionary<int, int> pkIdToValue) : this()
            {
                this.table = table;
                this.pkIdToValue = pkIdToValue;

                loop = -1;
            }

            public bool MoveNext()
            {
                while (++loop < table.Range)
                {
                    int groupIndex = 0;

                    for (int i = 0; i < table.primaryKeys.count; ++i)
                    {
                        ref var info = ref table.primaryKeys[i];

                        // mutiply parent index
                        groupIndex *= info.possibleKeyCount;

                        if (pkIdToValue.TryGetValue(info.id, out int value))
                        {
                            // give offset
                            groupIndex += value;
                            // one count as looping whole groups for this pk
                            loop += info.possibleKeyCount;
                        }
                    }

                    group = table.RangedGroup + (uint)groupIndex;
                }

                return loop < table.Range;
            }

            public ExclusiveGroupStruct Current => group;
        }
    }
}