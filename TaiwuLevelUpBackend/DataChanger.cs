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
using static GameData.Domains.DomainHelper;

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
            AdaptableLog.Info($"获得战斗历练值：{result.Exp}点，当前：{data.Exp}");
            data.AddExp(result.Exp, context);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.Character), "ChangeExp")]
        public static void Character_ChangeExp_Patch(Character __instance, int delta, DataContext context)
        {
            if (__instance.GetId() != DomainManager.Taiwu.GetTaiwuCharId()) return;
            if (delta < 0) return;
            DataLocal data = DataLocal.Instance;
            AdaptableLog.Info($"获得历练值：{delta}点，当前经验值：{data.Exp}");

            data.AddExp(delta, context);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.CombatHelper), "GetMaxTotalNeiliAllocation")]
        public static void CombatHelper_GetMaxTotalNeiliAllocation_Patch(ref short __result)
        {
            if (!DataLocal.EnableOverstepNeili) return;

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


        //[HarmonyPatch(typeof(GameDataBridge), "ProcessMethodCall")]
        //[HarmonyPrefix]
        //public static bool CallMethodPatch(Operation operation, RawDataPool argDataPool, DataContext context)
        //{
        //    if (operation.DomainId == 190)//占用90DomainId
        //    {
        //        NotificationCollection notificationCollection = (NotificationCollection)AccessTools.Field(typeof(GameDataBridge), "_pendingNotifications").GetValue(context);
        //        //int level, exp, total,resultOffset;
        //        //AdaptableLog.Info($"GameDataBridge::ProcessMethodCall:DomainId={operation.DomainId}  MethodId={operation.MethodId}");

        //        if (operation.MethodId == 0)
        //        {
        //            notificationCollection.Notifications.Add(Notification.CreateMethodReturn(operation.ListenerId, operation.DomainId, operation.MethodId, 0));
        //            int curExp = DataLocal.Instance.CurrrentExp;
        //            int expNeed = DataLocal.Instance.ExpNeed;
        //            int level = DataLocal.Instance.Level;
        //            int freeMainAttribute = DataLocal.Instance.FreeMainAttribute;
        //            Serializer.Serialize(level, notificationCollection.DataPool);
        //            Serializer.Serialize(curExp, notificationCollection.DataPool);
        //            Serializer.Serialize(expNeed, notificationCollection.DataPool);
        //            Serializer.Serialize(freeMainAttribute, notificationCollection.DataPool);
        //        }
        //        return false;
        //    }
        //    return true;
        //}

        //[HarmonyPatch(typeof(GameDataBridge), "ProcessDataModification")]
        //[HarmonyPrefix]
        //public static bool ProcessDataModificationPatch(Operation operation, RawDataPool dataPool, DataContext context)
        //{
        //    if(operation.DomainId == DomainIds.Character && operation.DataId == TaiwuLevelUpMethodAndDataIDs.Data_LevelAndExp)
        //    {
        //        NotificationCollection notificationCollection = (NotificationCollection)AccessTools.Field(typeof(GameDataBridge), "_pendingNotifications").GetValue(context);
        //        DataUid uid = new DataUid(operation.DomainId,operation.DataId,operation.SubId0,operation.SubId1);
        //        notificationCollection.Notifications.Add(Notification.CreateDataModification(uid,))
        //        return false;
        //    }
        //    return true;
        //}

        [HarmonyPatch(typeof(CharacterDomain), "CallMethod")]
        [HarmonyPrefix]
        public static bool CharacterDomainCallMethodPatch(CharacterDomain __instance, ref int __result, Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context)
        {
            if (operation.MethodId == TaiwuLevelUpMethodAndDataIDs.Method_GetLevelAndExp)//占用1723DomainId
            {
                int num = operation.ArgsCount;

                if (num != 0)
                {
                    AdaptableLog.Info($"CharacterDomainCallMethodPatch: 调用方法 Method_GetLevelAndExp 参数个数错误 应为0个参数，当前：{num}个");
                    return true;
                }
               
                List<int> resultInts = new List<int>
                {
                    DataLocal.Instance.Level,
                    DataLocal.Instance.CurrrentExp,
                    DataLocal.Instance.ExpNeed,
                    DataLocal.Instance.FreeMainAttribute
                };

                __result = Serializer.Serialize(resultInts, returnDataPool);
                return false;
            }
            else if (operation.MethodId == TaiwuLevelUpMethodAndDataIDs.Method_AddAttributePoint)//占用1001DomainId
            {
                int num = operation.ArgsCount;
                int offset = operation.ArgsOffset;

                if (num != 1)
                {
                    AdaptableLog.Info($"CharacterDomainCallMethodPatch: 调用方法 Method_AddAttributePoint 参数个数错误 应为1个参数，当前：{num}个");
                    return true;
                }

                int index = 0;
                offset += Serializer.Deserialize(argDataPool, offset, ref index);

                DataLocal.Instance.AddAttribute(index, context);


                List<int> resultInts = new List<int>
                {
                    DataLocal.Instance.Level,
                    DataLocal.Instance.CurrrentExp,
                    DataLocal.Instance.ExpNeed,
                    DataLocal.Instance.FreeMainAttribute
                };

                __result = Serializer.Serialize(resultInts, returnDataPool);
                return false;
            }
            return true;
        }

    }
}
