using GameData.Domains;
using GameData.Domains.Taiwu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SXDZD
{
    internal class Character
    {
        public int TaiwuCharId { get; private set; }
        public Character(int taiwuCharId)
        {
            TaiwuCharId = taiwuCharId;
        }

    }
}
