using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{

    public interface IReactRowAddAndRemove<TR, T1> : IEngine
        where TR : IEntityRow<T1>
        where T1 : struct, IEntityComponent
    {
        void OnAdd(ref T1 component, IEntityTable<TR> table);
        void OnRemove(ref T1 compoent, IEntityTable<TR> table);
        void OnSwap(ref T1 component, IEntityTable<TR> table);
    }

    public interface IReactRowAddAndRemove<TR, T1, T2> : IEngine
        where TR : IEntityRow<T1, T2>
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {

    }
}