namespace Svelto.ECS.Schema.Definition
{
    public sealed class Index<T> : EntitySchemaIndex
        where T : unmanaged, IEntityIndexKey<T>
    {
        public Accessor At(ShardOffset offset)
        {
            return new Accessor(this, offset);
        }

        public IndexQuery<T> Query(in T key)
        {
            return GetQuery(metadata.root, 0, key);
        }

        private IndexQuery<T> GetQuery(SchemaMetadata.PartitionNode parent, int index, in T key)
        {
            var node = parent.indexers[siblingOrder];

            if (node.element != this)
                throw new ECSException("Cannot find correct node. Did you forget to call .At(Offset)?");

            return new IndexQuery<T>(node.indexerStartIndex + index, key);
        }

        internal override IEngine CreateEngine(SchemaContext context)
        {
            return new TableIndexingEngine<T>(context);
        }

        public readonly struct Accessor
        {
            private readonly Index<T> indexer;
            private readonly ShardOffset offset;

            internal Accessor(Index<T> indexer, ShardOffset offset)
            {
                this.indexer = indexer;
                this.offset = offset;
            }

            public IndexQuery<T> Query(in T key)
            {
                return indexer.GetQuery(offset.node, offset.index, key);
            }
        }
    }
}