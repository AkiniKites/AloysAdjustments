using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Data;
using AloysAdjustments.Utility;

namespace AloysAdjustments.Modules
{
    public class UpgradesLogic
    {
        //public async Task<OutfitFile[]> GenerateUpgradeList()
        //{
        //    //TODO: Fix hack to ignore patch files
        //    var patch = Path.Combine(Configs.GamePackDir, IoC.Config.PatchFile);
        //    using var rn = new FileRenamer(patch);

        //    //extract game files
        //    var upgrades = await GenerateUpgradeListFromPath(Configs.GamePackDir);
        //    return upgrades;
        //}

        //public async Task<OutfitFile[]> GenerateUpgradeListFromPath(string path)
        //{
        //    var files = await FileManager.ExtractFiles(IoC.Decima,
        //        IoC.Config.TempPath, path, false, IoC.Config.UpgradeFile);

        //    var maps = files.Select(x => {
        //        var map = new OutfitFile { File = x.Source };

        //        var core = HzdCore.Load(x.Output);
        //        foreach (var item in GetOutfits(core))
        //            map.Outfits.Add(item);

        //        return map;
        //    }).ToArray();
        //    //await SaveOutfitMaps(maps);

        //    await FileManager.Cleanup(IoC.Config.TempPath);

        //    return maps;
        //}
    }
}
