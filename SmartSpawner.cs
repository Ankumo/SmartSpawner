using Rocket.API;
using Rocket.Core.Plugins;
using UnityEngine;
using Rocket.Core.Commands;
using SDG.Unturned;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using System;
using Rocket.API.Collections;

namespace SmartSpawner
{
    public class SmartSpawner : RocketPlugin<SmartSpawnerConfiguration>
    {
        public SmartSpawner Instance;

        public double spawnChance = Provider.modeConfigData.Items.Spawn_Chance;

        private DateTime lastRespawn;

        private bool noticedAlready;

        protected override void Load()
        {
            Instance = this;

            lastRespawn = DateTime.Now;

            Configuration.Load();
        }

        public void Update()
        {
            if (!Configuration.Instance.isAutosetted)
            {
                return;
            }

            if (DateTime.Now >= lastRespawn + TimeSpan.FromSeconds(Configuration.Instance.interval))
            {
                lastRespawn = DateTime.Now;

                RefreshItems();

                noticedAlready = false;

                UnturnedChat.Say(Translate("items_have_been_respawned"), Color.cyan);
            }

            if (!Configuration.Instance.isAttentionEnabled)
            {
                return;
            }

            if (noticedAlready)
            {
                return;
            }

            if (DateTime.Now >= lastRespawn + (TimeSpan.FromSeconds(Configuration.Instance.interval) - TimeSpan.FromSeconds(Configuration.Instance.noticeLatency)))
            {
                UnturnedChat.Say(Translate("items_going_to_respawn", Configuration.Instance.noticeLatency), Color.cyan);

                noticedAlready = true;
            }
        }

        const string ssHelp = "Launches SmartSpawner manually or automatically.";
        const string ssSyntax = @"<manual|auto (interval in sec|off) [attention countdown]>";
        [RocketCommand("ss", ssHelp, ssSyntax, AllowedCaller.Console)]
        public void ExecuteSmartSpawner(string[] command)
        {
            if (command.Length < 1)
            {
                print(ssHelp);
                return;
            }

            switch (command[0])
            {
                case "manual":
                    RefreshItems();

                    lastRespawn = DateTime.Now;

                    UnturnedChat.Say(Translate("items_have_been_respawned"), Color.cyan);
                    break;
                case "auto":
                    if (command.Length > 3)
                    {
                        print(ssSyntax);
                        return;
                    }

                    if (command[1] == "off")
                    {
                        Configuration.Instance.LoadDefaults();
                        Configuration.Save();

                        print("Automatic SmartSpawner has been successfully disabled.");
                        return;
                    }

                    double currentInterval = 0.0, currentNoticeLatency = 0.0;

                    if (Double.TryParse(command[1], out currentInterval))
                    {
                        if (currentInterval < 60.0 || currentInterval > 60.0 * 60.0 * 24.0 * 30.0)
                        {
                            print("Respawn interval must be in values between 60 and " + (60 * 60 * 24 * 30).ToString() + "!");
                            return;
                        }

                        Configuration.Instance.LoadDefaults();

                        lastRespawn = DateTime.Now;

                        Configuration.Instance.isAutosetted = true;
                        Configuration.Instance.interval = currentInterval;
                        Configuration.Save();
                    }
                    else
                    {
                        print(ssSyntax);
                        return;
                    }

                    if (command.Length == 3)
                    {
                        if (Double.TryParse(command[2], out currentNoticeLatency))
                        {
                            if (currentNoticeLatency < currentInterval)
                            {
                                noticedAlready = false;

                                Configuration.Instance.isAttentionEnabled = true;
                                Configuration.Instance.noticeLatency = currentNoticeLatency;
                                Configuration.Save();

                                print("Automatic SmartSpawner has been successfully enabled with " + currentInterval.ToString() + " sec interval.");
                                print("Players will be noticed within " + currentNoticeLatency.ToString() + " seconds.");
                                return;
                            }
                            else
                            {
                                Configuration.Instance.LoadDefaults();
                                Configuration.Save();

                                print("Respawn interval value can't be higher than notice latency.");
                                return;
                            }
                        }
                        else
                        {
                            print(ssSyntax);
                            return;
                        }
                    }

                    print("Automatic SmartSpawner has been successfully enabled with " + currentInterval.ToString() + " sec interval.");
                    break;
                default:
                    print(ssSyntax);
                    return;
            }
        }

        private void RefreshItems()
        {
            for (byte x = 0; x < Regions.WORLD_SIZE; x++)
            {
                for (byte y = 0; y < Regions.WORLD_SIZE; y++)
                {
                    ItemRegion currentItemRegion = ItemManager.regions[x, y];
                    List<ItemData> droppedItems = new List<ItemData>();

                    if (!Configuration.Instance.clearPlayerItems)
                    {                                        
                        if (currentItemRegion.items.Count > 0)
                        {
                            for (int i = 0; i < currentItemRegion.items.Count; i++)
                            {
                                if (currentItemRegion.items[i].isDropped)
                                {
                                    droppedItems.Add(currentItemRegion.items[i]);
                                }
                            }
                        }                       
                    }

                    List<int> currentRegionSpawnsIndexes = new List<int>();

                    for (int i = 0; i < LevelItems.spawns[x, y].Count; i++)
                    {
                        currentRegionSpawnsIndexes.Add(i);
                    }

                    ItemManager.askClearRegionItems(x, y);

                    long itemsSpawnedCount = 0;

                    if (!Configuration.Instance.clearPlayerItems)
                    {
                        for (var i = 0; i < droppedItems.Count; i++)
                        {
                            ItemManager.dropItem(droppedItems[i].item, droppedItems[i].point, false, true, false);
                        }
                    }

                    if (currentRegionSpawnsIndexes.Count <= 0)
                    {
                        continue;
                    }

                    while (currentRegionSpawnsIndexes.Count > 0 && (double)LevelItems.spawns[(int)x, (int)y].Count * spawnChance > (double)itemsSpawnedCount)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, currentRegionSpawnsIndexes.Count);

                        ItemSpawnpoint randomSpawn = LevelItems.spawns[x, y][currentRegionSpawnsIndexes[randomIndex]];

                        currentRegionSpawnsIndexes.RemoveAt(randomIndex);

                        ushort itemId = LevelItems.getItem(randomSpawn);

                        if (itemId == 0)
                        {
                            continue;
                        }

                        var newItem = new Item(itemId, false);

                        ItemManager.dropItem(newItem, randomSpawn.point, false, false, false);

                        itemsSpawnedCount++;
                    }
                                   
                }
            }
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"items_have_been_respawned", "All items have been respawned just now."},
                    {"items_going_to_respawn", "All items will be respawned within {0} seconds."}
                };
            }
        }

    }

}
