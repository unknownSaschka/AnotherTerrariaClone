using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    [Serializable]
    public class Item
    {
        public ushort ID;
        public short Amount;

        public Item(ushort id, byte amount)
        {
            ID = id;
            Amount = amount;
        }
    }

    public class ItemInfo
    {
        public ushort ID;
        public string Name;
        public bool Stackable;
        public bool Placable;
        public bool Walkable;
        public bool Fluid;

        public ItemInfo(ushort id, string name, bool stackable, bool placable, bool walkable, bool fluid)
        {
            ID = id;
            Name = name;
            Stackable = stackable;
            Placable = placable;
            Walkable = walkable;
            Fluid = fluid;
        }
    }

    public class ItemJSON
    {
        public string Name;
        public bool Stackable;
        public bool Placable;
        public bool Walkable;           //Transparent = true, Solide = false
        public bool Fluid;

        public ItemJSON(string name, bool stackable, bool placable, bool walkable, bool fluid)
        {
            Name = name;
            Stackable = stackable;
            Placable = placable;
            Walkable = walkable;
            Fluid = fluid;
        }
    }
}
