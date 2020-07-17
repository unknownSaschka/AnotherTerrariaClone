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

        public Item(ushort id, short amount)
        {
            ID = id;
            Amount = amount;
        }
    }

    public class ItemInfo
    {
        public ushort ID;
        public string Name;
        public float LightBlocking;
        public bool Stackable;
        public bool Placable;
        public bool Walkable;
        public bool Fluid;
        public bool LightSource;
        public bool HasInventory;

        public ItemInfo(ushort id, float lightBlocking, string name, bool stackable, bool placable, bool walkable, bool fluid, bool lightSource, bool hasInventory)
        {
            ID = id;
            LightBlocking = lightBlocking;
            Name = name;
            Stackable = stackable;
            Placable = placable;
            Walkable = walkable;
            Fluid = fluid;
            LightSource = lightSource;
            HasInventory = hasInventory;
        }
    }

    public class ItemJSON
    {
        public string Name;
        public float LightBlocking;
        public bool Stackable;
        public bool Placable;
        public bool Walkable;           //Transparent = true, Solide = false
        public bool Fluid;
        public bool LightSource;
        public bool HasInventory;

        public ItemJSON(string name, float lightBlocking, bool stackable, bool placable, bool walkable, bool fluid, bool lightSource, bool hasInventory)
        {
            Name = name;
            LightBlocking = lightBlocking;
            Stackable = stackable;
            Placable = placable;
            Walkable = walkable;
            Fluid = fluid;
            LightSource = lightSource;
            HasInventory = hasInventory;
        }
    }
}
