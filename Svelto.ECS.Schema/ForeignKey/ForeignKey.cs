using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// Foreign key is used to mimic Join operation
    /// Foreign key backend is special filter to map groups so join work
    /// </summary>
    public sealed class ForeignKey<TComponent, TReferRow>
        where TComponent : unmanaged, IForeignKeyComponent
        where TReferRow : class, IEntityRow
    {
        static ForeignKey()
        {
            KeyComponentHelperFK<TComponent, TReferRow>.Warmup();
        }

        public sealed class Index : IndexBase<IForeignKeyRow<TComponent>, TComponent>
        { }
    }
}
