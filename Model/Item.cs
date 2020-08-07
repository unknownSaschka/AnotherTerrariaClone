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
        public int Amount;

        public Item(ushort id, int amount)
        {
            ID = id;
            Amount = amount;
        }
    }

    public abstract class ItemInfo
    {
        public ushort ID;
        public string Name;
        public bool Stackable;
        public bool Placable;
    }

    public class ItemInfoWorld : ItemInfo
    {
        public float LightBlocking;
        
        public bool Walkable;
        public bool Fluid;
        public bool LightSource;
        public bool HasInventory;
        public int NeededToolLevel;
        public int MiningDuration;
        public ItemInfoTools.ItemToolType NeededToolType;

        public ItemInfoWorld(ushort id, string name, float lightBlocking, bool stackable, bool placable, bool walkable, bool fluid, bool lightSource, bool hasInventory, int neededToolLevel, int miningDuration, ItemInfoTools.ItemToolType neededToolType)
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
            NeededToolLevel = neededToolLevel;
            MiningDuration = miningDuration;
            NeededToolType = neededToolType;
        }
    }

    public class ItemInfoTools : ItemInfo
    {
        public enum ItemToolType { Pickaxe, Axe, Hammer, Sword, Hand, None }

        public ItemToolType ToolType;
        public int ToolLevel;
        public float MiningDuration;

        public ItemInfoTools(ushort id, string name, ItemToolType toolType, int toolLevel, float miningDuration, bool placable, bool stackable)
        {
            ID = id;
            Name = name;
            ToolType = toolType;
            ToolLevel = toolLevel;
            MiningDuration = miningDuration;
            Stackable = stackable;
            Placable = placable;
        }
    }

    public class ItemJSON
    {
        public Dictionary<ushort, ItemInfoWorld> WorldItems;
        public Dictionary<ushort, ItemInfoTools> ToolItems;

        public ItemJSON(Dictionary<ushort, ItemInfoWorld> worldItems, Dictionary<ushort, ItemInfoTools> toolItems)
        {
            WorldItems = worldItems;
            ToolItems = toolItems;
        }
    }

    /*
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

        public ItemJSON(string name, )
    
    }
    */
}
