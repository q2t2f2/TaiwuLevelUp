using GameData.Domains;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;

namespace SXDZD
{
    [PluginConfig("TaiwuLevelUp", "熟悉的总督", "0.1")]
    public class TaiwuLevelUpBackend : TaiwuRemakePlugin
    {

        private Harmony harmony;


        public override void Dispose()
        {
            harmony.UnpatchSelf();
        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(DataChanger));
        }
    }
}
