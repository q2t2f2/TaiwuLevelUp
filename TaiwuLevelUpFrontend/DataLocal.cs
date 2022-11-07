using GameData.Domains;
using GameData.Domains.Character;
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
        private int expRequireStep = 200;

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
                return expRequireStep * level;
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
            string currentDir = Environment.CurrentDirectory;
            AdaptableLog.Info($"当前目录：{currentDir}");
            string path = Path.Combine(currentDir, isTestVersion ? "Save_test" : "Save");
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
                level++;
                nextExp = GetExpNeed(level);
            }
            if (oldLevel != level)
            {
                ColcMainAttribute();
                ColcNeili();
            }
        }
        private void ColcMainAttribute()
        {
            extraMainAttribute = 0;
            for (int i = 0; i < Level; i++)
            {
                extraMainAttribute += (short)Level;
            }
        }
        private void ColcNeili()
        {
            extraNeili = 0;
            for (int i = 0; i < Level; i++)
            {
                extraNeili += (short)(Level * 2);
            }
        }

        private int GetExpNeed(int level)
        {
            int expNeed = expRequireStep;

            if (level <= 1) return expNeed;
            for (int i = 1; i < level; i++)
            {
                expNeed += expRequireStep * i;
            }
            return expNeed;
        }

        private string Serialize()
        {
            StringBuilder sb = new StringBuilder();
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
            }
        }
        public void SaveData()
        {
            string path = Path.Combine(GetArchiveDirPath(), fileName);
            File.WriteAllText(path, Serialize());
        }
    }
}
