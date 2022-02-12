using System;
using Svelto.ECS.Schedulers;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class GroupTests
    {
        uint characterIdCounter = 0;
        uint itemIdCounter = 0;

        [Fact]
        public void Test()
        {
            var submissionScheduler = new SimpleEntitiesSubmissionScheduler();
            var enginesRoot = new EnginesRoot(submissionScheduler);

            var entityFactory = enginesRoot.GenerateEntityFactory();
            var entityFunctions = enginesRoot.GenerateEntityFunctions();
            var schema = enginesRoot.AddSchema<SampleSchema>();

            Random random = new Random();

            for (int i = 0; i < 100; ++i)
            {
                var builder = entityFactory.BuildEntity(characterIdCounter++, schema.AI.Character);
            }

            for (int i = 0; i < 100; ++i)
            {
                var builder = entityFactory.BuildEntity(characterIdCounter++, schema.Player(random.Next(10)).Character);
            }

            for (int j = 0; j < 1000; ++j)
            {
                var itemBuilder = entityFactory.BuildEntity(itemIdCounter++, schema.Player(random.Next(5)).Item);

                var owner = new ItemOwner(random.Next((int)characterIdCounter));
                itemBuilder.Init(new Indexed<ItemOwner>(owner));
            }

            submissionScheduler.SubmitEntities();

            var entitiesDB = ((IUnitTestingInterface)enginesRoot).entitiesForTesting;

            {
                var (egid, count) = entitiesDB.QueryEntities<EGIDComponent>(schema.AI.Character);

                for (int i = 0; i < count; ++i)
                {
                    Console.Log($"{egid[i].ID}");
                }
            }

            PrintItemOwners(schema, "Index");

            var allItemsGroup = schema.AllItems.Build();

            foreach (var ((keys, count), _) in entitiesDB.QueryEntities<Indexed<ItemOwner>>(allItemsGroup))
            {
                for (int i = 0; i < count; ++i)
                {
                    keys[i].Update(schema.Context, new ItemOwner(0));
                }
            }

            PrintItemOwners(schema, "IndexUpdate");

            foreach (var ((keys, count), _) in entitiesDB.QueryEntities<Indexed<ItemOwner>>(allItemsGroup))
            {
                for (int i = 0; i < count; ++i)
                {
                    entityFunctions.SwapEntityGroup(keys[i].ID, schema.Player(9).Item);
                }
            }

            submissionScheduler.SubmitEntities();

            PrintItemOwners(schema, "GroupSwap");
        }

        private void PrintItemOwners(SampleSchema schema, string prefix)
        {
            for (int characterId = 0; characterId < characterIdCounter; ++characterId)
            {
                foreach (var ((keys, indices), _) in schema.Context.QueryEntities<Indexed<ItemOwner>>(schema.ItemsByOwner(characterId)))
                {
                    for (int i = 0; i < indices.Count(); ++i)
                    {
                        Console.Log($"[{prefix}] {characterId} - {keys[indices[i]].ID} - {keys[indices[i]].Key}");
                    }
                }
            }
        }
    }
}
