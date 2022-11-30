using FrameWork;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityGameUI;
using static GameData.Domains.DomainHelper;
using static System.Net.Mime.MediaTypeNames;
using Text = UnityEngine.UI.Text;

namespace SXDZD
{
    [PluginConfig("TaiwuLevelUp", "熟悉的总督", "0.91")]
    public class TaiwuLevelUpFrontend : TaiwuRemakePlugin
    {
        /// <summary>
        /// 是否突破精纯上限
        /// </summary>
        public static bool EnableOverstepNeili = true;

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
        public override void OnModSettingUpdate()
        {
            ModManager.GetSetting(base.ModIdStr, "EnableOverstepNeili", ref EnableOverstepNeili);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UI_Bottom), "OnEnable")]
        public static void UI_Bottom_OnEnable_Patch(UI_Bottom __instance)
        {
            Refers _timeDisk = __instance.CGet<Refers>("TimeDisk");// AccessTools.Field(typeof(UI_Bottom), "_timeDisk").GetValue(__instance) as Refers;
            //Debug.Log($"UI_Bottom_OnInit_Patch::__instance._timeDisk：{AccessTools.Field(typeof(UI_Bottom), "_timeDisk").GetValue(__instance)}");
            //Debug.Log($"UI_Bottom_OnInit_Patch::_timeDisk：{_timeDisk}");
            GameObject days = _timeDisk.CGet<TextMeshProUGUI>("Days").gameObject;

            Transform levelTrans = days.transform.parent.Find("roleLevelTxt");

            GameObject levelGo, expGo;
            ProgressBar bar;
            if (levelTrans == null)
            {
                levelGo = GameObject.Instantiate<GameObject>(days, days.transform.parent);
                expGo = GameObject.Instantiate<GameObject>(days, days.transform.parent);


                levelGo.name = "roleLevelTxt";
                expGo.name = "roleExpTxt";
                (levelGo.transform as RectTransform).anchoredPosition = (levelGo.transform as RectTransform).anchoredPosition + new Vector2(0, -35);
                (expGo.transform as RectTransform).anchoredPosition = (expGo.transform as RectTransform).anchoredPosition + new Vector2(0, -55);

                Vector2 barPos = (levelGo.transform as RectTransform).anchoredPosition + new Vector2(0, -55);
                bar = CreateBar(days.transform.parent, barPos);
            }
            else
            {
                levelGo = levelTrans.gameObject;
                expGo = days.transform.parent.Find("roleLevelTxt").gameObject;
                bar = days.transform.parent.Find("ExpBar").GetComponent<ProgressBar>();

            }


            //Debug.Log($"UI_Bottom_OnInit_Patch::levelGo：{levelGo}");
            //Debug.Log($"UI_Bottom_OnInit_Patch::expGo：{expGo}");

            if (coroutine != null)
            {
                __instance.StopCoroutine(coroutine);
            }
            coroutine = __instance.StartCoroutine(RefreshExp(__instance));

            __instance.AsynchMethodCall(DomainIds.Character, TaiwuLevelUpMethodAndDataIDs.Method_GetLevelAndExp, (offset, rawDataPool) =>
            {
                ShowExpAndLevel(offset, rawDataPool, levelGo, expGo, bar);
            });

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
            var wait = new WaitForSeconds(0.3f);
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

                __instance.AsynchMethodCall(DomainIds.Character, TaiwuLevelUpMethodAndDataIDs.Method_GetLevelAndExp, (offset, rawDataPool) =>
                {
                    ShowExpAndLevel(offset, rawDataPool, levelGo, expGo, bar);
                });
            }

        }

        public static void ShowExpAndLevel(int offset, RawDataPool rawDataPool, GameObject levelGo, GameObject expGo, ProgressBar bar)
        {
            //int level = 0, exp = 0, totalExp = 0, freeMainAttribute = 0;

            List<int> datas = new List<int>();
            Serializer.Deserialize(rawDataPool, offset, ref datas);
            if (datas.Count == 4)
            {
                level = datas[0];
                exp = datas[1];
                totalExp = datas[2];
                freeMainAttribute = datas[3];
            }
            else
            {
                Debug.Log($"收到的数据参数个数不正确,应为4,现在为{datas.Count}");
                return;
            }
            level = datas[0];
            exp = datas[1];
            totalExp = datas[2];
            freeMainAttribute = datas[3];

            if (levelGo != null)
            {
                levelGo.GetComponent<TextMeshProUGUI>().text = $"等级:{level}";
                expGo.GetComponent<TextMeshProUGUI>().text = $"经验：{exp}/{totalExp}";

                float progress = exp / (float)totalExp;
                bar.Progress = progress;
            }
        }


        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.CombatHelper), "GetMaxTotalNeiliAllocation")]
        public static void CombatHelper_GetMaxTotalNeiliAllocation_Patch(ref short __result)
        {
            if (!EnableOverstepNeili) return;

            short newResult = (short)(__result + TaiwuLevelUpFrontend.level);
            if (newResult > short.MaxValue)
            {
                newResult = short.MaxValue;
            }
            Debug.Log($"CombatHelper_GetMaxTotalNeiliAllocation_Patch::前端内力上限：当前等级：{TaiwuLevelUpFrontend.level}  之前：{__result} 之后： {newResult}");

            __result = (short)Mathf.Min(newResult, 400);

        }


        #region 加点功能
        [HarmonyPostfix, HarmonyPatch(typeof(UI_CharacterMenu), "OnInit")]
        public static void UI_CharacterMenu_OnInit_Patch(UI_CharacterMenu __instance)
        {
            Refers attributeRefers = __instance.CharacterAttributeView.CGet<Refers>("TabAttribute");
            RectTransform mainAttributeHolder = attributeRefers.CGet<RectTransform>("MainAttributeHolder");

            RectTransform pointTrans = mainAttributeHolder.Find("PointTxt") as RectTransform;
            GameObject pointGo;
            Transform[] attRectTrans = new Transform[6];
            if (pointTrans == null)
            {
                pointGo = UIControls.CreateText(new UIControls.Resources());
                pointGo.transform.SetParent(mainAttributeHolder);
                pointGo.name = "PointTxt";
                pointTrans = pointGo.GetComponent<RectTransform>();
                pointTrans.localScale = Vector3.one;
                pointGo.AddComponent<LayoutElement>().ignoreLayout = true;
                pointGo.GetComponent<Text>().supportRichText = true;
                pointGo.GetComponent<Text>().resizeTextForBestFit = true;
                pointTrans.anchoredPosition = new Vector2(-120, 157);
                pointGo.GetComponent<Text>().fontSize = 25;
                pointGo.GetComponent<Text>().color = Color.white;

                for (int i = 0; i < 6; i++)
                {
                    Transform trans = mainAttributeHolder.GetChild(i);
                    int index = i;
                    attRectTrans[i] = CreateButton(trans.gameObject, () =>
                    {
                        __instance.AsynchMethodCall(DomainIds.Character, TaiwuLevelUpMethodAndDataIDs.Method_AddAttributePoint, index, (offset, rawDataPool) =>
                        {
                            RefreshFreePoints(__instance, offset, rawDataPool, pointGo, attRectTrans);
                        });
                    });
                    attRectTrans[i].name = "AddPointBtn";
                }

            }
            else
            {
                pointGo = mainAttributeHolder.Find("PointTxt").gameObject;

                for (int i = 0; i < 6; i++)
                {
                    Transform trans = mainAttributeHolder.GetChild(i);
                    int index = i;
                    attRectTrans[i] = trans.Find("AddPointBtn");
                }
            }

            RefreshFreePoints(__instance, 0, null, pointGo, attRectTrans);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UI_CharacterMenu), "OnCharacterItemRender")]
        public static void UI_CharacterMenu_OnCharacterItemRender(UI_CharacterMenu __instance)
        {
            Refers attributeRefers = __instance.CharacterAttributeView.CGet<Refers>("TabAttribute");
            RectTransform mainAttributeHolder = attributeRefers.CGet<RectTransform>("MainAttributeHolder");
            GameObject pointGo;
            Transform[] attRectTrans = new Transform[6];

            pointGo = mainAttributeHolder.Find("PointTxt").gameObject;

            for (int i = 0; i < 6; i++)
            {
                Transform trans = mainAttributeHolder.GetChild(i);
                int index = i;
                attRectTrans[i] = trans.Find("AddPointBtn");
            }

            RefreshFreePoints(__instance, 0, null, pointGo, attRectTrans);
        }

        private static void RefreshFreePoints(UI_CharacterMenu __instance, int offset, RawDataPool rawDataPool, GameObject pointGo, Transform[] attRectTrans)
        {
            if (rawDataPool != null)
            {
                List<int> datas = new List<int>();
                Serializer.Deserialize(rawDataPool, offset, ref datas);
                if(datas.Count == 4)
                {
                    level = datas[0];
                    exp = datas[1];
                    totalExp = datas[2];
                    freeMainAttribute = datas[3];
                }else
                {
                    return;
                }
                
            }
            bool isTaiwu = IsTaiwu(__instance.CurCharacterId);
            pointGo.GetComponent<Text>().text = $"可分配点数： <color=#FF5500><size=30><b>{freeMainAttribute}</b></size></color>";
            pointGo.SetActive(isTaiwu);
            for (int i = 0; i < attRectTrans.Length; i++)
            {
                attRectTrans[i].gameObject.SetActive(isTaiwu && freeMainAttribute > 0);
            }
        }
        private static bool IsTaiwu(int characterId)
        {
            return characterId == SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
        }


        private static Transform CreateButton(GameObject parent, UnityAction onClick)
        {
            var btn = UIControls.createUIButton(parent, "#FF9933FF", "+", onClick);
            btn.transform.localScale = Vector3.one;
            (btn.transform as RectTransform).sizeDelta = new Vector2(25, 25);
            btn.transform.Find("Text").GetComponent<Text>().fontSize = 22;
            return btn.transform;
        }
        #endregion

    }
}
