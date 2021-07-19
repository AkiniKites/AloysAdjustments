using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using Decima;

namespace AloysAdjustments.Utility
{
    public static class HzdCoreUtility
    {
        public static HzdCore GetRefCore(HzdCore core, BaseRef reference)
        {
            if (reference != null)
            {
                if (reference.Type == BaseRef.Types.LocalCoreUUID || reference.Type == BaseRef.Types.UUIDRef)
                    return core;
                if (reference.Type == BaseRef.Types.ExternalCoreUUID || reference.Type == BaseRef.Types.StreamingRef)
                    return IoC.Archiver.LoadGameFile(reference.ExternalFile);
            }

            return null;
        }
        public static async Task<HzdCore> GetRefCoreAsync(HzdCore core, BaseRef reference)
        {
            if (reference != null)
            {
                if (reference.Type == BaseRef.Types.LocalCoreUUID || reference.Type == BaseRef.Types.UUIDRef)
                    return core;
                if (reference.Type == BaseRef.Types.ExternalCoreUUID || reference.Type == BaseRef.Types.StreamingRef)
                    return await IoC.Archiver.LoadGameFileAsync(reference.ExternalFile);
            }

            return await Task.FromResult<HzdCore>(null);
        }
    }
}
