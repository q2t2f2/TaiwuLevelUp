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
        private Character taiwuChar;


        public override void Dispose()
        {
            harmony.UnpatchSelf();
        }

        public override void Initialize()
        {
            taiwuChar = new Character(DomainManager.Taiwu.GetTaiwuCharId());
            DataLocal.Instance.LoadData();
            harmony = Harmony.CreateAndPatchAll(typeof(DataChanger));
        }
    }
}
