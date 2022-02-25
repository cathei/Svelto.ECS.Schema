using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{
    internal class IndexesDBEngine : IQueryingEntitiesEngine, IReactOnSubmission
    {
        private readonly IndexesDB _indexesDB;

        public EntitiesDB entitiesDB { private get; set; }

        public IndexesDBEngine(IndexesDB indexesDB)
        {
            _indexesDB = indexesDB;
        }

        public void Ready()
        {
            // this seems like only way to inject entitiesDB...
            _indexesDB.entitiesDB = entitiesDB;
        }

        public void EntitiesSubmitted()
        {
            _indexesDB.ClearMemos();
        }
    }
}
