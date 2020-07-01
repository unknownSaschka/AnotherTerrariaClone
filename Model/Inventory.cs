using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ITProject.Model
{
    public class Inventory
    {
        private Item[,] _item;
        private int _inventoryWidth = 10;
        private int _inventoryHeight = 4;

        public Inventory()
        {
            _item = new Item[_inventoryWidth, _inventoryHeight];
        }

        public Inventory(Item[,] items)
        {
            _item = items;
        }

        public Item[,] GetSaveInv()
        {
            return _item;
        }

        /// <summary>
        /// Gibt true zurück, falls das Item ins Inventar platziert wurde. False, wenn es keinen Platz gefunden hat.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddItemUnsorted(Item item)
        {
            int EmptyX = -1, EmptyY = -1;
            int StackableX = -1, StackableY = -1;

            for(int ix = 0; ix < _inventoryWidth; ix++)
            {
                for(int iy = 0; iy < _inventoryHeight; iy++)
                {
                    if(EmptyX == -1 && _item[ix, iy] == null)
                    {
                        EmptyX = ix;
                        EmptyY = iy;
                    }

                    if (_item[ix, iy] == null) continue;

                    if(StackableX == -1 && _item[ix, iy].ID == item.ID && MainModel.Item[item.ID].Stackable)
                    {
                        StackableX = ix;
                        StackableY = iy;
                    }
                }
            }

            if(StackableX != -1)
            {
                int amount = _item[StackableX, StackableY].Amount + item.Amount;
                if(amount > 99)
                {
                    _item[StackableX, StackableY].Amount = 99;
                    item.Amount = (short)(99 - amount);
                }
                else
                {
                    _item[StackableX, StackableY].Amount += item.Amount;
                    return true;
                }
            }

            if(EmptyX != -1)
            {
                _item[EmptyX, EmptyY] = item;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gibt ein Item zurück, falls dieses ander zu setzenden Stelle war
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        public Item SetItem(int x, int y, Item newItem)
        {
            if(_item[x, y] != null)
            {
                Item toReturn = _item[x, y];
                _item[x, y] = newItem;
                return toReturn;
            }
            else
            {
                _item[x, y] = newItem;
                return null;
            }
        }

        //Gibt entweder 0 zurück, wenn alles abgezogen werden konnte oder größer 0 wenn nicht alles abgezogen werden konnte
        public short RemoveItemAmount(Item item)
        {

            for(int iy = 0; iy < _inventoryHeight; iy++)
            {
                for(int ix = 0; ix < _inventoryWidth; ix++)
                {
                    if(_item[ix, iy].ID == item.ID)
                    {
                        if(_item[ix, iy].Amount < item.Amount)
                        {
                            item.Amount -= _item[ix, iy].Amount;
                            _item[ix, iy] = null;
                        }
                        else if(_item[ix, iy].Amount == item.Amount)
                        {
                            _item[ix, iy] = null;
                            return 0;
                        }
                        else
                        {
                            _item[ix, iy].Amount -= item.Amount;
                            return 0;
                        }
                    }
                }
            }

            return item.Amount;
        }

        //-1 wenn kein Item an dem Platz oder Fehler, 0 wenn abgezogen werden konnte, über 0 wenn nicht alles abgezogen werden konnte
        public short RemoveItemAmount(Item item, int x, int y)
        {
            if (_item[x, y] == null)
            {
                return -1;
            }

            if(_item[x, y].ID == item.ID)
            {
                if (_item[x, y].Amount < item.Amount)
                {
                    item.Amount -= _item[x, y].Amount;
                    _item[x, y] = null;
                    return item.Amount;
                }
                else if(_item[x, y].Amount == item.Amount)
                {
                    _item[x, y] = null;
                    return 0;
                }
                else
                {
                    _item[x, y].Amount -= item.Amount;
                }
            }

            return -1;
        }

        public void RemoveItem(int x, int y)
        {
            _item[x, y] = null;
        }

        public Item GetItem(int x, int y)
        {
            return _item[x, y];
        }

        public ushort GetItemID(int x, int y)
        {
            if(_item[x, y] == null)
            {
                return 0;
            }
            else
            {
                return _item[x, y].ID;
            }
        }
    }
}
