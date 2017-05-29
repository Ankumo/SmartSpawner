using Rocket.API;

namespace SmartSpawner
{
    public class SmartSpawnerConfiguration : IRocketPluginConfiguration
    {
        public bool isAutosetted;
        public bool isAttentionEnabled;
        public bool clearPlayerItems;

        public double interval;
        public double noticeLatency;

        public void LoadDefaults()
        {
            isAutosetted = false;
            isAttentionEnabled = false;
            clearPlayerItems = false;

            interval = 0.0;
            noticeLatency = 0.0;
        }
    }
}
