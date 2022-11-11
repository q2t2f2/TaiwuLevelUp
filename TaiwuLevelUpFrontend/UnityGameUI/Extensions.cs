using System;
using UnityEngine;

namespace UnityGameUI
{
    // Token: 0x02000004 RID: 4
    internal static class Extensions
    {
        // Token: 0x06000009 RID: 9 RVA: 0x00002050 File Offset: 0x00000250
        public static Color32 HexToColor(this string hexString)
        {
            string tmp = hexString;
            if (tmp.IndexOf('#') != -1)
            {
                tmp = tmp.Replace("#", "");
            }
            byte a = 0;
            byte r = Convert.ToByte(tmp.Substring(0, 2), 16);
            byte g = Convert.ToByte(tmp.Substring(2, 2), 16);
            byte b = Convert.ToByte(tmp.Substring(4, 2), 16);
            if (tmp.Length == 8)
            {
                a = Convert.ToByte(tmp.Substring(6, 2), 16);
            }
            return new Color32(r, g, b, a);
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000020D4 File Offset: 0x000002D4
        public static int ConvertToIntDef(this string input, int defaultValue)
        {
            int result;
            if (int.TryParse(input, out result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}
