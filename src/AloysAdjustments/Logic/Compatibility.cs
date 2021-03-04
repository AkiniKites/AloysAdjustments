using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Utility;
using Newtonsoft.Json.Linq;

namespace AloysAdjustments.Logic
{
    public class Compatibility
    {
        public static void CleanupOldVersions()
        {
            foreach (var fileName in IoC.Config.OldVersionsToDelete)
            {
                var path = Path.Combine(Configs.GamePackDir, fileName);

                FileBackup.CleanupBackups(path);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        public static bool MigrateSettings(JObject json)
        {
            var oldVersion = (int?)json["Version"] ?? 0;
            var changed = false;
            if (oldVersion < 1)
            {
                var plugins = new JObject();
                json.Add("PluginSettings", plugins);

                plugins.Add("Outfits", new JObject(
                    new JProperty("ApplyToAll", json["ApplyToAllOutfits"]),
                    new JProperty("ModelFilter", json["OutfitModelFilter"])
                ));
                plugins.Add("NPC Models", new JObject(
                    new JProperty("ApplyToAll", json["ApplyToAllNpcs"]),
                    new JProperty("ModelFilter", json["NpcModelFilter"])
                ));

                changed = true;
            }

            return changed;
        }
    }
}
