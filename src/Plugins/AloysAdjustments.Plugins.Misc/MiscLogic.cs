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
using Decima.HZD;
using NAudio.Wave;

namespace AloysAdjustments.Plugins.Misc
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
            if (adjustments.RemoveMenuMusic == true)
                RemoveMenuMusic(patch);
        }

        
        private async Task<bool?> GetIntroLogoState(Func<string, Task<HzdCore>> coreGetter)
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

        private void RemoveMenuMusic(Patch patch)
        {
            var core = patch.AddFile("sounds/music/menumusic/musicscape_01/musicscape_01.soundbank");
            
            using var ms = new MemoryStream(); 
            var reader = new WaveFileReader("silence.wav");
            reader.CopyTo(ms);

            var byteArray = new Array<byte>(ms.ToArray());

            //ms.Position = 0;
            //patch.AddFile("sounds/music/menumusic/musicscape_01/musicscape_01.soundbank");

            var wav = core.GetType<WaveResource>();
            wav.IsStreaming = false;
            wav.WaveData = byteArray;
            wav.WaveDataSize = (uint)byteArray.Count;
            wav.SampleCount = (int)reader.SampleCount;
            wav.StreamInfo = null;

            core.Save();
        }
    }
}
