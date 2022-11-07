// ScriptTrainer.Scripts
using System;
using System.Collections.Generic;
using GameData.Domains.Character;
using GameData.Domains.Character.Display;
using GameData.GameDataBridge;
using HarmonyLib;
using ScriptTrainer;
using UnityGameUI;

public static class Scripts
{
    public static int playerId => SingletonObject.getInstance<BasicGameData>().TaiwuCharId;

    public static int CurCharacterId
    {
        get
        {
            try
            {
                return UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>().CurCharacterId;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

    public static string CurCharacterName
    {
        get
        {
            UI_CharacterMenu uI_CharacterMenu = UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>();
            string name = "";
            foreach (string item in uI_CharacterMenu.Names)
            {
                name += item;
            }
            return name;
        }
    }

    public static void AddPlayerMoney()
    {
        UIWindows.SpawnInputDialog("您想要添加多少现金？", "添加", "1000", delegate (string count)
        {
            GMFunc.GetAdvancedResource(count.ConvertToIntDef(1000), money: true, authority: false, exp: false);
        });
    }

    public static void AddPlayerAuthority()
    {
        UIWindows.SpawnInputDialog("您想要添加多少威望？", "添加", "1000", delegate (string count)
        {
            GMFunc.GetAdvancedResource(count.ConvertToIntDef(1000), money: false, authority: true, exp: false);
        });
    }

    public static void AddPlayerGoldIron()
    {
        UIWindows.SpawnInputDialog("您想要添加多少金铁？", "添加", "1000", delegate (string count)
        {
            AddResource(2, count);
        });
    }

    public static void AddPlayerJadeStone()
    {
        UIWindows.SpawnInputDialog("您想要添加多少玉石？", "添加", "1000", delegate (string count)
        {
            AddResource(3, count);
        });
    }

    public static void AddPlayerCloth()
    {
        UIWindows.SpawnInputDialog("您想要添加多少织物？", "添加", "1000", delegate (string count)
        {
            AddResource(4, count);
        });
    }

    public static void AddPlayerMedicine()
    {
        UIWindows.SpawnInputDialog("您想要添加多少药材？", "添加", "1000", delegate (string count)
        {
            AddResource(5, count);
        });
    }

    public static void AddPlayerWood()
    {
        UIWindows.SpawnInputDialog("您想要添加多少木材？", "添加", "1000", delegate (string count)
        {
            AddResource(1, count);
        });
    }

    public static void AddPlayerFood()
    {
        UIWindows.SpawnInputDialog("您想要添加多少食材？", "添加", "1000", delegate (string count)
        {
            AddResource(0, count);
        });
    }

    private static void AddResource(sbyte type, string count)
    {
        Traverse.Create(typeof(GMFunc)).Method("AddResource", type, count.ConvertToIntDef(1000)).GetValue();
    }

    public static void ChangeInjury(bool isInnerInjury, sbyte bodyPartType, sbyte delta)
    {
        GameDataBridge.AddMethodCall(-1, 4, 76, playerId, isInnerInjury, bodyPartType, delta);
    }

    public static void ChangePoisoned(sbyte poisonType, int changeValue)
    {
        GameDataBridge.AddMethodCall(-1, 4, 77, playerId, poisonType, changeValue);
    }

    public static void ChangeSpiritualDebt(int areaId, int spiritualDebt)
    {
        GMFunc.ChangeSpiritualDebt(areaId, spiritualDebt);
    }

    public static void ChangeAge(int charid = 0)
    {
        if (charid == 0)
        {
            UIWindows.SpawnInputDialog("您想将自己修改为多少岁？", "设置", "18", delegate (string count)
            {
                GMFunc.EditActualAge(playerId, count.ConvertToIntDef(18));
            });
        }
        else
        {
            UIWindows.SpawnInputDialog($"您想将{charid}修改为多少岁？", "设置", "18", delegate (string count)
            {
                GMFunc.EditActualAge(charid, count.ConvertToIntDef(18));
            });
        }
    }

    public static void ChangeHp()
    {
        UIWindows.SpawnInputDialog("您想将血量设置为多少？", "设置", "200", delegate (string count)
        {
            short value = (short)count.ConvertToIntDef(200);
            GameDataBridge.AddDataModification(4, 0, (ulong)playerId, 19u, value);
            GameDataBridge.AddDataModification(4, 0, (ulong)playerId, 20u, value);
        });
    }

    public static void ChangeMainAttributes(short[] attributes, int charId = 0)
    {
        if (charId == 0)
        {
            GameDataBridge.AddDataModification(4, 0, (ulong)playerId, 18u, new MainAttributes(attributes));
            GameDataBridge.AddDataModification(4, 0, (ulong)playerId, 43u, new MainAttributes(attributes));
        }
        else
        {
            GameDataBridge.AddDataModification(4, 0, (ulong)charId, 18u, new MainAttributes(attributes));
            GameDataBridge.AddDataModification(4, 0, (ulong)charId, 43u, new MainAttributes(attributes));
        }
    }

    public static void ChangeNeiLi(int[] allocation)
    {
        Debug.Log($"{allocation[0]}-{allocation[1]}-{allocation[2]}-{allocation[3]}");
        GMFunc.EditExtraNeiliAllocation(playerId, allocation[0], allocation[1], allocation[2], allocation[3]);
    }

    public static void ChangeBaseMorality()
    {
        UIWindows.SpawnInputDialog("您想修改道德为多少？", "设置", "18", delegate (string count)
        {
            GMFunc.EditBaseMorality(playerId, count.ConvertToIntDef(18));
        });
    }

    public static void GetItem(sbyte itemType, int itemId, int count)
    {
        if (itemType == 99)
        {
            GameDataBridge.AddMethodCall(-1, 9, 85, itemId, playerId);
        }
        else
        {
            GMFunc.GetItem(playerId, count, itemType, (short)itemId, null);
        }
    }

    public static void ChangeFavor(int charId1, int charId2)
    {
        UIWindows.SpawnInputDialog("您想修改好感为多少？", "设置", "6000", delegate (string count)
        {
            GameDataBridge.AddMethodCall(-1, 4, 58, charId1, charId2, (short)count.ConvertToIntDef(18));
        });
    }

    public static void Kidnap(int charId)
    {
        GameDataBridge.AddMethodCall(-1, 4, 68, playerId, charId);
    }

    public static void Relationship(int charIdA, int charIdB)
    {
        List<string> options = new List<string>
        {
            "一般", "父母", "子女", "手足", "义父母", "养子", "养手足", "结义金兰", "结为夫妻", "师父",
            "徒弟", "朋友", "敬仰之人", "仇人"
        };
        List<ushort> o_type = new List<ushort>
        {
            0, 1, 2, 4, 8, 16, 256, 512, 1024, 2048,
            4096, 8192, 16384, 32768
        };
        UIWindows.SpawnDropdownDialog($"你想让{charIdB}成为你的什么？", "修改", options, delegate (int call)
        {
            GameDataBridge.AddMethodCall(-1, 4, 13, charIdA, charIdB, o_type[call]);
        });
    }

    public static string GetCharacterName(int charId)
    {
        bool isTaiwu = playerId == charId;
        NameRelatedData nameRelatedData = default(NameRelatedData);
        var (item, item2) = NameCenter.GetMonasticTitleOrName(ref nameRelatedData, isTaiwu);
        return item + item2;
    }

    public static void Test()
    {
        Debug.Log(CurCharacterId.ToString());
    }
}
