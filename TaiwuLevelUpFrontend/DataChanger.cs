//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SXDZD
//{
//    internal class DataChanger
//    {
//    }
//}
// GMFunc
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Config;
using FrameWork;
using GameData.Common;
using GameData.Domains.Building;
using GameData.Domains.Character;
using GameData.Domains.Character.Ai;
using GameData.Domains.Character.Display;
using GameData.Domains.Character.Relation;
using GameData.Domains.CombatSkill;
using GameData.Domains.Item.Display;
using GameData.Domains.Map;
using GameData.Domains.World;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using GM;
using UnityEngine;

public static class GMFunc
{
    private static List<(string name, (int type, short id))> _queryCache;

    public static IEnumerator AdvanceMonthCoroutine = null;

    public static bool DisableAutoSaving = false;

    private static bool _lockTime;

    private static bool _teleportMove;

    private static bool _isFinalBossDefeated = false;

    private static bool _lastWorldFunctionsStatusesLoaded = false;

    private static ulong _lastWorldFunctionsStatuses = 0uL;

    public static sbyte OvercomeCombatResultType = -1;

    public static sbyte OvercomeLifeSkillCombatResultType = -1;

    private static int _combatSkillCharId = -1;

    private static int _lifeSkillCharId = -1;

    [GMProperty(EGMGroup.CharacterEvent, 0.25f, 0.25f, 0, EWidgetType.Auto)]
    public static bool IgnoreEventBehavior
    {
        get
        {
            return EventModel.IgnoreEventBehavior;
        }
        set
        {
            EventModel.IgnoreEventBehavior = value;
            if (UIElement.EventWindow.Exist)
            {
                UIElement.EventWindow.UiBaseAs<UI_EventWindow>().Refresh();
            }
        }
    }

    [GMProperty(EGMGroup.MapBase, 0.25f, 0.25f, 0, EWidgetType.Auto)]
    public static bool LockTime
    {
        get
        {
            return _lockTime;
        }
        set
        {
            _lockTime = value;
            GameDataBridge.AddMethodCall(-1, 2, 0, _lockTime);
            if (!_lockTime)
            {
                TeleportMove = false;
            }
        }
    }

    [GMProperty(EGMGroup.MapBase, 0.25f, 0.25f, 0, EWidgetType.Auto)]
    public static bool TeleportMove
    {
        get
        {
            return _teleportMove;
        }
        set
        {
            _teleportMove = value;
            GameDataBridge.AddMethodCall(-1, 2, 1, _teleportMove);
            if (_teleportMove)
            {
                LockTime = true;
            }
        }
    }

    [GMProperty(EGMGroup.MapBase, 0.25f, 0.25f, 0, EWidgetType.Auto)]
    public static bool SkipMainStoryLine
    {
        get
        {
            return PlayerPrefs.GetInt("SkipMainStoryLine") == 1;
        }
        set
        {
            PlayerPrefs.SetInt("SkipMainStoryLine", value ? 1 : 0);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool IsFinalBossDefeated
    {
        get
        {
            if (!_lastWorldFunctionsStatusesLoaded)
            {
                return false;
            }
            return _isFinalBossDefeated;
        }
        set
        {
            if (Game.Instance.GetCurrentGameStateName() == EGameState.InGame && !UI_GMWindow.Instance.IsGameDataReceiving())
            {
                GameDataBridge.AddDataModification(1, 6, ulong.MaxValue, uint.MaxValue, value);
                UI_GMWindow.Instance.SwitchWindow();
            }
        }
    }

    [GMProperty(EGMGroup.CharacterBase, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool EnableRandomGenealogyConnection { get; set; }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool LocalMonthlyNotice
    {
        get
        {
            return _EditWorldFunctionsStatus(0);
        }
        set
        {
            _EditWorldFunctionsStatus(0, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool GlobalMonthlyNotice
    {
        get
        {
            return _EditWorldFunctionsStatus(1);
        }
        set
        {
            _EditWorldFunctionsStatus(1, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool MiniMapViewing
    {
        get
        {
            return _EditWorldFunctionsStatus(2);
        }
        set
        {
            _EditWorldFunctionsStatus(2, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool IntraStateTravel
    {
        get
        {
            return _EditWorldFunctionsStatus(3);
        }
        set
        {
            _EditWorldFunctionsStatus(3, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool InterStateTravel
    {
        get
        {
            return _EditWorldFunctionsStatus(4);
        }
        set
        {
            _EditWorldFunctionsStatus(4, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool WorldResourceCollection
    {
        get
        {
            return _EditWorldFunctionsStatus(5);
        }
        set
        {
            _EditWorldFunctionsStatus(5, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool LocationMarking
    {
        get
        {
            return _EditWorldFunctionsStatus(6);
        }
        set
        {
            _EditWorldFunctionsStatus(6, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool HereticStrongholdGenerating
    {
        get
        {
            return _EditWorldFunctionsStatus(7);
        }
        set
        {
            _EditWorldFunctionsStatus(7, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool RighteousStrongholdGenerating
    {
        get
        {
            return _EditWorldFunctionsStatus(8);
        }
        set
        {
            _EditWorldFunctionsStatus(8, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool CaravanDisplay
    {
        get
        {
            return _EditWorldFunctionsStatus(9);
        }
        set
        {
            _EditWorldFunctionsStatus(9, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool TaiwuVillageManagement
    {
        get
        {
            return _EditWorldFunctionsStatus(10);
        }
        set
        {
            _EditWorldFunctionsStatus(10, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool Chicken
    {
        get
        {
            return _EditWorldFunctionsStatus(11);
        }
        set
        {
            _EditWorldFunctionsStatus(11, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool Crafting
    {
        get
        {
            return _EditWorldFunctionsStatus(12);
        }
        set
        {
            _EditWorldFunctionsStatus(12, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool InfluenceInformation
    {
        get
        {
            return _EditWorldFunctionsStatus(13);
        }
        set
        {
            _EditWorldFunctionsStatus(13, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool SpiritualDebtAction
    {
        get
        {
            return _EditWorldFunctionsStatus(14);
        }
        set
        {
            _EditWorldFunctionsStatus(14, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool SkillLearning
    {
        get
        {
            return _EditWorldFunctionsStatus(15);
        }
        set
        {
            _EditWorldFunctionsStatus(15, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool SkillBookExchange
    {
        get
        {
            return _EditWorldFunctionsStatus(16);
        }
        set
        {
            _EditWorldFunctionsStatus(16, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool CombatSkillBreakOut
    {
        get
        {
            return _EditWorldFunctionsStatus(17);
        }
        set
        {
            _EditWorldFunctionsStatus(17, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool Aspiration
    {
        get
        {
            return _EditWorldFunctionsStatus(18);
        }
        set
        {
            _EditWorldFunctionsStatus(18, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool Kidnap
    {
        get
        {
            return _EditWorldFunctionsStatus(19);
        }
        set
        {
            _EditWorldFunctionsStatus(19, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool Information
    {
        get
        {
            return _EditWorldFunctionsStatus(20);
        }
        set
        {
            _EditWorldFunctionsStatus(20, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool LegendaryBook
    {
        get
        {
            return _EditWorldFunctionsStatus(21);
        }
        set
        {
            _EditWorldFunctionsStatus(21, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool WesternRegionMerchant
    {
        get
        {
            return _EditWorldFunctionsStatus(22);
        }
        set
        {
            _EditWorldFunctionsStatus(22, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool TeaCaravan
    {
        get
        {
            return _EditWorldFunctionsStatus(23);
        }
        set
        {
            _EditWorldFunctionsStatus(23, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool JuniorXiangshuSummoning
    {
        get
        {
            return _EditWorldFunctionsStatus(24);
        }
        set
        {
            _EditWorldFunctionsStatus(24, value);
        }
    }

    [GMProperty(EGMGroup.MapWorldFunction, 0.5f, 0.25f, 0, EWidgetType.Auto)]
    public static bool MartialArtContest
    {
        get
        {
            return _EditWorldFunctionsStatus(25);
        }
        set
        {
            _EditWorldFunctionsStatus(25, value);
        }
    }

    [GMProperty(EGMGroup.CombatSkill, 0.25f, 0.25f, 0, EWidgetType.CharIdField)]
    public static int CombatSkillCharId
    {
        get
        {
            return _combatSkillCharId;
        }
        set
        {
            _combatSkillCharId = value;
        }
    }

    [GMProperty(EGMGroup.LifeSkill, 0.25f, 0.25f, 0, EWidgetType.CharIdField)]
    public static int LifeSkillCharId
    {
        get
        {
            return _lifeSkillCharId;
        }
        set
        {
            _lifeSkillCharId = value;
        }
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    public static void EditCharacterInfo(int charId)
    {
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 10, charId, delegate (int offset, RawDataPool dataPool)
        {
            NameAndLifeRelatedData item = default(NameAndLifeRelatedData);
            Serializer.Deserialize(dataPool, offset, ref item);
            if (item.LifeState == 0)
            {
                UI_GMWindow.Instance.CharacterEditor.GetComponent<GMCharacterEditor>().SetCharacterId(charId);
                UI_GMWindow.Instance.CharacterEditor.gameObject.SetActive(value: true);
            }
        });
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    public static void EnterCharacterMenu(int charId)
    {
        ArgumentBox argBox = EasyPool.Get<ArgumentBox>();
        argBox.Set("CharacterId", charId);
        UIElement.CharacterMenu.SetOnInitArgs(argBox);
        UIManager.Instance.ShowUI(UIElement.CharacterMenu);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.LifeSkillIdField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    public unsafe static void SetCharacterLifeSkillFullLearned(int charId, int templateId, int modifiedQualification)
    {
        DataUid uid = new DataUid(4, 0, (ulong)charId, 29u);
        DataUid uid2 = new DataUid(4, 0, (ulong)charId, 30u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            List<GameData.Domains.Character.LifeSkillItem> list = (List<GameData.Domains.Character.LifeSkillItem>)data[uid];
            LifeSkillShorts value = (LifeSkillShorts)data[uid2];
            if (templateId < 0)
            {
                for (short num = 0; num < LifeSkill.Instance.Count; num = (short)(num + 1))
                {
                    GameData.Domains.Character.LifeSkillItem lifeSkillItem = new GameData.Domains.Character.LifeSkillItem(num, 5);
                    Config.LifeSkillItem config2 = LifeSkill.Instance.GetItem(lifeSkillItem.SkillTemplateId);
                    int num2 = list.FindIndex((GameData.Domains.Character.LifeSkillItem skill) => skill.SkillTemplateId == config2.TemplateId);
                    if (num2 >= 0)
                    {
                        list[num2] = lifeSkillItem;
                    }
                    else
                    {
                        list.Add(lifeSkillItem);
                    }
                    value.Items[config2.Type] = (short)modifiedQualification;
                }
            }
            else
            {
                GameData.Domains.Character.LifeSkillItem lifeSkillItem2 = new GameData.Domains.Character.LifeSkillItem((short)templateId, 5);
                Config.LifeSkillItem config = LifeSkill.Instance.GetItem(lifeSkillItem2.SkillTemplateId);
                int num3 = list.FindIndex((GameData.Domains.Character.LifeSkillItem skill) => skill.SkillTemplateId == config.TemplateId);
                if (num3 >= 0)
                {
                    list[num3] = lifeSkillItem2;
                }
                else
                {
                    list.Add(lifeSkillItem2);
                }
                value.Items[config.Type] = (short)modifiedQualification;
            }
            GameDataBridge.AddMethodCall(-1, 4, 18, charId, list);
            if (modifiedQualification > 0)
            {
                GameDataBridge.AddDataModification(4, 0, (ulong)charId, 30u, value);
            }
        }, (uid, typeof(List<GameData.Domains.Character.LifeSkillItem>)), (uid2, typeof(LifeSkillShorts)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.CombatSkillIdField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    public unsafe static void SetCharacterCombatSkillFullLearned(int charId, int templateId, int modifiedQualification)
    {
        DataUid uid = new DataUid(4, 0, (ulong)charId, 60u);
        DataUid uid2 = new DataUid(4, 0, (ulong)charId, 32u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            List<short> list = (List<short>)data[uid];
            CombatSkillShorts value = (CombatSkillShorts)data[uid2];
            if (templateId < 0)
            {
                for (short num = 0; num < CombatSkill.Instance.Count; num = (short)(num + 1))
                {
                    CombatSkillItem config2 = CombatSkill.Instance.GetItem(num);
                    int num2 = list.FindIndex((short skillId) => skillId == config2.TemplateId);
                    if (num2 >= 0)
                    {
                        GameDataBridge.AddDataModification(7, 0, (ulong)new CombatSkillKey(charId, config2.TemplateId), 2u, ushort.MaxValue);
                        GameDataBridge.AddDataModification((ushort)7, (ushort)0, (ulong)new CombatSkillKey(charId, config2.TemplateId), 1u, (sbyte)100);
                    }
                    else
                    {
                        GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)93, charId, config2.TemplateId, (sbyte)100, ushort.MaxValue);
                        list.Add(config2.TemplateId);
                    }
                    value.Items[config2.Type] = (short)modifiedQualification;
                }
            }
            else
            {
                CombatSkillItem config = CombatSkill.Instance.GetItem((short)templateId);
                int num3 = list.FindIndex((short skillId) => skillId == config.TemplateId);
                if (num3 >= 0)
                {
                    GameDataBridge.AddDataModification(7, 0, (ulong)new CombatSkillKey(charId, config.TemplateId), 2u, ushort.MaxValue);
                    GameDataBridge.AddDataModification((ushort)7, (ushort)0, (ulong)new CombatSkillKey(charId, config.TemplateId), 1u, (sbyte)100);
                }
                else
                {
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)93, charId, config.TemplateId, (sbyte)100, ushort.MaxValue);
                    list.Add(config.TemplateId);
                }
                value.Items[config.Type] = (short)modifiedQualification;
            }
            if (modifiedQualification > 0)
            {
                GameDataBridge.AddDataModification(4, 0, (ulong)charId, 32u, value);
            }
        }, (uid, typeof(List<short>)), (uid2, typeof(CombatSkillShorts)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    public static void QueryAliveCharByPreexistenceChar(int deadCharId)
    {
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 73, deadCharId, delegate (int offset, RawDataPool dataPool)
        {
            int item = -1;
            Serializer.Deserialize(dataPool, offset, ref item);
            UI_GMWindow.Instance.Log(string.Format(LocalStringManager.Get("GM_Message_GMFunc_QueryAliveCharByPreexistenceChar_Msg_0"), (item >= 0) ? item.ToString() : LocalStringManager.Get(3346)));
        });
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    public static void LogCharacterSamsaraInfo()
    {
        GameDataBridge.AddMethodCall(-1, 4, 74);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    public static void KillCharacter(int charId)
    {
        DialogCmd successCmd = new DialogCmd
        {
            Title = LocalStringManager.Get("LK_Success"),
            Content = string.Format(LocalStringManager.Get("GM_Message_GMFunc_KillCharacter_Msg_1"), charId),
            Yes = OnKillConfirm
        };
        GameDataBridge.AddMethodCall(-1, 4, 72, charId);
        if (charId != SingletonObject.getInstance<BasicGameData>().TaiwuCharId)
        {
            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", successCmd));
            UIManager.Instance.ShowUI(UIElement.Dialog);
        }
        static void OnKillConfirm()
        {
            if (UIElement.EventWindow.Exist)
            {
                UIManager.Instance.HideUI(UIElement.EventWindow);
            }
            if (UIElement.CharacterMenu.Exist)
            {
                UIManager.Instance.StackBack();
            }
        }
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CombatTypeField, 0.1f)]
    [GMFuncArg(1, EWidgetType.CharIdField, 0.16f)]
    [GMFuncArg(2, EWidgetType.CharIdField, 0.16f)]
    [GMFuncArg(3, EWidgetType.IntField, 0.08f)]
    [GMFuncArg(4, EWidgetType.IntField, 0.08f)]
    [GMFuncArg(5, EWidgetType.IntField, 0.08f)]
    public static void SimulateNpcCombat(sbyte combatType, int charIdA, int charIdB, int killBaseChance, int kidnapBaseChance, int releaseBaseChance)
    {
        GameDataBridge.AddMethodCall(-1, 4, 64, charIdA, charIdB, combatType, killBaseChance, kidnapBaseChance, releaseBaseChance);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.12f)]
    public static void SetXiangshuInfection(int charId, byte value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 65u, value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.ItemTypeIdField, 0.12f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.12f)]
    public static void GenerateRandomRefinedItem(int charId, sbyte itemType, int times)
    {
        GameDataBridge.AddMethodCall(-1, 4, 13, charId, itemType, times);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.CharIdField, 0.2f)]
    public static void CheckFavorability(int selfCharId, int relatedCharId)
    {
        DataUid selfCharTemplateIdUid = new DataUid(4, 0, (ulong)selfCharId, 1u);
        DataUid relatedCharTemplateIdUid = new DataUid(4, 0, (ulong)relatedCharId, 1u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            byte creatingType = Character.Instance[(short)data[selfCharTemplateIdUid]].CreatingType;
            byte creatingType2 = Character.Instance[(short)data[relatedCharTemplateIdUid]].CreatingType;
            if (creatingType == 1 && creatingType2 == 1)
            {
                SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 11, selfCharId, relatedCharId, delegate (int offset, RawDataPool dataPool)
                {
                    short item = 0;
                    Serializer.Deserialize(dataPool, offset, ref item);
                    UI_GMWindow.Instance.Log(item.ToString());
                });
            }
            else
            {
                UI_GMWindow.Instance.Log(short.MinValue.ToString());
            }
        }, (selfCharTemplateIdUid, typeof(short)), (relatedCharTemplateIdUid, typeof(short)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    public static void ChangeFavorability(int selfCharId, int relatedCharId, short delta)
    {
        DataUid selfCharTemplateIdUid = new DataUid(4, 0, (ulong)selfCharId, 1u);
        DataUid relatedCharTemplateIdUid = new DataUid(4, 0, (ulong)relatedCharId, 1u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            byte creatingType = Character.Instance[(short)data[selfCharTemplateIdUid]].CreatingType;
            byte creatingType2 = Character.Instance[(short)data[relatedCharTemplateIdUid]].CreatingType;
            if (creatingType == 1 && creatingType2 == 1)
            {
                GameDataBridge.AddMethodCall(-1, 4, 66, selfCharId, relatedCharId, delta);
            }
        }, (selfCharTemplateIdUid, typeof(short)), (relatedCharTemplateIdUid, typeof(short)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    public static void QuestCharacterCombatSkillAttainmentPanels(int selfCharId)
    {
        DataUid selfCharCombatSkillAttainmentPanelsUid = new DataUid(4, 0, (ulong)selfCharId, 62u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<short> list = (List<short>)data[selfCharCombatSkillAttainmentPanelsUid];
            for (sbyte b = 0; b < 14; b = (sbyte)(b + 1))
            {
                stringBuilder.Append(Config.CombatSkillType.Instance.GetItem(b).Name + ": ");
                for (int i = 0; i < 9; i++)
                {
                    short num = list[b * 9 + i];
                    if (i != 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    if (num >= 0)
                    {
                        stringBuilder.Append(CombatSkill.Instance.GetItem(num).Name ?? "");
                    }
                    else
                    {
                        stringBuilder.Append(LocalStringManager.Get(3346));
                    }
                }
                stringBuilder.AppendLine();
            }
            UI_GMWindow.Instance.Log(stringBuilder.ToString());
        }, (selfCharCombatSkillAttainmentPanelsUid, typeof(List<short>)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(2, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(3, EWidgetType.IntField, 0.1f)]
    public static void RecordFameAction(int charId, short fameActionId, int targetCharId = -1, short fameMultiplier = 1)
    {
        GameDataBridge.AddMethodCall(-1, 4, 68, charId, fameActionId, targetCharId, fameMultiplier);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    public static void ClearFameActionRecords(int charId)
    {
        GameDataBridge.AddMethodCall(-1, 4, 67, charId);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    public static void DisplayAllFameActions()
    {
        string ret = "";
        for (int i = 0; i < FameAction.Instance.Count; i++)
        {
            ret += $"{FameAction.Instance[i].Name.SetColor(Color.green)}-{i}  ";
        }
        UI_GMWindow.Instance.Log(ret);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.12f)]
    public static void MoveCharacterToCurrentMapBlock(int charId)
    {
        WorldMapModel mapModel = SingletonObject.getInstance<WorldMapModel>();
        GameDataBridge.AddMethodCall(-1, 4, 77, charId, new Location(mapModel.CurrentAreaId, mapModel.CurrentBlockId));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.12f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.12f)]
    [GMFuncArg(2, EWidgetType.BoolField, 0.22f)]
    public static void CreateRandomIntelligentCharacters(int charCount, sbyte orgTemplateId, bool createHere = true)
    {
        GameDataBridge.AddMethodCall(-1, 4, 69, charCount, orgTemplateId, createHere);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void RandomizeRelationShipsInSettlement(int orgTemplateId, int times)
    {
        for (int i = 0; i < times; i++)
        {
            GameDataBridge.AddMethodCall(-1, 4, 78, (sbyte)orgTemplateId);
        }
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void SetCharacterOrganization(int charId, sbyte orgTemplateId)
    {
        GameDataBridge.AddMethodCall(-1, 4, 70, charId, orgTemplateId);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(2, EWidgetType.BoolField, 0.2f)]
    public static void SetCharacterGrade(int charId, sbyte grade, bool principal = true)
    {
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 71, charId, grade, principal, delegate (int offset, RawDataPool dataPool)
        {
            bool item = true;
            Serializer.Deserialize(dataPool, offset, ref item);
            if (!item)
            {
                DialogCmd arg = new DialogCmd
                {
                    Title = LocalStringManager.Get("LK_Failed"),
                    Content = LocalStringManager.Get("GM_Message_GMFunc_SetCharacterGrade_Msg"),
                    Type = 2
                };
                UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", arg));
                UIManager.Instance.ShowUI(UIElement.Dialog);
            }
        });
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.StringField, 0.2f)]
    public static void QueryOrgTemplateIdByName(string orgName)
    {
        string ret = "";
        foreach (OrganizationItem config in (IEnumerable<OrganizationItem>)Organization.Instance)
        {
            if (config.Name.Contains(orgName))
            {
                ret += $"{config.Name.SetColor(Color.green)}-{config.TemplateId}  ";
            }
        }
        UI_GMWindow.Instance.Log(ret);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditDisorderOfQi(int charId, int value)
    {
        if (value >= 0 && value <= 8000)
        {
            GameDataBridge.AddDataModification(4, 0, (ulong)charId, 21u, (short)value);
        }
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(2, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(3, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(4, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(5, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(6, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(7, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(8, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(9, EWidgetType.BoolField, 0.08f)]
    public unsafe static void EditCharacterResource(int charId, int value, bool food = true, bool wood = true, bool metal = true, bool jade = true, bool fabric = true, bool herb = true, bool money = true, bool author = true)
    {
        if (charId == SingletonObject.getInstance<BasicGameData>().TaiwuCharId)
        {
            DialogCmd cmd = new DialogCmd
            {
                Title = LocalStringManager.Get("LK_Failed"),
                Content = LocalStringManager.Get("GM_Message_GMFunc_EditCharacterResource_Msg_0")
            };
            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", cmd));
            UIManager.Instance.ShowUI(UIElement.Dialog);
        }
        else
        {
            if (value < 0)
            {
                return;
            }
            DataUid uid = new DataUid(4, 0, (ulong)charId, 34u);
            UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
            {
                ResourceInts value2 = (ResourceInts)data[uid];
                if (food)
                {
                    ref int fixedElementField = ref value2.Items[0];
                    fixedElementField = value;
                }
                if (wood)
                {
                    value2.Items[1] = value;
                }
                if (metal)
                {
                    value2.Items[2] = value;
                }
                if (jade)
                {
                    value2.Items[3] = value;
                }
                if (fabric)
                {
                    value2.Items[4] = value;
                }
                if (herb)
                {
                    value2.Items[5] = value;
                }
                if (money)
                {
                    value2.Items[6] = value;
                }
                if (author)
                {
                    value2.Items[7] = value;
                }
                GameDataBridge.AddDataModification(uid.DomainId, uid.DataId, uid.SubId0, uid.SubId1, value2);
            }, (uid, typeof(ResourceInts)));
        }
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    public static void EditCharacterHealthAndBaseMaxHealth(int charId, int value, int max)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 19u, (short)value);
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 20u, (short)max);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    public static void EditCharacterLovingAndHatingItemSubType(int charId, int loving, int hating)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 35u, (short)loving);
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 36u, (short)hating);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(3, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(4, EWidgetType.IntField, 0.2f)]
    public unsafe static void EditExtraNeiliAllocation(int charId, int neili0, int neili1, int neili2, int neili3)
    {
        NeiliAllocation value = default(NeiliAllocation);
        value.Items[0] = (short)neili0;
        value.Items[1] = (short)neili1;
        value.Items[2] = (short)neili2;
        value.Items[3] = (short)neili3;
        GameDataBridge.AddMethodCall(-1, 4, 75, charId, value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditExtraNeili(int charId, int value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 27u, value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditConsummateLevel(int charId, int value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 28u, (sbyte)value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditActualAge(int charId, int value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 66u, (short)value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditBaseHealth(int charId, int health)
    {
        short target = (short)health;
        if (target < 0)
        {
            return;
        }
        DataUid maxHealthUid = new DataUid(4, 0, (ulong)charId, 95u);
        DataUid baseMaxHealthUid = new DataUid(4, 0, (ulong)charId, 20u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            short num = (short)data[maxHealthUid];
            if (num < target)
            {
                short num2 = (short)data[baseMaxHealthUid];
                GameDataBridge.AddDataModification(4, 0, (ulong)charId, 20u, (short)(num2 + (target - num)));
            }
            GameDataBridge.AddDataModification(4, 0, (ulong)charId, 19u, target);
        }, (maxHealthUid, typeof(short)), (baseMaxHealthUid, typeof(short)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditBirthMonth(int charId, int value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 5u, (sbyte)value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditHappiness(int charId, int value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 6u, (sbyte)value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditBaseMorality(int charId, int value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 7u, (short)value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.BoolField, 0.2f)]
    public static void EditTransgender(int charId, bool value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 13u, value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.BoolField, 0.2f)]
    public static void EditBisexual(int charId, bool value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 14u, value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditMonkType(int charId, int value)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 16u, (byte)value);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(3, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(4, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(5, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(6, EWidgetType.IntField, 0.1f)]
    public unsafe static void EditBaseMainAttributes(int charId, int attr0, int attr1, int attr2, int attr3, int attr4, int attr5)
    {
        short[] value = new short[6]
        {
            (short)attr0,
            (short)attr1,
            (short)attr2,
            (short)attr3,
            (short)attr4,
            (short)attr5
        };
        DataUid uid = new DataUid(4, 0, (ulong)charId, 18u);
        DataUid uid2 = new DataUid(4, 0, (ulong)charId, 43u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            MainAttributes value2 = (MainAttributes)data[uid];
            MainAttributes value3 = (MainAttributes)data[uid2];
            for (int i = 0; i < 6; i++)
            {
                ref short reference = ref value3.Items[i];
                reference = (short)(reference + (short)(value[i] - value2.Items[i]));
                value2.Items[i] = value[i];
            }
            GameDataBridge.AddDataModification(4, 0, (ulong)charId, 18u, value2);
            GameDataBridge.AddDataModification(4, 0, (ulong)charId, 43u, value3);
        }, (uid, typeof(MainAttributes)), (uid2, typeof(MainAttributes)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(3, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(4, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(5, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(6, EWidgetType.IntField, 0.1f)]
    public unsafe static void EditCurrMainAttributes(int charId, int attr0, int attr1, int attr2, int attr3, int attr4, int attr5)
    {
        short[] value = new short[6]
        {
            (short)attr0,
            (short)attr1,
            (short)attr2,
            (short)attr3,
            (short)attr4,
            (short)attr5
        };
        DataUid uid = new DataUid(4, 0, (ulong)charId, 43u);
        DataUid uid2 = new DataUid(4, 0, (ulong)charId, 80u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            MainAttributes value2 = (MainAttributes)data[uid];
            MainAttributes mainAttributes = (MainAttributes)data[uid2];
            for (int i = 0; i < 6; i++)
            {
                value2.Items[i] = (short)Mathf.Clamp(value[i], 0, mainAttributes.Items[i]);
            }
            GameDataBridge.AddDataModification(4, 0, (ulong)charId, 43u, value2);
        }, (uid, typeof(MainAttributes)), (uid2, typeof(MainAttributes)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.BoolField, 0.2f)]
    [GMFuncArg(2, EWidgetType.BoolField, 0.2f)]
    [GMFuncArg(3, EWidgetType.BoolField, 0.2f)]
    [GMFuncArg(4, EWidgetType.BoolField, 0.2f)]
    public static void EditDisableState(int charId, bool haveLeftArm, bool haveRightArm, bool haveLeftLeg, bool haveRightLeg)
    {
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 22u, haveLeftArm);
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 23u, haveRightArm);
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 24u, haveLeftLeg);
        GameDataBridge.AddDataModification(4, 0, (ulong)charId, 25u, haveRightLeg);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.CharIdField, 0.2f)]
    public static void MakeCharacterKidnapped(int charId, int targetCharId)
    {
        if (charId == targetCharId)
        {
            return;
        }
        if (targetCharId == SingletonObject.getInstance<BasicGameData>().TaiwuCharId)
        {
            DialogCmd cmd = new DialogCmd
            {
                Title = LocalStringManager.Get("GM_Message_GMFunc_TargetTaiwu_Msg_0"),
                Content = LocalStringManager.Get("GM_Message_GMFunc_TargetTaiwu_Msg_0"),
                Type = 2,
                MouseRightCancel = true
            };
            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", cmd));
            UIManager.Instance.ShowUI(UIElement.Dialog);
            return;
        }
        DataUid uid = new DataUid(4, 0, (ulong)targetCharId, 69u);
        DataUid selfCharTemplateIdUid = new DataUid(4, 0, (ulong)charId, 1u);
        DataUid targetCharTemplateIdUid = new DataUid(4, 0, (ulong)targetCharId, 1u);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            int num = (int)data[uid];
            byte creatingType = Character.Instance[(short)data[selfCharTemplateIdUid]].CreatingType;
            byte creatingType2 = Character.Instance[(short)data[targetCharTemplateIdUid]].CreatingType;
            if (num < 0 && (charId == SingletonObject.getInstance<BasicGameData>().TaiwuCharId || creatingType == 1) && creatingType2 == 1)
            {
                GameDataBridge.AddMethodCall(-1, 4, 76, charId, targetCharId);
            }
        }, (uid, typeof(int)), (selfCharTemplateIdUid, typeof(short)), (targetCharTemplateIdUid, typeof(short)));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(2, EWidgetType.BoolField, 0.2f)]
    [GMFuncArg(3, EWidgetType.IntField, 0.2f)]
    public static void MakeCharacterHaveSex(int selfCharId, int targetCharId, bool isRaped, int pregnantRemainTime)
    {
        pregnantRemainTime = Math.Max(0, pregnantRemainTime) + 1;
        GameDataBridge.AddMethodCall(-1, 4, 79, selfCharId, targetCharId, isRaped, pregnantRemainTime);
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.BoolField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(3, EWidgetType.IntField, 0.2f)]
    public static void ChangeInjury(int charId, bool isInnerInjury, sbyte bodyPartType, sbyte delta)
    {
        if (bodyPartType < 0 || bodyPartType > 6)
        {
            UI_GMWindow.Instance.Log("身体部位值设置错误(0-胸背；1-腰腹；2-头颅；3-左臂；4-右臂；5-左腿；6-右腿)");
            return;
        }
        GameDataBridge.AddMethodCall(-1, 4, 14, charId, isInnerInjury, bodyPartType, delta);
        UI_GMWindow.Instance.Log("设置成功");
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    public static void ChangePoisoned(int charId, sbyte poisonType, int changeValue)
    {
        if (poisonType < 0 || poisonType > 5)
        {
            UI_GMWindow.Instance.Log("中毒类型设置错误（0-烈毒；1-郁毒；2-寒毒；3-赤毒；4-腐毒；5-幻毒）");
            return;
        }
        GameDataBridge.AddMethodCall(-1, 4, 15, charId, poisonType, changeValue);
        UI_GMWindow.Instance.Log("设置成功");
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.4f)]
    public static void QueryFixedCharacterIdByTemplateId(short templateId)
    {
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 63, templateId, delegate (int offset, RawDataPool dataPool)
        {
            int item = 0;
            Serializer.Deserialize(dataPool, offset, ref item);
            UI_GMWindow.Instance.Log($"{item}");
        });
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharacterRelationshipTypeField, 0.1f)]
    [GMFuncArg(1, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(2, EWidgetType.CharIdField, 0.2f)]
    public static void AddCharacterRelationship(sbyte type, int charIdA, int charIdB)
    {
        GameDataBridge.AddMethodCall(-1, 4, 20, charIdA, charIdB, RelationType.GetRelationType(type));
    }

    [GMFunc(EGMGroup.CharacterBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    public static void QuestAiPersonalNeeds(int charId)
    {
        UI_GMWindow gmWindow = UI_GMWindow.Instance;
        DataUid dataUid = new DataUid(4, 0, (ulong)charId, 72u);
        gmWindow.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            List<GameData.Domains.Character.Ai.PersonalNeed> list = (List<GameData.Domains.Character.Ai.PersonalNeed>)data[dataUid];
            StringBuilder stringBuilder = new StringBuilder();
            foreach (GameData.Domains.Character.Ai.PersonalNeed item in list)
            {
                stringBuilder.AppendLine(item.ToString());
            }
            gmWindow.Log(stringBuilder.ToString());
        }, (dataUid, typeof(List<GameData.Domains.Character.Ai.PersonalNeed>)));
    }

    [GMFunc(EGMGroup.CharacterResource, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(1, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(2, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(3, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(4, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(5, EWidgetType.BoolField, 0.08f)]
    [GMFuncArg(6, EWidgetType.BoolField, 0.08f)]
    public static void GetBaseResource(int value, bool food = true, bool wood = true, bool metal = true, bool jade = true, bool fabric = true, bool herb = true)
    {
        if (food)
        {
            AddResource(0, value);
        }
        if (wood)
        {
            AddResource(1, value);
        }
        if (metal)
        {
            AddResource(2, value);
        }
        if (jade)
        {
            AddResource(3, value);
        }
        if (fabric)
        {
            AddResource(4, value);
        }
        if (herb)
        {
            AddResource(5, value);
        }
    }

    [GMFunc(EGMGroup.CharacterResource, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.1f)]
    [GMFuncArg(1, EWidgetType.BoolField, 0.1f)]
    [GMFuncArg(2, EWidgetType.BoolField, 0.1f)]
    [GMFuncArg(3, EWidgetType.BoolField, 0.1f)]
    public static void GetAdvancedResource(int value, bool money = true, bool authority = true, bool exp = true)
    {
        if (money)
        {
            AddResource(6, value);
        }
        if (authority)
        {
            AddResource(7, value);
        }
        if (exp)
        {
            GameDataBridge.AddMethodCall(-1, 5, 20, value);
        }
    }

    private static void AddResource(sbyte type, int count)
    {
        GameDataBridge.AddMethodCall(-1, 5, 18, type, count);
    }

    [GMFunc(EGMGroup.CharacterResource, 0.25f, 1000)]
    public static void QueryCricketLuckPoint()
    {
        DataUid dataId1 = new DataUid(5, 2, ulong.MaxValue);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            int num = (int)data[dataId1];
            UI_GMWindow.Instance.Log($"{num}");
        }, (dataId1, typeof(int)));
    }

    [GMFunc(EGMGroup.CharacterResource, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    public static void EditCricketLuckPoint(int point)
    {
        GameDataBridge.AddDataModification(5, 2, ulong.MaxValue, uint.MaxValue, point);
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.12f)]
    [GMFuncArg(2, EWidgetType.ItemTypeIdField, 0.12f)]
    [GMFuncArg(3, EWidgetType.ItemIdField, 0.12f)]
    [GMFuncArg(4, EWidgetType.ItemIdField, 0.12f)]
    public static void GetItem(int charId, int count, sbyte itemType, short idStar, short? idEnd)
    {
        if (idEnd.HasValue)
        {
            idEnd = Math.Max(idStar, idEnd.Value);
            for (short itemId = idStar; itemId <= idEnd; itemId = (short)(itemId + 1))
            {
                GameDataBridge.AddMethodCall(-1, 4, 26, charId, itemType, itemId, count);
            }
        }
        else
        {
            GameDataBridge.AddMethodCall(-1, 4, 26, charId, itemType, idStar, count);
        }
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.StringField, 0.2f)]
    public static void QueryItemIdByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        if (_queryCache == null)
        {
            _queryCache = new List<(string, (int, short))>();
            _queryCache.AddRange(Weapon.Instance.Select((WeaponItem a) => (a.Name, (0, a.TemplateId))));
            _queryCache.AddRange(Armor.Instance.Select((ArmorItem a) => (a.Name, (1, a.TemplateId))));
            _queryCache.AddRange(Accessory.Instance.Select((AccessoryItem a) => (a.Name, (2, a.TemplateId))));
            _queryCache.AddRange(Clothing.Instance.Select((ClothingItem a) => (a.Name, (3, a.TemplateId))));
            _queryCache.AddRange(Carrier.Instance.Select((CarrierItem a) => (a.Name, (4, a.TemplateId))));
            _queryCache.AddRange(Config.Material.Instance.Select((MaterialItem a) => (a.Name, (5, a.TemplateId))));
            _queryCache.AddRange(CraftTool.Instance.Select((CraftToolItem a) => (a.Name, (6, a.TemplateId))));
            _queryCache.AddRange(Food.Instance.Select((FoodItem a) => (a.Name, (7, a.TemplateId))));
            _queryCache.AddRange(Medicine.Instance.Select((MedicineItem a) => (a.Name, (8, a.TemplateId))));
            _queryCache.AddRange(TeaWine.Instance.Select((TeaWineItem a) => (a.Name, (9, a.TemplateId))));
            _queryCache.AddRange(SkillBook.Instance.Select((SkillBookItem a) => (a.Name, (10, a.TemplateId))));
            _queryCache.AddRange(Cricket.Instance.Select((CricketItem a) => (a.Name, (11, a.TemplateId))));
            _queryCache.AddRange(Misc.Instance.Select((MiscItem a) => (a.Name, (12, a.TemplateId))));
        }
        IEnumerable<(string, (int, short))> result = _queryCache.Where(((string name, (int type, short id)) a) => a.name.Contains(name));
        string ret = "";
        foreach (var one in result)
        {
            ret += $"{one.Item1.SetColor(Color.green)}-type:{one.Item2.Item1}-id:{one.Item2.Item2}  ";
        }
        UI_GMWindow.Instance.Log(ret);
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    public static void QueryItemType()
    {
        string ret = "";
        for (int i = 0; i < 13; i++)
        {
            ret += $"{LocalStringManager.Get($"LK_ItemType_{i}").SetColor(Color.green)}-{i}  ";
        }
        UI_GMWindow.Instance.Log(ret);
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void GetCricket(short colorId, short partId)
    {
        GameDataBridge.AddMethodCall(-1, 4, 19, colorId, partId);
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(2, EWidgetType.IntField, 0.2f)]
    public static void GetRandomCricket(int grade, int winsCount, int lossesCount)
    {
        if (grade < 0 || grade > 8)
        {
            UI_GMWindow.Instance.Log(LocalStringManager.Get("GM_Message_GMFunc_GetRandomCricket_Msg"));
        }
        GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)19, (short)0, (short)0, grade, (short)winsCount, (short)lossesCount);
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    public static void QueryCricketPartIds(string partName = "")
    {
        string ret = "";
        foreach (CricketPartsItem config in (IEnumerable<CricketPartsItem>)CricketParts.Instance)
        {
            if (config.Name.Any((string a) => a.Contains(partName)))
            {
                ret += $"{config.Name[0].SetColor(Color.green)}-{config.TemplateId}  ";
            }
        }
        UI_GMWindow.Instance.Log(ret);
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    public static void DisplayCricketPreview()
    {
        UI_GMWindow.Instance.CricketPreview.SetActive(value: true);
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.12f)]
    [GMFuncArg(2, EWidgetType.ItemTypeIdField, 0.12f)]
    [GMFuncArg(3, EWidgetType.ItemIdField, 0.12f)]
    [GMFuncArg(4, EWidgetType.ItemIdField, 0.12f)]
    public static void ChangeItemDurability(int charId, short change, sbyte itemType, short idStar, short? idEnd)
    {
        if (!idEnd.HasValue)
        {
            idEnd = idStar;
        }
        GameDataBridge.AddMethodCall(-1, 6, 22, charId, change, itemType, idStar, idEnd.GetValueOrDefault());
    }

    [GMFunc(EGMGroup.CharacterItem, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.CharIdField, 0.1f)]
    [GMFuncArg(1, EWidgetType.BoolField, 0.1f)]
    public static void ChangePoisonIdentified(int charId, bool isIdentified)
    {
        GameDataBridge.AddMethodCall(-1, 6, 23, charId, isIdentified);
    }

    [GMFunc(EGMGroup.MapQuickOperation, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.BoolField, 0.15f)]
    [GMFuncArg(1, EWidgetType.BoolField, 0.15f)]
    [GMFuncArg(2, EWidgetType.BoolField, 0.15f)]
    [GMFuncArg(3, EWidgetType.BoolField, 0.15f)]
    [GMFuncArg(4, EWidgetType.BoolField, 0.15f)]
    public static void QuickDeployGame(bool disableMove, bool disableItem, bool disableSkill, bool disableResource, bool disableSwordTomb)
    {
        BasicGameData basic = SingletonObject.getInstance<BasicGameData>();
        WorldMapModel mapModel = SingletonObject.getInstance<WorldMapModel>();
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 50, new List<int> { basic.TaiwuCharId }, delegate (int offsetE, RawDataPool dataPoolE)
        {
            List<CharacterDisplayData> item = new List<CharacterDisplayData>();
            Serializer.Deserialize(dataPoolE, offsetE, ref item);
            if (item != null && item.Count > 0 && item[0].Location.IsValid() && mapModel.Areas[item[0].Location.AreaId].GetConfig().TemplateId == 0)
            {
                OpenAllWorldFunction();
                EditMainlineProgress(8);
                SetCharacterOrganization(basic.TaiwuCharId, 16);
                if (!disableItem)
                {
                    int num = 10;
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)12, (short)9, 200);
                    for (int i = 0; i < 13; i++)
                    {
                        if (i != 11 && i != 0 && i != 1 && i != 2 && i != 4 && i != -1)
                        {
                            for (int j = 0; j < num; j++)
                            {
                                GameDataBridge.AddMethodCall(-1, 4, 26, basic.TaiwuCharId, (sbyte)i, (short)j, 1);
                            }
                        }
                    }
                }
                if (!disableSwordTomb)
                {
                    Location[] swordTombLocations = mapModel.SwordTombLocations;
                    int k = 0;
                    for (int num2 = swordTombLocations.Length; k < num2; k++)
                    {
                        GameDataBridge.AddMethodCall(-1, 2, 29, swordTombLocations[k], (short)(128 + k), arg3: true);
                    }
                }
                DataUid learnedUid = new DataUid(4, 0, (ulong)basic.TaiwuCharId, 60u);
                UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
                {
                    if (!disableSkill)
                    {
                        List<short> list = (List<short>)data[learnedUid];
                        int num3 = 10;
                        ushort arg2 = ushort.MaxValue;
                        for (int l = 0; l < num3; l++)
                        {
                            GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)93, basic.TaiwuCharId, (short)l, (sbyte)100, arg2);
                        }
                    }
                }, (learnedUid, typeof(List<short>)));
                if (!disableItem)
                {
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)27, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)28, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)29, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)30, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)31, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)32, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)33, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)34, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)35, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)18, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)19, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)20, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)21, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)22, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)23, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)24, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)25, 1);
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)9, (short)26, 1);
                }
                if (!disableResource)
                {
                    int arg = 1048576;
                    for (sbyte b = -1; b < 8; b = (sbyte)(b + 1))
                    {
                        if (b != -1)
                        {
                            GameDataBridge.AddMethodCall(-1, 5, 18, b, arg);
                        }
                    }
                }
                short id = 26;
                if (!disableItem)
                {
                    GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)26, basic.TaiwuCharId, (sbyte)4, id, 1);
                }
                SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 27, basic.TaiwuCharId, Carrier.Instance[id].ItemSubType, delegate (int offset, RawDataPool dataPool)
                {
                    List<ItemDisplayData> item2 = new List<ItemDisplayData>();
                    Serializer.Deserialize(dataPool, offset, ref item2);
                    if (!disableItem)
                    {
                        foreach (ItemDisplayData current in item2)
                        {
                            if (current.Key.TemplateId == id)
                            {
                                GameDataBridge.AddMethodCall(-1, (ushort)4, (ushort)25, basic.TaiwuCharId, (sbyte)(-1), (sbyte)11, current.Key);
                                break;
                            }
                        }
                    }
                    if (!disableMove)
                    {
                        Location taiwuVillageBlock = SingletonObject.getInstance<WorldMapModel>().GetTaiwuVillageBlock();
                        UI_Worldmap uI_Worldmap = UIElement.WorldMap.UiBaseAs<UI_Worldmap>();
                        uI_Worldmap.StartCoroutine(uI_Worldmap.QuickTravel(taiwuVillageBlock.AreaId));
                    }
                });
            }
        });
    }

    [GMFunc(EGMGroup.MapQuickOperation, 0.25f, 1000)]
    public static void GotoSecretVillage()
    {
        WorldMapModel mapModel = SingletonObject.getInstance<WorldMapModel>();
        IntraStateTravel = true;
        InterStateTravel = true;
        UI_Worldmap worldMap = UIElement.WorldMap.UiBaseAs<UI_Worldmap>();
        worldMap.StartCoroutine(worldMap.QuickTravel(137));
    }

    [GMFunc(EGMGroup.MapQuickOperation, 0.25f, 1000)]
    public static void GotoBrokenPerform()
    {
        WorldMapModel mapModel = SingletonObject.getInstance<WorldMapModel>();
        IntraStateTravel = true;
        InterStateTravel = true;
        UI_Worldmap worldMap = UIElement.WorldMap.UiBaseAs<UI_Worldmap>();
        worldMap.StartCoroutine(worldMap.QuickTravel(138));
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.AdventureIdField, 0.2f)]
    public static void EnterAdventure(short adventureId)
    {
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(10, 0, adventureId, delegate (int offset, RawDataPool dataPool)
        {
            Location item = new Location(-1, -1);
            Serializer.Deserialize(dataPool, offset, ref item);
            if (item.IsValid())
            {
                MapAreaItem mapAreaItem = MapArea.Instance[SingletonObject.getInstance<WorldMapModel>().Areas[item.AreaId].GetTemplateId()];
                UI_GMWindow.Instance.Log(LocalStringManager.GetFormat(3005, Adventure.Instance[adventureId].Name, MapState.Instance[mapAreaItem.StateID].Name + "-" + mapAreaItem.Name));
            }
            else
            {
                UI_GMWindow.Instance.Log(LocalStringManager.Get(3004));
            }
        });
    }

    [GMFunc(EGMGroup.MapQuickOperation, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.FloatField, 0.2f)]
    [GMFuncArg(1, EWidgetType.FloatField, 0.2f)]
    public static void SetAdventureAnimSpeed(float unfoldSpeed, float moveSpeed)
    {
        UI_Adventure.UnfoldAnimationTimeScale = Mathf.Clamp(unfoldSpeed, 0.2f, 5f);
        UI_Adventure.DoMoveAnimationTimeScale = Mathf.Clamp(moveSpeed, 0.2f, 5f);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.BossCharIdField, 0.2f)]
    public static void CreateFixedCharacterAtCurrentBlock(int bossTemplateId)
    {
        GameDataBridge.AddMethodCall(-1, 2, 6, (short)bossTemplateId);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.BossCharIdField, 0.2f)]
    public static void FightWithBoss(int bossCharacterId)
    {
        GameDataBridge.AddMethodCall(-1, 8, 30, (short)bossCharacterId);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.AnimalCharIdField, 0.2f)]
    public static void FightWithAnimal(int animalCharacterId)
    {
        GameDataBridge.AddMethodCall(-1, 8, 31, (short)animalCharacterId);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void QuestAllFactionInfos()
    {
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(3, 7, delegate (int offset, RawDataPool dataPool)
        {
            List<List<CharacterDisplayData>> item = new List<List<CharacterDisplayData>>();
            Serializer.Deserialize(dataPool, offset, ref item);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (List<CharacterDisplayData> current in item)
            {
                CharacterDisplayData characterDisplayData = current[0];
                current.RemoveAt(0);
                stringBuilder.AppendFormat("{0}[{1}][{2}]\n", CommonUtils.GetOrganizationGradeString(characterDisplayData.OrgInfo, characterDisplayData.Gender, characterDisplayData.CurrAge), NameCenter.GetNameByDisplayData(characterDisplayData, isTaiwu: false, getRealName: true), characterDisplayData.CharacterId);
                int i = 0;
                for (int count = current.Count; i < count; i++)
                {
                    CharacterDisplayData characterDisplayData2 = current[i];
                    stringBuilder.AppendFormat("\t- {0}[{1}]\n", NameCenter.GetNameByDisplayData(characterDisplayData2, isTaiwu: false, getRealName: true), characterDisplayData2.CharacterId);
                }
            }
            UI_GMWindow.Instance.Log(stringBuilder.ToString());
        });
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void QuestAllGroupInfos()
    {
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 12, delegate (int offset, RawDataPool dataPool)
        {
            List<List<CharacterDisplayData>> item = new List<List<CharacterDisplayData>>();
            Serializer.Deserialize(dataPool, offset, ref item);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (List<CharacterDisplayData> current in item)
            {
                CharacterDisplayData characterDisplayData = current[0];
                current.RemoveAt(0);
                stringBuilder.AppendFormat("{0}[{1}][{2}]\n", CommonUtils.GetOrganizationGradeString(characterDisplayData.OrgInfo, characterDisplayData.Gender, characterDisplayData.CurrAge), NameCenter.GetNameByDisplayData(characterDisplayData, isTaiwu: false, getRealName: true), characterDisplayData.CharacterId);
                int i = 0;
                for (int count = current.Count; i < count; i++)
                {
                    CharacterDisplayData characterDisplayData2 = current[i];
                    stringBuilder.AppendFormat("\t- {0}[{1}]\n", NameCenter.GetNameByDisplayData(characterDisplayData2, isTaiwu: false, getRealName: true), characterDisplayData2.CharacterId);
                }
            }
            UI_GMWindow.Instance.Log(stringBuilder.ToString());
        });
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.LegacyTemplateIdField, 0.4f)]
    public static void AddLegacy(short templateId)
    {
        DataUid uid = new DataUid(5, 39, ulong.MaxValue);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            List<short> list = (List<short>)data[uid];
            list.Add(templateId);
            GameDataBridge.AddDataModification(uid.DomainId, uid.DataId, uid.SubId0, uid.SubId1, list);
        }, (uid, typeof(List<short>)));
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.LegacyPointTemplateIdField, 0.4f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.1f)]
    public static void AddLegacyPoint(short template, int percent)
    {
        GameDataBridge.AddMethodCall(-1, 5, 19, template, percent);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void UnlockAllSettlementInformation()
    {
        GameDataBridge.AddMethodCall(-1, 3, 6);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void UnlockAllStation()
    {
        GameDataBridge.AddMethodCall(-1, 2, 3);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void ShowAllMapBlocks()
    {
        GameDataBridge.AddMethodCall(-1, 2, 2);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void Save()
    {
        if (UIManager.Instance.IsInStack(UIElement.StateAdventure) || UIElement.Adventure.Exist || UIManager.Instance.IsInStack(UIElement.Combat) || UIElement.Combat.Exist)
        {
            return;
        }
        GameDataBridge.AddMethodCall(-1, 12, 16);
        GlobalOperations.SaveWorld();
        UI_Advance.OnSavingWorldStateChanged = delegate (bool isSaving)
        {
            if (!isSaving)
            {
                UI_Advance.OnSavingWorldStateChanged = null;
                DialogCmd arg = new DialogCmd
                {
                    Title = LocalStringManager.Get("GM_Message_GMFunc_Save_Msg_0"),
                    Content = LocalStringManager.Get("GM_Message_GMFunc_Save_Msg_1"),
                    Type = 2,
                    MouseRightCancel = true
                };
                UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", arg));
                UIManager.Instance.ShowUI(UIElement.Dialog);
            }
        };
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void AdvanceManyMonths(int monthCount, int saveSpacing)
    {
        if (AdvanceMonthCoroutine == null && !UIManager.Instance.IsInStack(UIElement.StateAdventure) && !UIElement.Adventure.Exist && !UIManager.Instance.IsInStack(UIElement.Combat) && !UIElement.Combat.Exist)
        {
            SingletonObject.getInstance<YieldHelper>().StartCoroutine(AdvanceMonthCoroutine = QuickAdvance(monthCount, saveSpacing));
        }
    }

    private static IEnumerator QuickAdvance(int times, int saveSpacing)
    {
        WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();
        BasicGameData basicGameData = SingletonObject.getInstance<BasicGameData>();
        int advancedTimes = 0;
        bool disableSavingOrigin = DisableAutoSaving;
        while (advancedTimes < times)
        {
            DisableAutoSaving = advancedTimes < times - 1 && (saveSpacing <= 0 || advancedTimes % saveSpacing != saveSpacing - 1);
            int leftDays = SingletonObject.getInstance<TimeManager>().GetLeftDaysInCurrMonth();
            if (leftDays > 0)
            {
                GameDataBridge.AddMethodCall(-1, 1, 9, leftDays);
                yield return waitFrame;
            }
            advancedTimes++;
            if (basicGameData.AdvancingMonthState != 0)
            {
                Debug.LogWarning($"assert failed: unexpected month state: {basicGameData.AdvancingMonthState}");
            }
            GameDataBridge.AddMethodCall(-1, 1, 10);
            Game.AdvancingMonth = true;
            yield return new WaitUntil(() => basicGameData.AdvancingMonthState != 0);
            yield return waitFrame;
            yield return new WaitUntil(() => basicGameData.AdvancingMonthState == 20);
            yield return waitFrame;
            yield return new WaitUntil(() => UIElement.MonthNotify.IsInState(EUiElementState.Ready));
            yield return waitFrame;
            if (!DisableAutoSaving)
            {
                yield return new WaitUntil(() => SingletonObject.getInstance<BasicGameData>().SavingWorld);
                yield return waitFrame;
                yield return new WaitUntil(() => !SingletonObject.getInstance<BasicGameData>().SavingWorld);
                yield return waitFrame;
            }
            UIManager.Instance.HideUI(UIElement.MonthNotify);
            yield return new WaitUntil(() => basicGameData.AdvancingMonthState == 0);
            yield return waitFrame;
        }
        DisableAutoSaving = disableSavingOrigin;
        AdvanceMonthCoroutine = null;
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void KillTaiWuVillagers()
    {
        int taiWuId = SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(3, 2, SingletonObject.getInstance<WorldMapModel>().GetTaiwuVillageSettlementId(), delegate (int offset, RawDataPool dataPool)
        {
            List<CharacterDisplayData> item = new List<CharacterDisplayData>();
            Serializer.Deserialize(dataPool, offset, ref item);
            foreach (CharacterDisplayData current in item)
            {
                int characterId = current.CharacterId;
                if (characterId != taiWuId)
                {
                    GameDataBridge.AddMethodCall(-1, 4, 72, characterId);
                }
            }
        });
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    public static void TransferChicken(int amount)
    {
        short taiWuSettlementId = SingletonObject.getInstance<WorldMapModel>().GetTaiwuVillageSettlementId();
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall((ushort)9, (ushort)92, (int)taiWuSettlementId, (Action<int, RawDataPool>)delegate (int offset, RawDataPool dataPool)
        {
            List<int> item = new List<int>();
            Serializer.Deserialize(dataPool, offset, ref item);
            List<int> list = new List<int>();
            for (int i = 0; i < Config.Chicken.Instance.Count; i++)
            {
                if (list.Count >= amount)
                {
                    break;
                }
                if (!item.Contains(i))
                {
                    list.Add(i);
                }
            }
            list.Shuffle();
            foreach (int current in list)
            {
                GameDataBridge.AddMethodCall(-1, (ushort)9, (ushort)90, current, (int)taiWuSettlementId);
            }
        });
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void QueryXiangshuLevel()
    {
        DataUid dataId1 = new DataUid(1, 1, ulong.MaxValue);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            sbyte xiangshuProgress = (sbyte)data[dataId1];
            UI_GMWindow.Instance.Log($"{GameData.Domains.World.SharedMethods.GetXiangshuLevel(xiangshuProgress)}");
        }, (dataId1, typeof(sbyte)));
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.StringField, 0.2f)]
    public static void QueryAreaId(string areaName)
    {
        MapAreaData[] areas = SingletonObject.getInstance<WorldMapModel>().Areas;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < areas.Length; i++)
        {
            MapAreaData area = areas[i];
            if (area.GetConfig().Name.Equals(areaName))
            {
                sb.AppendFormat("{0}：{1}\n", areaName, i);
            }
        }
        UI_GMWindow.Instance.Log(sb.ToString());
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    public static void QueryAreaName(int id)
    {
        MapAreaData[] areas = SingletonObject.getInstance<WorldMapModel>().Areas;
        if (id >= 0 && id < areas.Length)
        {
            UI_GMWindow.Instance.Log(areas[id].GetConfig().Name);
        }
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void ChangeSpiritualDebt(int areaId, int spiritualDebt)
    {
        GameDataBridge.AddMethodCall(-1, 2, 4, (short)areaId, (short)spiritualDebt);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    public static void QuerySpiritualDebt(int areaId)
    {
        WorldMapModel mapModel = SingletonObject.getInstance<WorldMapModel>();
        if (areaId >= 0 && areaId < mapModel.Areas.Length)
        {
            MapAreaData area = mapModel.Areas[areaId];
            UI_GMWindow.Instance.Log($"{area.GetConfig().Name}[{areaId}]: {area.SpiritualDebt}");
        }
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.StoryProgressField, 0.5f)]
    public static void EditMainlineProgress(short value)
    {
        GameDataBridge.AddDataModification(1, 5, ulong.MaxValue, uint.MaxValue, value);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void QueryMainlineProgress()
    {
        DataUid uid = new DataUid(1, 5, ulong.MaxValue);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            Dictionary<short, string> storyProgress2Name = UI_GMWindow.StoryProgress2Name;
            short key = (short)data[uid];
            if (storyProgress2Name.ContainsKey(key))
            {
                UI_GMWindow.Instance.Log(storyProgress2Name[key] ?? "");
            }
        }, (uid, typeof(short)));
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.StateIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditStateTaskStatus(sbyte id, int value)
    {
        GameDataBridge.AddDataModification(1, 4, (ulong)id, uint.MaxValue, (sbyte)value);
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.XiangshuAvatarIdField, 0.2f)]
    public static void QueryXiangshuAvatarTaskStatus(sbyte id)
    {
        DataUid uid = new DataUid(1, 2, (ulong)id);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            XiangshuAvatarTaskStatus xiangshuAvatarTaskStatus = (XiangshuAvatarTaskStatus)data[uid];
            UI_GMWindow.Instance.Log($"剑冢状态: {xiangshuAvatarTaskStatus.SwordTombStatus} 紫竹状态: {xiangshuAvatarTaskStatus.JuniorXiangshuTaskStatus} 紫竹角色 Id: {xiangshuAvatarTaskStatus.JuniorXiangshuCharId}");
        }, (uid, typeof(XiangshuAvatarTaskStatus)));
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.XiangshuAvatarIdField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void EditXiangshuAvatarFavorability(sbyte id, short delta)
    {
        DataUid uid = new DataUid(1, 2, (ulong)id);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            GameDataBridge.AddMethodCall(-1, 4, 66, ((XiangshuAvatarTaskStatus)data[uid]).JuniorXiangshuCharId, SingletonObject.getInstance<BasicGameData>().TaiwuCharId, delta);
        }, (uid, typeof(XiangshuAvatarTaskStatus)));
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.XiangshuAvatarIdField, 0.2f)]
    public static void QueryXiangshuAvatarFavorability(sbyte id)
    {
        DataUid uid = new DataUid(1, 2, (ulong)id);
        UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
        {
            XiangshuAvatarTaskStatus xiangshuAvatarTaskStatus = (XiangshuAvatarTaskStatus)data[uid];
            SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(4, 11, xiangshuAvatarTaskStatus.JuniorXiangshuCharId, SingletonObject.getInstance<BasicGameData>().TaiwuCharId, delegate (int offset, RawDataPool dataPool)
            {
                short item = 0;
                Serializer.Deserialize(dataPool, offset, ref item);
                UI_GMWindow.Instance.Log(item.ToString());
            });
        }, (uid, typeof(XiangshuAvatarTaskStatus)));
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.BattleSceneIdField, 0.1f)]
    public static void SetBattleSceneId(int id)
    {
        UI_Combat.GmSetSceneId = id;
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public unsafe static void QueryCurrentMapBlockData()
    {
        if (!UIElement.WorldMap.Exist)
        {
            return;
        }
        UI_Worldmap map = UIElement.WorldMap.UiBaseAs<UI_Worldmap>();
        if (!(map.GetType().GetField("_curSelectBlock", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(map) is MapBlockData currentBlockData))
        {
            return;
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Format("{0}: {1}", LocalStringManager.Get("LK_MapBlockData_Malice"), currentBlockData.Malice));
        sb.Append(LocalStringManager.Get(1881) + ": ");
        for (sbyte i = 0; i < 6; i = (sbyte)(i + 1))
        {
            if (i != 0)
            {
                sb.Append(", ");
            }
            sb.Append($"[{Config.ResourceType.Instance.GetItem(i).Name}: {currentBlockData.CurrResources.Items[i]} / {currentBlockData.MaxResources.Items[i]}]");
        }
        sb.AppendLine();
        UI_GMWindow.Instance.Log(sb.ToString());
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    public static void EditCurrentMapBlockMalice(int value)
    {
        if (UIElement.WorldMap.Exist)
        {
            UI_Worldmap map = UIElement.WorldMap.UiBaseAs<UI_Worldmap>();
            if (map.GetType().GetField("_curSelectBlock", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(map) is MapBlockData currentBlockData)
            {
                currentBlockData.Malice = (short)value;
                GameDataBridge.AddMethodCall(-1, 2, 5, currentBlockData);
            }
        }
    }

    [GMFunc(EGMGroup.MapBase, 0.25f, 1000)]
    public static void OpenAllWorldFunction()
    {
        _lastWorldFunctionsStatuses = ulong.MaxValue;
        UI_GMWindow.Instance.SwitchWindow();
    }

    private static bool _EditWorldFunctionsStatus(byte id)
    {
        if (!_lastWorldFunctionsStatusesLoaded)
        {
            return false;
        }
        return WorldFunctionType.Get(_lastWorldFunctionsStatuses, id);
    }

    public static void RefreshWorldFunctionsStatus()
    {
        if (Game.Instance.GetCurrentGameStateName() == EGameState.InGame)
        {
            DataUid dataId1 = new DataUid(1, 7, ulong.MaxValue);
            DataUid dataId2 = new DataUid(1, 6, ulong.MaxValue);
            UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
            {
                _lastWorldFunctionsStatusesLoaded = true;
                _lastWorldFunctionsStatuses = (ulong)data[dataId1];
                _isFinalBossDefeated = (bool)data[dataId2];
                UI_GMWindow.Instance.RefreshPage(1);
            }, (dataId1, typeof(ulong)), (dataId2, typeof(bool)));
        }
    }

    public static void ModifyWorldFunctionsStatus()
    {
        if (Game.Instance.GetCurrentGameStateName() == EGameState.InGame)
        {
            _lastWorldFunctionsStatusesLoaded = false;
            GameDataBridge.AddDataModification(1, 7, ulong.MaxValue, uint.MaxValue, _lastWorldFunctionsStatuses);
        }
    }

    private static bool _EditWorldFunctionsStatus(byte id, bool value)
    {
        if (Game.Instance.GetCurrentGameStateName() != EGameState.InGame || UI_GMWindow.Instance.IsGameDataReceiving())
        {
            return false;
        }
        if (value)
        {
            _lastWorldFunctionsStatuses = WorldFunctionType.Set(_lastWorldFunctionsStatuses, id);
        }
        else
        {
            _lastWorldFunctionsStatuses = WorldFunctionType.Reset(_lastWorldFunctionsStatuses, id);
        }
        UI_GMWindow.Instance.SwitchWindow();
        return value;
    }

    [GMFunc(EGMGroup.MapBase, 0.4f, 1000)]
    [GMFuncArg(0, EWidgetType.CombatResultTypeField, 0.2f)]
    [GMFuncArg(1, EWidgetType.LifeSkillCombatResultTypeField, 0.2f)]
    public static void OvercomeBattleInEvent(sbyte combatResultType, sbyte lifeSkillCombatResultType)
    {
        OvercomeCombatResultType = combatResultType;
        OvercomeLifeSkillCombatResultType = lifeSkillCombatResultType;
        switch (lifeSkillCombatResultType)
        {
            case 2:
                UI_LifeSkillBattle.IsPlayerUseAI = true;
                UI_LifeSkillBattle.IsAdversaryUseManual = false;
                break;
            case 3:
                UI_LifeSkillBattle.IsPlayerUseAI = false;
                UI_LifeSkillBattle.IsAdversaryUseManual = true;
                break;
            default:
                UI_LifeSkillBattle.IsPlayerUseAI = false;
                UI_LifeSkillBattle.IsAdversaryUseManual = false;
                break;
        }
    }

    public static bool IsOvercomeCombat(short combatConfigId)
    {
        if (OvercomeCombatResultType < 0)
        {
            return false;
        }
        CombatConfigItem combatConfig = CombatConfig.Instance.GetItem(combatConfigId);
        sbyte currentOvercomeCombatResultType = OvercomeCombatResultType;
        sbyte b = currentOvercomeCombatResultType;
        if ((uint)b > 5u)
        {
            sbyte[] results = null;
            results = ((combatConfig.CombatType != 2) ? new sbyte[6] { 5, 3, 1, 4, 2, 0 } : new sbyte[4] { 5, 3, 4, 2 });
            results.Shuffle();
            currentOvercomeCombatResultType = results.First();
        }
        UI_GMWindow.Instance.LogCombatOvercome(currentOvercomeCombatResultType);
        GameDataBridge.AddMethodCall(-1, (ushort)12, (ushort)3, "CombatResult", (int)currentOvercomeCombatResultType);
        GameDataBridge.AddMethodCall(-1, (ushort)12, (ushort)3, "CombatType", (int)combatConfig.CombatType);
        GameDataBridge.AddMethodCall(-1, 12, 2, string.Empty, 0);
        return true;
    }

    public static bool IsOvercomeLifeSkillCombat()
    {
        if (OvercomeLifeSkillCombatResultType < 0)
        {
            return false;
        }
        sbyte currentOvercomeLifeSkillCombatResultType = OvercomeLifeSkillCombatResultType;
        switch (currentOvercomeLifeSkillCombatResultType)
        {
            case 2:
            case 3:
                return false;
            default:
                currentOvercomeLifeSkillCombatResultType = (sbyte)UnityEngine.Random.Range(0, 2);
                break;
            case 0:
            case 1:
                break;
        }
        UI_GMWindow.Instance.LogLifeSkillCombatOvercome(currentOvercomeLifeSkillCombatResultType);
        GameDataBridge.AddMethodCall(-1, 12, 5, "WinState", currentOvercomeLifeSkillCombatResultType == 1);
        GameDataBridge.AddMethodCall(-1, 12, 2, string.Empty, 0);
        return true;
    }

    [GMObject(EGMGroup.CombatBase, 2000)]
    public static GameObject GetCombatEditor()
    {
        GameObject obj = UnityEngine.Object.Instantiate(UI_GMWindow.Instance.CombatEditor);
        GMCombatEditor editor = obj.GetComponent<GMCombatEditor>();
        UI_GMWindow instance = UI_GMWindow.Instance;
        instance.OnWorldDataReadyChild = (UI_GMWindow.WorldStateCallback)Delegate.Combine(instance.OnWorldDataReadyChild, new UI_GMWindow.WorldStateCallback(editor.OnWorldDataReady));
        UI_GMWindow instance2 = UI_GMWindow.Instance;
        instance2.OnLeaveWorldChild = (UI_GMWindow.WorldStateCallback)Delegate.Combine(instance2.OnLeaveWorldChild, new UI_GMWindow.WorldStateCallback(editor.OnLeaveWorld));
        obj.SetActive(value: true);
        editor.Init();
        return obj;
    }

    [GMFunc(EGMGroup.CombatBase, 0.25f, 1000)]
    public static void CricketForceWin()
    {
        try
        {
            UIElement.CricketCombat.UiBaseAs<UI_CricketCombat>().GmCmd_ForceDefeat(self: false);
        }
        catch (Exception)
        {
            UI_GMWindow.Instance.Log(LocalStringManager.Get("GM_Message_CricketForceResult_Failed_Msg"));
        }
    }

    [GMFunc(EGMGroup.CombatBase, 0.25f, 1000)]
    public static void CricketForceLose()
    {
        try
        {
            UIElement.CricketCombat.UiBaseAs<UI_CricketCombat>().GmCmd_ForceDefeat(self: true);
        }
        catch (Exception)
        {
            UI_GMWindow.Instance.Log(LocalStringManager.Get("GM_Message_CricketForceResult_Failed_Msg"));
        }
    }

    [GMFunc(EGMGroup.BuildBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.StringField, 0.2f)]
    public static void QueryBuildId(string buildName)
    {
        string ret = "";
        foreach (BuildingBlockItem config in (IEnumerable<BuildingBlockItem>)BuildingBlock.Instance)
        {
            if (config.Name.Contains(buildName))
            {
                ret += $"{config.Name.SetColor(Color.green)}-{config.TemplateId}  ";
            }
        }
        UI_GMWindow.Instance.Log(ret);
    }

    [GMFunc(EGMGroup.BuildBase, 0.25f, 1000)]
    [GMFuncArg(0, EWidgetType.IntField, 0.2f)]
    [GMFuncArg(1, EWidgetType.IntField, 0.2f)]
    public static void PlaceBuilding(short buildId, sbyte level)
    {
        if (!UIElement.BuildingArea.Exist)
        {
            UI_GMWindow.Instance.Log(LocalStringManager.Get("GM_Message_GMFunc_PlaceBuilding_Msg"));
            return;
        }
        UI_BuildingArea buildingArea = UIElement.BuildingArea.UiBaseAs<UI_BuildingArea>();
        buildingArea.StartPlacingBuilding(BuildingBlock.Instance[buildId], null, level, instantBuild: true);
    }

    [GMFunc(EGMGroup.BuildBase, 0.25f, 1000)]
    public static void RemoveBuilding()
    {
        if (!UIElement.BuildingManage.Exist)
        {
            UI_GMWindow.Instance.Log(LocalStringManager.Get("GM_Message_GMFunc_RemoveBuilding_Msg"));
            return;
        }
        UI_BuildingManage buildManage = UIElement.BuildingManage.UiBaseAs<UI_BuildingManage>();
        BuildingBlockKey key = buildManage.GetCurrentBuildingBlockKey();
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(9, 45, key, delegate (int offset, RawDataPool dataPool)
        {
            (short, BuildingBlockData) buildingData = (0, new BuildingBlockData());
            Serializer.Deserialize(dataPool, offset, ref buildingData);
            UI_BuildingArea uI_BuildingArea = UIElement.BuildingArea.UiBaseAs<UI_BuildingArea>();
            uI_BuildingArea.UpdateBuildingData(key, buildingData.Item2);
            uI_BuildingArea.UpdateRoad();
            UIElement.BuildingManage.UiBaseAs<UI_BuildingManage>().QuickHide();
        });
    }

    [GMFunc(EGMGroup.BuildBase, 0.25f, 1000)]
    public static void QueryLocalChickenCharacterFeatureInfo()
    {
        if (!UIElement.BuildingArea.Exist)
        {
            UI_GMWindow.Instance.Log(LocalStringManager.Get("GM_Message_GMFunc_PlaceBuilding_Msg"));
            return;
        }
        UI_BuildingArea buildingArea = UIElement.BuildingArea.UiBaseAs<UI_BuildingArea>();
        Location location = buildingArea.CurrentLocation;
        SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(9, 91, location, delegate (int offset, RawDataPool dataPool)
        {
            List<int> item = new List<int>();
            Serializer.Deserialize(dataPool, offset, ref item);
            List<GameData.Domains.Building.Chicken> localChicken = new List<GameData.Domains.Building.Chicken>();
            HashSet<short> localChickenFeatureIds = new HashSet<short>();
            int i = 0;
            int leni;
            for (leni = item.Count; i < leni; i++)
            {
                int arg = item[i];
                SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(9, 94, arg, delegate (int offset2, RawDataPool dataPool2)
                {
                    GameData.Domains.Building.Chicken item2 = default(GameData.Domains.Building.Chicken);
                    Serializer.Deserialize(dataPool2, offset2, ref item2);
                    localChicken.Add(item2);
                    localChickenFeatureIds.Add(Config.Chicken.Instance[item2.TemplateId].FeatureId);
                    if (localChicken.Count == leni)
                    {
                        HashSet<int> characterSet = new HashSet<int>();
                        int characterSetCount = 0;
                        for (int j = 0; j < leni; j++)
                        {
                            SingletonObject.getInstance<AsynchMethodDispatcher>().AsynchMethodCall(3, 2, localChicken[j].CurrentSettlementId, delegate (int offset3, RawDataPool dataPool3)
                            {
                                List<CharacterDisplayData> item3 = new List<CharacterDisplayData>();
                                Serializer.Deserialize(dataPool3, offset3, ref item3);
                                characterSet.UnionWith(item3.Select((CharacterDisplayData data) => data.CharacterId));
                                characterSetCount++;
                                if (characterSetCount == leni)
                                {
                                    IEnumerable<(DataUid, Type)> source = characterSet.Select((int characterId) => (new DataUid(4, 0, (ulong)characterId, 17u), typeof(List<short>)));
                                    UI_GMWindow.Instance.RequestGameDataReceiving(delegate (Dictionary<DataUid, object> data)
                                    {
                                        StringBuilder stringBuilder = new StringBuilder();
                                        foreach (KeyValuePair<DataUid, object> current in data)
                                        {
                                            ulong subId = current.Key.SubId0;
                                            short[] array = ((List<short>)current.Value).Where((short featureId) => localChickenFeatureIds.Contains(featureId)).ToArray();
                                            stringBuilder.AppendFormat("({0}: ", subId);
                                            if (array.Any())
                                            {
                                                for (int k = 0; k < array.Length; k++)
                                                {
                                                    if (k != 0)
                                                    {
                                                        stringBuilder.Append(", ");
                                                    }
                                                    stringBuilder.Append(CharacterFeature.Instance[array[k]].Name);
                                                }
                                                stringBuilder.Append(')');
                                            }
                                            else
                                            {
                                                stringBuilder.AppendFormat("{0})", LocalStringManager.Get(3346));
                                            }
                                        }
                                        UI_GMWindow.Instance.Log(stringBuilder.ToString());
                                    }, source.ToArray());
                                }
                            });
                        }
                    }
                });
            }
        });
    }

    [GMObject(EGMGroup.LifeSkill, 2000)]
    public static GameObject GetLifeSkillEditor()
    {
        GameObject obj = UnityEngine.Object.Instantiate(UI_GMWindow.Instance.LifeSkillEditor);
        GMLifeSkillEditor editor = obj.GetComponent<GMLifeSkillEditor>();
        UI_GMWindow instance = UI_GMWindow.Instance;
        instance.OnWorldDataReadyChild = (UI_GMWindow.WorldStateCallback)Delegate.Combine(instance.OnWorldDataReadyChild, new UI_GMWindow.WorldStateCallback(editor.OnWorldDataReady));
        UI_GMWindow instance2 = UI_GMWindow.Instance;
        instance2.OnLeaveWorldChild = (UI_GMWindow.WorldStateCallback)Delegate.Combine(instance2.OnLeaveWorldChild, new UI_GMWindow.WorldStateCallback(editor.OnLeaveWorld));
        obj.SetActive(value: true);
        editor.Init();
        return obj;
    }

    [GMObject(EGMGroup.CombatSkill, 1999)]
    public static GameObject GetCombatSkillEditor()
    {
        GameObject obj = UnityEngine.Object.Instantiate(UI_GMWindow.Instance.CombatSkillEditor);
        GMCombatSkillEditor editor = obj.GetComponent<GMCombatSkillEditor>();
        UI_GMWindow instance = UI_GMWindow.Instance;
        instance.OnWorldDataReadyChild = (UI_GMWindow.WorldStateCallback)Delegate.Combine(instance.OnWorldDataReadyChild, new UI_GMWindow.WorldStateCallback(editor.OnWorldDataReady));
        UI_GMWindow instance2 = UI_GMWindow.Instance;
        instance2.OnLeaveWorldChild = (UI_GMWindow.WorldStateCallback)Delegate.Combine(instance2.OnLeaveWorldChild, new UI_GMWindow.WorldStateCallback(editor.OnLeaveWorld));
        obj.SetActive(value: true);
        editor.Init();
        return obj;
    }

    public static void Reset()
    {
        _lockTime = false;
        _teleportMove = false;
        CombatSkillCharId = -1;
        LifeSkillCharId = -1;
        UI_GMWindow.Instance?.Reset();
    }
}
