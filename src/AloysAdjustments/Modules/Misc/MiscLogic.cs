using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Data;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Modules.Misc
{
    public class MiscLogic
    {
        public async Task<MiscAdjustments> GenerateMiscData(Func<string, Task<HzdCore>> coreGetter)
        {
            var adj = new MiscAdjustments();
            adj.SkipIntroLogos = !(await GetIntroLogoState(coreGetter));

            return adj;
        }

        public void CreatePatch(Patch patch, MiscAdjustments adjustments)
        {
            if (adjustments.SkipIntroLogos == true)
                RemoveIntroLogo(patch);
        }

        
        private async Task<bool?> GetIntroLogoState(
            Func<string, Task<HzdCore>> coreGetter)
        {
            var core = await coreGetter(IoC.Get<MiscConfig>().IntroFile);
            if (core == null) return null;

            return GetIntroMenu(core).PropertyAnimations.Any();
        }

        private void RemoveIntroLogo(Patch patch)
        {
            var core = patch.AddFile(IoC.Get<MiscConfig>().IntroFile);

            var menu = GetIntroMenu(core);
            menu.PropertyAnimations.Clear();
            menu.Blendtime = 0;

            core.Save();
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
