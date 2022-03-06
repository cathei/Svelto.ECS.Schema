using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// Support structured result set
    /// Usage: IDamagableRow : IQueryableRow<(NB<HealthComponent> health, NB<DefenseComponent> defense)>
    /// </summary>
    public interface IQueryableRow<T> where T : struct, ITuple
    {
        internal static IComponentBuilder[] componentBuilders;

        static IQueryableRow()
        {
            // refelect T and pick component only
        }
    }

    // todo readonly
    public ref struct ResultSet<TResult, TRow>
        where TResult : struct
        where TRow : class, IEntityRow
    {
        public TResult result;
        public RangedIndices indices;
        public IEntityTable<TRow> table;
    }

    public ref struct IndexedResultSet<TResult, TRow>
        where TResult : struct
        where TRow : class, IEntityRow
    {
        public TResult result;
        public IndexedIndices indices;
        public IEntityTable<TRow> table;
    }

    public static class Test
    {
        public interface ITupleFactory<TTuple, TRet>
        {
            TRet Return(in TTuple x);
        }

        public struct TupleFactory<TTuple, TRet> : ITupleFactory<TTuple, TRet>
            where TTuple : TRet
        {
            public TTuple Default => default;
            public TRet Return(in TTuple x) => x;
        }

        public static void TestTuple()
        {
            TupleFactory<(int, int), (int x, int y)> factoryImpl;
            ITupleFactory<(int, int), (int x, int y)> factoryInterface = null;

            var tmp = TestExt(factoryInterface, factoryImpl);

            ICov<B> a =null;
            a.CovExt();
        }

        public static TRet TestExt<TRet, TB>(ITupleFactory<(int, int), TRet> x, TB factory)
            where TB : ITupleFactory<(int, int), TRet>
        {
            var tup = (0, 0);
            tup.Item2 = 100;
            return factory.Return(tup);
        }

        public class A : I {}
        public class B:A , I{}
        public interface I {}
        public interface ICov<out I>{}

        public static void CovExt(this ICov<A> tmp)
        {

        }

        public interface IConvertFrom
        {

        }


        public struct ConvertFrom : IConvertFrom
        {
            public static implicit operator ConvertTo(in ConvertFrom x) => new ConvertTo();

        }

        public struct ConvertTo
        {

        }

        public static void TestTupleInference()
        {
            var from = new ConvertFrom();

            // this is illegal
            // from.ExtNoTuple();
            ExtNoTuple(from);

            // this is also illegal
            // (from, 1).ExtTuple();

            // these both legal....
            from.ExtNoTuple2();
            (from, 1).ExtTuple2();
        }

        public static void ExtNoTuple(this in ConvertTo convertTo)
        {

        }

        public static void ExtNoTuple2(this IConvertFrom convertFrom)
        {

        }

        public static void ExtTuple(this in (ConvertTo a, int x) convertTo)
        {

        }

        public static void ExtTuple2(this in (IConvertFrom a, int x) convertTo)
        {

        }
    }


}