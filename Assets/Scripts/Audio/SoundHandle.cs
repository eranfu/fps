namespace Audio
{
    public struct SoundHandle
    {
        private SoundEmitter emitter;
        private int seq;

        public SoundHandle(SoundEmitter emitter)
        {
            this.emitter = emitter;
            seq = emitter?.seqId ?? -1;
        }
    }
}