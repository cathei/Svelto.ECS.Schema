namespace Svelto.ECS.Schedulers
{
    public abstract class EntitiesSubmissionScheduler
    {
        protected internal abstract EnginesRoot.EntitiesSubmitter onTick { set; }

        public abstract void Dispose();

        public bool paused    { get; set; }
        public uint iteration { get; protected internal set; }

        internal bool isRunning;
    }
}