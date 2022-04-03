#if UNITY_JOBS
using Svelto.Common;
using Svelto.DataStructures;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// Note unsorted jobs run in parallel
    /// </summary>
    /// <typeparam name="Interface"></typeparam>
    public abstract class UnsortedJobifiedEnginesGroup<Interface> : IJobifiedEngine
        where Interface : class, IJobifiedEngine
    {
        protected UnsortedJobifiedEnginesGroup(FasterList<Interface> engines)
        {
            _name    = "JobifiedEnginesGroup - " + this.GetType().Name;
            _engines = engines;
        }

        protected UnsortedJobifiedEnginesGroup()
        {
            _name    = "JobifiedEnginesGroup - " + this.GetType().Name;
            _engines = new FasterList<Interface>();
        }

        public JobHandle Execute(JobHandle inputHandles)
        {
            var       engines         = _engines;
            JobHandle combinedHandles = inputHandles;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < engines.count; index++)
                {
                    ref var engine = ref engines[index];
                    using (profiler.Sample(engine.name))
                    {
                        combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(inputHandles));
                    }
                }
            }

            return combinedHandles;
        }

        public void Add(Interface engine)
        {
            _engines.Add(engine);
        }

        public string name => _name;

        protected readonly FasterList<Interface> _engines;
        readonly           string                _name;
    }

    public abstract class UnsortedJobifiedEnginesGroup<Interface, Param> : IJobifiedGroupEngine<Param>
        where Interface : class, IJobifiedEngine<Param>
    {
        protected UnsortedJobifiedEnginesGroup(FasterList<Interface> engines)
        {
            _name    = "JobifiedEnginesGroup - " + this.GetType().Name;
            _engines = engines;
        }

        public JobHandle Execute(JobHandle combinedHandles, ref Param _param)
        {
            var engines = _engines;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < engines.count; index++)
                {
                    var engine = engines[index];
                    using (profiler.Sample(engine.name))
                        combinedHandles = JobHandle.CombineDependencies(combinedHandles,
                            engine.Execute(combinedHandles, ref _param));
                }
            }

            return combinedHandles;
        }

        public string name => _name;

        readonly string _name;

        readonly FasterReadOnlyList<Interface> _engines;
    }
}
#endif