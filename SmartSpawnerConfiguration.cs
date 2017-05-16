using Rocket.API;

namespace SmartSpawner
{
    public class SmartSpawnerConfiguration : IRocketPluginConfiguration
    {
        public bool isAutosetted;
        public bool isAttentionEnabled;

        public double interval;
        public double noticeLatency;

        public void LoadDefaults()
        {
            isAutosetted = false;
            isAttentionEnabled = false;

            interval = 0.0;
            noticeLatency = 0.0;
        }
    }
}
