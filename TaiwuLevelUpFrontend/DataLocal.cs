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
    internal class DataLocal
    {
        private string fileName = "TaiwuLevelUp_Data.sav";
        //private MainAttributes extraMainAttributes;
        private int exp = 0;
        private int level = 1;

        public int Level { get => level; set => level = value; }
        public int Exp { get => exp; set => SetExp(value); }

        public static string GetArchiveDirPath(bool isTestVersion = true)
        {
            string currentDir = Environment.CurrentDirectory;

            string path = Path.Combine(currentDir, isTestVersion ? "Save_test" : "Save");
            return Path.GetFullPath(path);
        }

        public void SetExp(int exp)
        {
            this.exp = exp;
            ColcLevel();
        }

        public void ColcLevel()
        {
            int nextExp = GetNextExp();
            while(Exp >= nextExp)
            {
                level++;
                nextExp = GetNextExp();
            }
        }

        private int GetNextExp()
        {
            int step = 200;
            int curLevelUpExp = 0;// level * 200;

            for (int i = 1; i < level + 1; i++)
            {
                curLevelUpExp += i * step;
            }
            return curLevelUpExp * 2;
        }

        private string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"exp={Exp}");
            sb.AppendLine($"level={Level}");
            return sb.ToString();
        }

        private void Deserialize(string[] dataString)
        {
            foreach (var s in dataString)
            {
                string[] arr = s.Split('=');
                string paraName = arr[0];
                if (int.TryParse(arr[1], out int value))
                {
                    AccessTools.Field(this.GetType(), paraName).SetValue(this, value);
                }
                else
                {
                    AdaptableLog.Error($"解析{paraName}数据出错: {arr[1]} ");
                }
            }
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
