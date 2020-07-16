﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static ITProject.Logic.GameExtentions;

namespace ITProject.Model
{
    public class Inventory
    {
        private Item[,] _item;

        private int _inventoryWidth = 10;
        private int _inventoryHeight = 4;
        private short _maxItemStack = 99;

        public Item ActiveHoldingItem { get; internal set; }

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
            InventoryPosition stackableInvPos = GetNextStackable(item.ID);
            if(stackableInvPos != null)
            {
                short restAmount = (short) (_item[stackableInvPos.X, stackableInvPos.Y].Amount + item.Amount);

                if(restAmount > _maxItemStack)
                {
                    _item[stackableInvPos.X, stackableInvPos.Y].Amount = _maxItemStack;
                    restAmount -= _maxItemStack;
                    AddItemUnsorted(new Item(item.ID, restAmount));
                }
                else
                {
                    _item[stackableInvPos.X, stackableInvPos.Y].Amount = restAmount;
                    return true;
                }
            }

            InventoryPosition emptyInvPos = GetNextEmpty();
            if(emptyInvPos != null)
            {
                _item[emptyInvPos.X, emptyInvPos.Y] = item;
            }

            return false;
        }

        private InventoryPosition GetNextEmpty()
        {
            for(int iy = 0; iy < _inventoryHeight; iy++)
            {
                for(int ix = 0; ix < _inventoryWidth; ix++)
                {
                    if(_item[ix, iy] == null)
                    {
                        return new InventoryPosition(ix, iy);
                    }
                }
            }

            return null;
        }

        private InventoryPosition GetNextStackable(ushort itemID)
        {
            for (int iy = 0; iy < _inventoryHeight; iy++)
            {
                for (int ix = 0; ix < _inventoryWidth; ix++)
                {
                    if (_item[ix, iy] == null) continue;
                    
                    if(_item[ix, iy].ID == itemID && _item[ix, iy].Amount < _maxItemStack)
                    {
                        return new InventoryPosition(ix, iy);
                    }
                }
            }

            return null;
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

        public void LeftClick(int x, int y)
        {
            if(ActiveHoldingItem == null)
            {
                ActiveHoldingItem = _item[x, y];
                _item[x, y] = null;
            }
            else
            {
                if(_item[x, y] == null)
                {
                    _item[x, y] = ActiveHoldingItem;
                    ActiveHoldingItem = null;
                }
                else
                {
                    if(_item[x, y].ID == ActiveHoldingItem.ID)
                    {
                        _item[x, y].Amount += ActiveHoldingItem.Amount;

                        if(_item[x, y].Amount > _maxItemStack)
                        {
                            ActiveHoldingItem.Amount = (short)(_maxItemStack - _item[x, y].Amount);
                            _item[x, y].Amount = _maxItemStack;
                        }
                        else
                        {
                            ActiveHoldingItem = null;
                        }
                    }
                    else
                    {
                        Item hold = _item[x, y];
                        _item[x, y] = ActiveHoldingItem;
                        ActiveHoldingItem = hold;
                    }
                }
            }
        }

        public void RightClick(int x, int y)
        {
            if(ActiveHoldingItem == null)
            {
                //Nichts passiert mal (Kommt auf UX an)
                ActiveHoldingItem = new Item(_item[x, y].ID, (short)(_item[x, y].Amount / 2));
                _item[x, y].Amount -= ActiveHoldingItem.Amount;
            }
            else
            {
                if(_item[x, y] == null)
                {
                    _item[x, y] = new Item(ActiveHoldingItem.ID, 1);

                    if(ActiveHoldingItem.Amount < 1)
                    {
                        ActiveHoldingItem = null;
                    }
                    else
                    {
                        ActiveHoldingItem.Amount -= 1;
                        if(ActiveHoldingItem.Amount <= 0)
                        {
                            ActiveHoldingItem = null;
                        }
                    }
                }
                else
                {
                    if(_item[x, y].ID == ActiveHoldingItem.ID)
                    {
                        if(_item[x, y].Amount < _maxItemStack)
                        {
                            _item[x, y].Amount += 1;
                            ActiveHoldingItem.Amount -= 1;

                            if(ActiveHoldingItem.Amount <= 0)
                            {
                                ActiveHoldingItem = null;
                            }
                        }
                    }
                    else
                    {
                        //Nichts passiert (Kommt auf UX an)
                    }
                }
            }
        }

        public void RemoveHoldItem()
        {
            ActiveHoldingItem = null;
        }
    }

    public class InventoryPosition
    {
        public int X, Y;

        public InventoryPosition(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}