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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SXDZD
{
    public class DataChanger
    {
        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.Character), "CalcMaxMainAttributes")]
        public static unsafe void Character_CalcMaxMainAttributes_Patch(ref MainAttributes __result)
        {
            DataLocal data = DataLocal.Instance;

            AdaptableLog.Info($"更新升级后的额外属性：{data.ExtraMainAttribute}点");
            for (int j = 0; j < 6; j++)
            {
                AdaptableLog.Info($"更新前属性：{__result.Items[j]}点");
                __result.Items[j] += data.ExtraMainAttribute;
                AdaptableLog.Info($"更新后属性：{__result.Items[j]}点");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetMaxNeili")]
        public static void Character_GetMaxNeili_Patch(ref int __result)
        {
            DataLocal data = DataLocal.Instance;
            AdaptableLog.Info($"更新升级后的额外内力：{data.ExtraNeili}点");
            
            __result += data.ExtraNeili;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Combat.CombatDomain), "CalcEvaluationList")]
        public static void CombatDomain_CalcEvaluationList_Patch(CombatDomain __instance)
        {
            DataLocal data = DataLocal.Instance;

            CombatResultDisplayData result = __instance.GetCombatResultDisplayData();
            data.Exp += result.Exp;
            AdaptableLog.Info($"获得经验值：{result.Exp}点，当前：{data.Exp}");
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
            if (operation.DomainId == 90)//占用90DomainId
            {
                NotificationCollection notificationCollection = (NotificationCollection)AccessTools.Field(typeof(GameDataBridge), "_pendingNotifications").GetValue(context);
                //int level, exp, total,resultOffset;
                AdaptableLog.Info($"GameDataBridge::ProcessMethodCall:DomainId={operation.DomainId}  MethodId={operation.MethodId}");

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
