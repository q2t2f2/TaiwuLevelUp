using FrameWork;
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
        public static void UI_Bottom_OnInit_Patch(UI_Bottom __instance)
        {
            Refers _timeDisk = __instance.CGet<Refers>("TimeDisk");// AccessTools.Field(typeof(UI_Bottom), "_timeDisk").GetValue(__instance) as Refers;
            //Debug.Log($"UI_Bottom_OnInit_Patch::__instance._timeDisk：{AccessTools.Field(typeof(UI_Bottom), "_timeDisk").GetValue(__instance)}");
            Debug.Log($"UI_Bottom_OnInit_Patch::_timeDisk：{_timeDisk}");
            GameObject days = _timeDisk.CGet<TextMeshProUGUI>("Days").gameObject;

            Transform levelTrans = days.transform.parent.Find("roleLevelTxt");

            GameObject levelGo, expGo;
            if (levelTrans == null)
            {
                levelGo = GameObject.Instantiate<GameObject>(days, days.transform.parent);
                expGo = GameObject.Instantiate<GameObject>(days, days.transform.parent);
                levelGo.name = "roleLevelTxt";
                expGo.name = "roleExpTxt";
                (levelGo.transform as RectTransform).anchoredPosition = (levelGo.transform as RectTransform).anchoredPosition + new Vector2(0, -30);
                (expGo.transform as RectTransform).anchoredPosition = (expGo.transform as RectTransform).anchoredPosition + new Vector2(0, -60);
            }else
            {
                levelGo = levelTrans.gameObject;
                expGo = days.transform.parent.Find("roleLevelTxt").gameObject;
            }

            
            Debug.Log($"UI_Bottom_OnInit_Patch::levelGo：{levelGo}");
            Debug.Log($"UI_Bottom_OnInit_Patch::expGo：{expGo}");

            __instance.AsynchMethodCall(90, 0, (offset, rawDataPool) =>
            {
                int level = 0, exp = 0, totalExp = 0;
                Serializer.Deserialize(rawDataPool, offset, ref level);
                Serializer.Deserialize(rawDataPool, offset, ref exp);
                Serializer.Deserialize(rawDataPool, offset, ref totalExp);

                levelGo.GetComponent<TextMeshProUGUI>().text = $"Lv:{level}";
                expGo.GetComponent<TextMeshProUGUI>().text = $"{exp}/{totalExp}";

            });
        }



        [HarmonyPostfix, HarmonyPatch(typeof(UI_Bottom), "OnNotifyGameData")]
        public static void UI_Make_OnNotifyGameData_Patch(UI_Bottom __instance)
        {
            Refers _timeDisk = __instance.CGet<Refers>("TimeDisk");

            GameObject days = _timeDisk.CGet<TextMeshProUGUI>("Days").gameObject;
            Transform levelTrans = days.transform.parent.Find("roleLevelTxt");
            if(levelTrans == null)
            {
                Debug.Log($"UI_Make_OnNotifyGameData_Patch::尚未生成 roleLevelTxt 组件！");
                return;
            }
            GameObject levelGo = levelTrans.gameObject;
            GameObject expGo = days.transform.parent.Find("roleExpTxt").gameObject;

            __instance.AsynchMethodCall(90, 0, (offset, rawDataPool) =>
            {
                int level = 0, exp = 0, totalExp = 0;
                int newOffset = offset;
                newOffset += Serializer.Deserialize(rawDataPool, offset, ref level);
                newOffset += Serializer.Deserialize(rawDataPool, newOffset, ref exp);
                Serializer.Deserialize(rawDataPool, newOffset, ref totalExp);

                levelGo.GetComponent<TextMeshProUGUI>().text = $"等级:{level}";
                expGo.GetComponent<TextMeshProUGUI>().text = $"经验：{exp}/{totalExp}";

            });
        }

    }
}
