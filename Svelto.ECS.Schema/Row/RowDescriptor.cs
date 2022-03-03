using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public abstract class DescriptorRow<TSelf> : IEntityRow
        where TSelf : DescriptorRow<TSelf>
    {
        public class Descriptor : IDynamicEntityDescriptor
        {
            public IComponentBuilder[] componentsToBuild { get; }

            public Descriptor()
            {
                // reflection time!
                // for all interface the row implements
                var interfaceTypes = typeof(TSelf).GetInterfaces();
                var componentBuilderDict = new FasterDictionary<RefWrapperType, IComponentBuilder>();

                foreach (var interfaceType in interfaceTypes)
                {
                    // we are finding generics only
                    if (!interfaceType.IsGenericType)
                        continue;

                    var genericDefinition = interfaceType.GetGenericTypeDefinition();

                    if (genericDefinition == typeof(IEntityRow<>) ||
                        genericDefinition == typeof(IEntityRow<,>) ||
                        genericDefinition == typeof(IEntityRow<,,>) ||
                        genericDefinition == typeof(IEntityRow<,,,>))
                    {
                        var componentBuildersField = interfaceType.GetField(
                            nameof(IEntityRow<EGIDComponent>.componentBuilders),
                            BindingFlags.Static | BindingFlags.NonPublic);

                        var componentBuilders = (IComponentBuilder[])componentBuildersField.GetValue(null);

                        // prevent duplication with Dictionary
                        foreach (var componentBuilder in componentBuilders)
                        {
                            var wrapper = new RefWrapperType(componentBuilder.GetEntityComponentType());
                            componentBuilderDict[wrapper] = componentBuilder;
                        }
                    }
                }

                componentsToBuild = new IComponentBuilder[componentBuilderDict.count];
                componentBuilderDict.CopyValuesTo(componentsToBuild);
            }
        }

        public sealed class Table : TableBase<TSelf> { }

        public class Tables : TablesBase<TSelf>
        {
            public Tables(int range) : base(GenerateTables(range), false) { }

            private static Table[] GenerateTables(int range)
            {
                var tables = new Table[range];

                for (int i = 0; i < range; ++i)
                    tables[i] = new Table();

                return tables;
            }
        }

        public sealed class Tables<TIndex> : Tables
        {
            internal readonly Func<TIndex, int> _mapper;

            internal Tables(int range, Func<TIndex, int> mapper) : base(range)
            {
                _mapper = mapper;
            }

            internal Tables(TIndex range, Func<TIndex, int> mapper) : base(mapper(range))
            {
                _mapper = mapper;
            }

            public IEntityTable<TSelf> this[TIndex index] => _tables[_mapper(index)];
            public IEntityTable<TSelf> Get(TIndex index) => _tables[_mapper(index)];
        }



    }

    public struct Component1 : IEntityComponent {}
    public struct Component2 : IEntityComponent {}
    public struct Component3 : IEntityComponent {}


    public static class Tester
    {
        // Row Interfaces
        public interface IDamagableRow : IEntityRow<Component1> { }
        public interface IMovableRow : IEntityRow<Component1, Component2, Component3> { }

        // Row Descriptors
        public sealed class CharacterRow : DescriptorRow<CharacterRow>, IDamagableRow, IMovableRow
        {

        }


    //     public interface IEntityTable<out IEntity> {}
    //     // public class CharacterTable : IEntityTable<CharacterEntity> {}
    //     // public class DamagableTable : IEntityTable<IDamagableEntity> {}


        public static void Testing(IndexedDB indexedDB)
        {
            var characters = new CharacterRow.Table();

            IEntityTable<IDamagableRow> damagables = characters;
            IEntityTable<IMovableRow> movables = characters;

            // foreach (var ((position, speed, _, count), group) in indexedDB.Select<IMovableRow>().Entities())
            // {

            // }

            {
                // var ((position, speed, _, count), indices) = indexedDB
                //     .Select<IMovableRow>().From(characters).Which(characterFSM, CharacterState.Happy).Entities();

                // var (position, speed, _, count) = indexedDB.Select<IMovableRow>().From(damagables).Entities();
            }
        }
    }


    // Row => Table restrain possible
    // Is Row => Components possible?


}