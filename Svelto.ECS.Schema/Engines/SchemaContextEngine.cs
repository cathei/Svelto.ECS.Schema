using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{
    internal class SchemaContextEngine : IQueryingEntitiesEngine
    {
        private readonly IndexesDB _indexesDB;

        public EntitiesDB entitiesDB { private get; set; }

        public SchemaContextEngine(IndexesDB indexesDB)
        {
            _indexesDB = indexesDB;
        }

        public void Ready()
        {
            // this seems like only way to inject entitiesDB...
            _indexesDB.entitiesDB = entitiesDB;
        }
    }
}
