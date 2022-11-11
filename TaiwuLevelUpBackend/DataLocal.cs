using GameData.ArchiveData;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace SXDZD
{
    public class DataLocal
    {
        private static DataLocal instance;
        public static DataLocal Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataLocal();
                }
                return instance;
            }
        }

        private string fileName = "TaiwuLevelUp_Data.sav";
        public static int ExpRequireStep = 200;

        private int exp = 0;
        private int nextExp = 0;
        private int level = 1;

        private short extraMainAttribute;
        private short extraNeili;
        private int totalExp = 0;



        public DataLocal()
        {
        }

        public int Level { get => level; set => level = value < 1 ? 1 : value; }
        public int Exp { get => exp; set => SetExp(value); }

        public short ExtraMainAttribute => extraMainAttribute;
        public short ExtraNeili => extraNeili;

        public int ExpNeed
        {
            get
            {
                return ExpRequireStep * level;
            }
        }

        public int CurrrentExp
        {
            get
            {
                if (Level == 1)
                {
                    return Exp;
                }
                return Exp - GetExpNeed(level - 1);
            }
        }

        public static string GetArchiveDirPath(bool isTestVersion = true)
        {
            AdaptableLog.Info($"当前存档：Common.GetCurrArchiveId()={Common.GetCurrArchiveId() + 1}");

            string path = Path.Combine(Common.ArchiveBaseDir, $"world_{Common.GetCurrArchiveId() + 1}");

            AdaptableLog.Info($"存档目录目录：{path}");

            return Path.GetFullPath(path);
        }

        public void SetExp(int exp)
        {
            this.exp = exp;
            ColcLevel();
        }

        private void ColcLevel()
        {
            nextExp = GetExpNeed(level);
            int oldLevel = level;
            while (Exp >= nextExp)
            {
                LevelUp();
                nextExp = GetExpNeed(level);
            }
            if (oldLevel != level)
            {
                ColcMainAttribute();
                SetTaiwuMainAttributeFull();
            }
        }

        private void LevelUp()
        {
            level++;
            var taiwu = DomainManager.Taiwu.GetTaiwu();
            int extraNeili = 5 + level * 2;
            AdaptableLog.Error($"升级增加内力{extraNeili}");
            taiwu.ChangeExtraNeili(DataContextManager.GetCurrentThreadDataContext(), extraNeili);
        }
        private void SetTaiwuMainAttributeFull()
        {
            var taiwu = DomainManager.Taiwu.GetTaiwu();
            taiwu.SetCurrMainAttributes(taiwu.GetMaxMainAttributes(), DataContextManager.GetCurrentThreadDataContext());
        }

        private void ColcMainAttribute()
        {
            extraMainAttribute = 0;
            for (int i = 0; i < Level; i++)
            {
                extraMainAttribute += (short)Level;
            }
        }
        private int GetExpNeed(int level)
        {
            int expNeed = ExpRequireStep;

            if (level <= 1) return expNeed;
            for (int i = 1; i < level; i++)
            {
                expNeed += ExpRequireStep * i;
            }
            return expNeed;
        }

        private string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"level={level}");
            sb.AppendLine($"exp={exp}");
            return sb.ToString();
        }

        private void Deserialize(string[] dataString)
        {
            foreach (var s in dataString)
            {
                string[] arr = s.Split('=');
                string paraName = Regex.Replace(arr[0], "\\s", "");
                string valueName = Regex.Replace(arr[1], "\\s", "");
                if (int.TryParse(valueName, out int value))
                {
                    AccessTools.Field(this.GetType(), paraName).SetValue(this, value);
                }
                else
                {
                    AdaptableLog.Error($"解析{paraName}数据出错: {arr[1]} ");
                }
            }
            ColcLevel();
        }
        public void LoadData()
        {
            string path = Path.Combine(GetArchiveDirPath(), fileName);
            if (File.Exists(path))
            {
                string[] dataStr = File.ReadAllLines(path);
                Deserialize(dataStr);
                AdaptableLog.Info($"加载经验值成功{path}，Level:{Level}  Exp:{Exp}点。");
            }
        }
        public void SaveData()
        {
            string dir = GetArchiveDirPath();
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = Path.Combine(dir, fileName);
            AdaptableLog.Info($"保存经验值到本地{path},Level:{Level}  Exp:{Exp}点。");
            File.WriteAllText(path, Serialize());
        }
    }
}
