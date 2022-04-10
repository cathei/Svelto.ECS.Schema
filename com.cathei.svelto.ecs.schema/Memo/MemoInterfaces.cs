using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IEntityMemo<in TRow>
        where TRow : IEntityRow
    { }

    public interface IEntityMemo : IEntityMemo<IEntityRow>
    { }
}