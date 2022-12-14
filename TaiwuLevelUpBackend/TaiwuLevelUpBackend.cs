using GameData.Domains;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;

namespace SXDZD
{
    [PluginConfig("TaiwuLevelUp", "熟悉的总督", "0.91")]
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
        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(base.ModIdStr, "MainAttributePerLevel", ref DataLocal.MainAttributePerLevel);
            DomainManager.Mod.GetSetting(base.ModIdStr, "FreeMainAttributePerLevel", ref DataLocal.FreeMainAttributePerLevel);
            DomainManager.Mod.GetSetting(base.ModIdStr, "NeiliPerLevel", ref DataLocal.NeiliPerLevel);
            DomainManager.Mod.GetSetting(base.ModIdStr, "ExpRequireStep", ref DataLocal.ExpRequireStep);
            DomainManager.Mod.GetSetting(base.ModIdStr, "ExpRatio", ref DataLocal.ExpRatio);
            DomainManager.Mod.GetSetting(base.ModIdStr, "EnableOverstepNeili", ref DataLocal.EnableOverstepNeili);
            AdaptableLog.Info($"加载Mod配置   DataLocal.ExpRequireStep：{DataLocal.ExpRequireStep}，DataLocal.MainAttributePerLevel:{DataLocal.MainAttributePerLevel}。");
        }
    }
}
