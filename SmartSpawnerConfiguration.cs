using Rocket.API;

namespace SmartSpawner
{
    public class SmartSpawnerConfiguration : IRocketPluginConfiguration
    {
        public bool isAutosetted;
        public bool isAttentionEnabled;
        public bool clearPlayerItems;
        public bool resetMessageEnabled;

        public double interval;
        public double noticeLatency;

        public void LoadDefaults()
        {
            isAutosetted = false;
            isAttentionEnabled = false;
            clearPlayerItems = false;
            resetMessageEnabled = true;

            interval = 0.0;
            noticeLatency = 0.0;
        }
    }
}
