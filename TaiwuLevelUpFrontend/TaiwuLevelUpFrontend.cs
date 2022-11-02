using GameData.GameDataBridge;
using GameData.Serializer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;
using TMPro;
using UnityEngine;

namespace SXDZD
{
    [PluginConfig("TaiwuLevelUp", "熟悉的总督", "0.1")]
    public class TaiwuLevelUpFrontend : TaiwuRemakePlugin
    {
        private Harmony harmony;
        public override void Dispose()
        {

        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(TaiwuLevelUpFrontend));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UI_Bottom), "OnInit")]
        public static void UI_MakeOnInit_Patch(UI_Bottom __instance, Refers ___timeDisk)
        {
            GameObject days = ___timeDisk.CGet<TextMeshProUGUI>("Days").gameObject;
            GameObject levelGo = GameObject.Instantiate<GameObject>(days);
            (levelGo.transform as RectTransform).anchoredPosition = (levelGo.transform as RectTransform).anchoredPosition + new Vector2(0, 10);

            __instance.AsynchMethodCall(90, 0, (i, rawDataPool) =>
            {
                int item = 0;
                Serializer.Deserialize(rawDataPool,i,ref item)
                levelGo.GetComponent<TextMeshProUGUI>().text = level;
            });
        }


    }
}
