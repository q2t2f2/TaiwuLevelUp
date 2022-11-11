using GameData.ArchiveData;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.Global;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SXDZD
{
    public class DataChanger
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Common), "SetArchiveId")]
        public static void Common_SetArchiveId_Patch(sbyte archiveId)
        {
            AdaptableLog.Info($"加载当前存档的等级和经验值数据 World_：{Common.GetCurrArchiveId()}");
            DataLocal.Instance.LoadData();
        }


        //[HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetMaxMainAttributes")]
        //public static unsafe void Character_GetMaxMainAttributes_Patch(GameData.Domains.Character.Character __instance, ref MainAttributes __result)
        //{
        //    if(__instance.GetId() != DomainManager.Taiwu.GetTaiwuCharId())
        //    {
        //        return;
        //    }
        //    DataLocal data = DataLocal.Instance;

        //    for (int j = 0; j < 6; j++)
        //    {
        //        AdaptableLog.Info($"更新前属性：{__result.Items[j]}点");
        //        __result.Items[j] += data.ExtraMainAttribute;
        //        AdaptableLog.Info($"GetMaxMainAttributes 新主属性：{__result.Items[j]}点");
        //    }
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetMaxNeili")]
        //public static void Character_GetMaxNeili_Patch(GameData.Domains.Character.Character __instance, ref int __result)
        //{
        //    if (__instance.GetId() != DomainManager.Taiwu.GetTaiwuCharId())
        //    {
        //        return;
        //    }
        //    DataLocal data = DataLocal.Instance;
        //    AdaptableLog.Info($"更新前最大内力：{__result}点");

        //    __result += data.ExtraNeili;
        //    AdaptableLog.Info($"更新升级后的额外内力：{data.ExtraNeili}点  当前最大内力：{__result}");

        //}

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Combat.CombatDomain), "CalcAndAddExp")]
        public static void CombatDomain_CalcAndAddExp_Patch(CombatDomain __instance, DataContext context)
        {
            DataLocal data = DataLocal.Instance;

            CombatResultDisplayData result = __instance.GetCombatResultDisplayData();
            data.AddExp(result.Exp, context);
            AdaptableLog.Info($"获得战斗经验值：{result.Exp}点，当前：{data.Exp}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.Character), "ChangeExp")]
        public static void Character_ChangeExp_Patch(Character __instance, int delta, DataContext context)
        {
            if (__instance.GetId() != DomainManager.Taiwu.GetTaiwuCharId()) return;

            DataLocal data = DataLocal.Instance;
            data.AddExp(delta, context);
            AdaptableLog.Info($"获得经验值：{delta}点，当前：{data.Exp}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.CombatHelper), "GetMaxTotalNeiliAllocation")]
        public static void CombatHelper_GetMaxTotalNeiliAllocation_Patch(ref short __result)
        {
            DataLocal data = DataLocal.Instance;

            short newResult = (short)(__result + data.Level);
            if (newResult > short.MaxValue)
            {
                newResult = short.MaxValue;
            }
            __result = Math.Min(newResult, (short)400);
            AdaptableLog.Info($"CombatHelper_GetMaxTotalNeiliAllocation_Patch::后端内力上限：{newResult}");
        }



        [HarmonyPostfix, HarmonyPatch(typeof(GlobalDomain), "SaveWorld")]
        public static void GlobalDomain_SaveWorld_Patch(CombatDomain __instance)
        {
            DataLocal data = DataLocal.Instance;
            data.SaveData();
        }


        [HarmonyPatch(typeof(GameDataBridge), "ProcessMethodCall")]
        [HarmonyPrefix]
        public static bool CallMethodPatch(Operation operation, RawDataPool argDataPool, DataContext context)
        {
            if (operation.DomainId == 190)//占用90DomainId
            {
                NotificationCollection notificationCollection = (NotificationCollection)AccessTools.Field(typeof(GameDataBridge), "_pendingNotifications").GetValue(context);
                //int level, exp, total,resultOffset;
                //AdaptableLog.Info($"GameDataBridge::ProcessMethodCall:DomainId={operation.DomainId}  MethodId={operation.MethodId}");

                if (operation.MethodId == 0)
                {
                    notificationCollection.Notifications.Add(Notification.CreateMethodReturn(operation.ListenerId, operation.DomainId, operation.MethodId, 0));
                    int curExp = DataLocal.Instance.CurrrentExp;
                    int expNeed = DataLocal.Instance.ExpNeed;
                    int level = DataLocal.Instance.Level;
                    Serializer.Serialize(level, notificationCollection.DataPool);
                    Serializer.Serialize(curExp, notificationCollection.DataPool);
                    Serializer.Serialize(expNeed, notificationCollection.DataPool);
                }
                return false;
            }
            return true;
        }

    }
}
