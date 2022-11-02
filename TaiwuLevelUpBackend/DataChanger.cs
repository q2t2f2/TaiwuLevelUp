using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.Global;
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
            short extraAtt = 0;
            for (int i = 0; i < data.Level; i++)
            {
                extraAtt += (short)data.Level;
            }
            for (int j = 0; j < 6; j++)
            {
                __result.Items[j] += extraAtt;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetMaxNeili")]
        public static void Character_GetMaxNeili_Patch(ref int __result)
        {
            DataLocal data = DataLocal.Instance;

            int extraNeili = 0;
            for (int i = 0; i < data.Level; i++)
            {
                extraNeili += ((short)data.Level * 2);
            }
            __result += extraNeili;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData.Domains.Combat.CombatDomain), "CalcEvaluationList")]
        public static void CombatDomain_CalcEvaluationList_Patch(CombatDomain __instance)
        {
            DataLocal data = DataLocal.Instance;

            CombatResultDisplayData result = __instance.GetCombatResultDisplayData();
            data.Exp += result.Exp;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(GlobalDomain), "SaveWorld")]
        public static void GlobalDomain_SaveWorld_Patch(CombatDomain __instance)
        {
            DataLocal data = DataLocal.Instance;
            data.SaveData();
        }
    }
}
