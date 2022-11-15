using FrameWork;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameUI;

namespace SXDZD
{
    [PluginConfig("TaiwuLevelUp", "熟悉的总督", "0.3")]
    public class TaiwuLevelUpFrontend : TaiwuRemakePlugin
    {
        private Harmony harmony;
        private static Coroutine coroutine;

        public static int level = 1;
        public static int exp = 0;
        public static int totalExp = 200;
        public static int freeMainAttribute = 0;
        public override void Dispose()
        {

        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(TaiwuLevelUpFrontend));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UI_Bottom), "OnEnable")]
        public static void UI_Bottom_OnEnable_Patch(UI_Bottom __instance)
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
                (levelGo.transform as RectTransform).anchoredPosition = (levelGo.transform as RectTransform).anchoredPosition + new Vector2(0, -35);
                (expGo.transform as RectTransform).anchoredPosition = (expGo.transform as RectTransform).anchoredPosition + new Vector2(0, -55);

                Vector2 barPos = (levelGo.transform as RectTransform).anchoredPosition + new Vector2(0, -55);
                ProgressBar progressBar = CreateBar(days.transform.parent, barPos);
            }
            else
            {
                levelGo = levelTrans.gameObject;
                expGo = days.transform.parent.Find("roleLevelTxt").gameObject;
            }


            Debug.Log($"UI_Bottom_OnInit_Patch::levelGo：{levelGo}");
            Debug.Log($"UI_Bottom_OnInit_Patch::expGo：{expGo}");

            if (coroutine != null)
            {
                __instance.StopCoroutine(coroutine);
            }
            coroutine = __instance.StartCoroutine(RefreshExp(__instance));

            //__instance.AsynchMethodCall(90, 0, (offset, rawDataPool) =>
            //{
            //    ShowExpAndLevel(offset, rawDataPool, levelGo, expGo);
            //});

        }

        private static ProgressBar CreateBar(Transform parent, Vector2 pos)
        {
            GameObject barContainer = new GameObject("ExpBar");
            RectTransform rect = barContainer.AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.anchoredPosition = pos;
            rect.localScale = Vector3.one * 10;

            ProgressBar bar = barContainer.AddComponent<ProgressBar>().Init("25", "1");
            bar.BarColor = new Color(0.3f, 0.3f, 1f);
            bar.BarBGColor = Color.white;

            return bar;
        }

        private static IEnumerator RefreshExp(UI_Bottom __instance)
        {
            var wait = new WaitForSeconds(0.5f);
            while (true)
            {
                yield return wait;
                Refers _timeDisk = __instance.CGet<Refers>("TimeDisk");
                GameObject days = _timeDisk.CGet<TextMeshProUGUI>("Days").gameObject;
                Transform levelTrans = days.transform.parent.Find("roleLevelTxt");
                if (levelTrans == null)
                {
                    Debug.Log($"UI_Make_OnNotifyGameData_Patch::尚未生成 roleLevelTxt 组件！");
                    yield break;
                }

                ProgressBar bar = days.transform.parent.Find("ExpBar").GetComponent<ProgressBar>();

                GameObject levelGo = levelTrans.gameObject;
                GameObject expGo = days.transform.parent.Find("roleExpTxt").gameObject;

                __instance.AsynchMethodCall(190, 0, (offset, rawDataPool) =>
                {
                    ShowExpAndLevel(offset, rawDataPool, levelGo, expGo, bar);
                });
            }

        }

        //[HarmonyPostfix, HarmonyPatch(typeof(UI_Bottom), "OnNotifyGameData")]
        //public static void UI_Make_OnNotifyGameData_Patch(UI_Bottom __instance, List<NotificationWrapper> notifications)
        //{
        //    Refers _timeDisk = __instance.CGet<Refers>("TimeDisk");

        //    GameObject days = _timeDisk.CGet<TextMeshProUGUI>("Days").gameObject;
        //    Transform levelTrans = days.transform.parent.Find("roleLevelTxt");
        //    if (levelTrans == null)
        //    {
        //        Debug.Log($"UI_Make_OnNotifyGameData_Patch::尚未生成 roleLevelTxt 组件！");
        //        return;
        //    }
        //    GameObject levelGo = levelTrans.gameObject;
        //    GameObject expGo = days.transform.parent.Find("roleExpTxt").gameObject;
        //    bool has90 = false;
        //    foreach (NotificationWrapper wrapper in notifications)
        //    {
        //        Notification notification = wrapper.Notification;
        //        var rawDataPool = wrapper.DataPool;
        //        Debug.Log($"UI_Make_OnNotifyGameData_Patch::notification.DomainId={notification.DomainId}");

        //        if (notification.DomainId == 90)
        //        {
        //            has90 = true;
        //            int level = 0, exp = 0, totalExp = 0;
        //            int offset = 0;
        //            offset += Serializer.Deserialize(wrapper.DataPool, offset, ref level);
        //            offset += Serializer.Deserialize(rawDataPool, offset, ref exp);
        //            Serializer.Deserialize(rawDataPool, offset, ref totalExp);

        //            levelGo.GetComponent<TextMeshProUGUI>().text = $"等级:{level}";
        //            expGo.GetComponent<TextMeshProUGUI>().text = $"经验：{exp}/{totalExp}";
        //        }
        //    }
        //    if(false == has90)
        //    {
        //        __instance.AsynchMethodCall(90, 0, (offset, rawDataPool) =>
        //        {
        //            ShowExpAndLevel(offset, rawDataPool, levelGo, expGo);
        //        });
        //    }

        //}

        public static void ShowExpAndLevel(int offset, RawDataPool rawDataPool, GameObject levelGo, GameObject expGo, ProgressBar bar)
        {
            //int level = 0, exp = 0, totalExp = 0, freeMainAttribute = 0;
            int newOffset = offset;
            newOffset += Serializer.Deserialize(rawDataPool, offset, ref level);
            newOffset += Serializer.Deserialize(rawDataPool, newOffset, ref exp);
            newOffset += Serializer.Deserialize(rawDataPool, newOffset, ref totalExp);
            newOffset += Serializer.Deserialize(rawDataPool, newOffset, ref freeMainAttribute);

            levelGo.GetComponent<TextMeshProUGUI>().text = $"等级:{level}";
            expGo.GetComponent<TextMeshProUGUI>().text = $"经验：{exp}/{totalExp}";

            float progress = exp / (float)totalExp;
            bar.Progress = progress;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.CombatHelper), "GetMaxTotalNeiliAllocation")]
        public static void CombatHelper_GetMaxTotalNeiliAllocation_Patch(ref short __result)
        {
            short newResult = (short)(__result + TaiwuLevelUpFrontend.level);
            if (newResult > short.MaxValue)
            {
                newResult = short.MaxValue;
            }
            Debug.Log($"CombatHelper_GetMaxTotalNeiliAllocation_Patch::前端内力上限：当前等级：{TaiwuLevelUpFrontend.level}  之前：{__result} 之后： {newResult}");

            __result = (short)Mathf.Min(newResult, 400);

        }


    }
}
