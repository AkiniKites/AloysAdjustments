using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Modules.Misc
{
    public class MiscLogic
    {
        public async Task<MiscAdjustments> GenerateMiscData()
        {
            //extract game files
            return await GenerateMiscDataFromPath(Configs.GamePackDir, true);
        }

        public async Task<MiscAdjustments> GenerateMiscDataFromPath(string path, bool checkMissing)
        {
            var adj = new MiscAdjustments();
            adj.SkipIntroLogos = !(await GetIntroLogoState(path, checkMissing));

            return adj;
        }

        public async Task CreatePatch(Patch patch, MiscAdjustments adjustments)
        {
            if (adjustments.SkipIntroLogos == true)
                await RemoveIntroLogo(patch);
        }

        
        private async Task<bool?> GetIntroLogoState(string path, bool checkMissing)
        {
            var core = await IoC.Archiver.LoadFileAsync(path, IoC.Get<MiscConfig>().IntroFile, checkMissing);
            if (core == null) return null;

            return GetIntroMenu(core).PropertyAnimations.Any();
        }

        private async Task RemoveIntroLogo(Patch patch)
        {
            var core = await patch.AddFile(IoC.Get<MiscConfig>().IntroFile);

            var menu = GetIntroMenu(core);
            menu.PropertyAnimations.Clear();
            menu.Blendtime = 0;

            await core.Save();
        }

        private MenuAnimationResource GetIntroMenu(HzdCore core)
        {
            var menuName = IoC.Get<MiscConfig>().IntroMenuName;
            var menu = core.GetTypes<MenuAnimationResource>().FirstOrDefault(x => x.Name == menuName);
            if (menu == null)
                throw new HzdException($"Unable to find intro menu with name: {menuName}");
            return menu;
        }
    }
}
