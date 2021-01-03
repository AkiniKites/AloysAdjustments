using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using Decima.HZD;

namespace AloysAdjustments.Modules.Outfits
{
    public class CharacterModelBuilder
    {
        private static readonly Regex HumanoidMatcher = new Regex("^entities/.*humanoids/.+");

        public async Task GetCharacterModels()
        {
            var files = (await Prefetch.LoadPrefetch()).Keys;

            foreach (var file in files)
            {
                if (!HumanoidMatcher.IsMatch(file))
                    continue;

                Debug.Write(file);
                var pack = await IoC.Archiver.LoadFile(IoC.Settings.GamePath, file);
                var vars = pack.GetTypes<HumanoidBodyVariant>();
                Debug.WriteLine($"\t{vars.Count}");
            }
        }
    }
}
