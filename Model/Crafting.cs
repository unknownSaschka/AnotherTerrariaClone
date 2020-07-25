using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    public class Crafting
    {
        public List<CraftingRecipie> UpdateCraftableRecipies(Inventory playerInventory)
        {
            List<CraftingRecipie> craftableRecipies = new List<CraftingRecipie>();
            Dictionary<ushort, int> availableItems = new Dictionary<ushort, int>();

            //Zusammenfassen aller verfügbaren Items in Inventar in ein Dictionary
            foreach(Item item in playerInventory.GetSaveInv())
            {
                if (item == null) continue;

                if (availableItems.ContainsKey(item.ID))
                {
                    availableItems[item.ID] += item.Amount;
                    if (availableItems[item.ID] > 99) availableItems[item.ID] = 99;
                }
                else
                {
                    availableItems.Add(item.ID, item.Amount);
                }
            }

            foreach(CraftingRecipie recipie in MainModel.CraftingRecipies)
            {
                int available = 0;

                foreach(Item item in recipie.NeededItems)
                {
                    if (availableItems.ContainsKey(item.ID))
                    {
                        if(availableItems[item.ID] >= item.Amount) available++;
                    }
                }

                if(available >= recipie.NeededItems.Length)
                {
                    craftableRecipies.Add(recipie);
                }
            }                           

            return craftableRecipies;
        }
    }

    [Serializable]
    public class CraftingRecipie
    {
        public Item[] NeededItems;
        public Item ResultItem;

        public CraftingRecipie(Item resultItem, Item[] neededItems)
        {
            ResultItem = resultItem;
            NeededItems = neededItems;
        }
    }
}
