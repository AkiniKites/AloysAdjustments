using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Logic.Patching;
using AloysAdjustments.Plugins.Misc.Data;
using AloysAdjustments.Utility;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Plugins.Misc
{
    public class MiscLogic
    {
        public async Task<MiscAdjustments> GenerateMiscData(Func<string, Task<HzdCore>> coreGetter)
        {
            var adj = new MiscAdjustments();
            adj.SkipIntroLogos = !(await CheckIntroLogosExist(coreGetter));
            adj.RemoveMenuMusic = !(await CheckMenuMusicExists(coreGetter));

            return adj;
        }

        public void CreatePatch(Patch patch, MiscAdjustments adjustments)
        {
            if (adjustments.SkipIntroLogos == true)
                RemoveIntroLogo(patch);
            if (adjustments.RemoveMenuMusic == true)
                RemoveMenuMusic(patch);
        }
        
        private async Task<bool?> CheckIntroLogosExist(Func<string, Task<HzdCore>> coreGetter)
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

        private async Task<bool?> CheckMenuMusicExists(Func<string, Task<HzdCore>> coreGetter)
        {
            var core = await coreGetter(IoC.Get<MiscConfig>().MenuMusicFile);
            if (core == null) return null;

            return GetMenuMusicParam(core).DefaultObject.Type != BaseRef.Types.Null;
        }
        private void RemoveMenuMusic(Patch patch)
        {
            var core = patch.AddFile(IoC.Get<MiscConfig>().MenuMusicFile);

            var param = GetMenuMusicParam(core);
            param.DefaultObject = new Ref<RTTIRefObject>();

            core.Save();
        }
        private ProgramParameter GetMenuMusicParam(HzdCore core)
        {
            var node = core.GetType<NodeConstantsResource>();
            var param = node?.Parameters.FirstOrDefault();
            if (param == null)
                throw new HzdException($"Unable to find menu music program parameter");
            return param;
        }
    }
}
