using ITProject.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    public class CollisionHandler
    {
        private ModelManager _modelManager;
        public CollisionHandler(ModelManager modelManager)
        {
            _modelManager = modelManager;
        }

        public Vector2 CheckCollisionX(Vector2 position, Vector2 size, ref float velocity, GameObject gameObject)
        {
            //Idee wieder mit Blöcke haben auch Hitboxen
            World world = _modelManager.World;
            Hitbox playerHitbox = new Hitbox(position, size, Hitbox.HitboxType.Player);
            Vector2 newPlayerPosition = new Vector2(position.X, position.Y);
            Vector2 newPlayerPositionSlope = new Vector2(position.X, position.Y);
            bool slope = false;
            float oldVelocity = velocity;

            for(int iy = (int)playerHitbox.MinY; iy <= (int)playerHitbox.MaxY; iy++)
            {
                for(int ix = (int)playerHitbox.MinX; ix <= (int)playerHitbox.MaxX; ix++)
                {
                    if (!GameExtentions.CheckIfInBound(ix, iy, world.WorldSize)) continue;
                    if (((ItemInfoWorld)MainModel.Item[_modelManager.World.GetWorld[ix, iy]]).Walkable) continue;

                    Hitbox blockHitbox = new Hitbox(new Vector2(ix, iy), new Vector2(1.0f, 1.0f), Hitbox.HitboxType.Block);
                    if (!Intersects(playerHitbox, blockHitbox)) continue;

                    //Prüfen, ob geeignet für Stairs
                    if (((ItemInfoWorld)MainModel.Item[_modelManager.World.GetWorld[(int)blockHitbox.Position.X, (int)blockHitbox.Position.Y + 1]]).Walkable &&
                        ((ItemInfoWorld)MainModel.Item[_modelManager.World.GetWorld[(int)blockHitbox.Position.X, (int)blockHitbox.Position.Y + 2]]).Walkable &&
                        ((ItemInfoWorld)MainModel.Item[_modelManager.World.GetWorld[(int)blockHitbox.Position.X, (int)blockHitbox.Position.Y + 3]]).Walkable &&
                        position.Y > blockHitbox.MaxY && gameObject.Grounded)
                    {
                        newPlayerPositionSlope.Y = blockHitbox.MaxY + size.Y / 2;
                        slope = true;
                        //return newPlayerPositionSlope;
                    }

                    if (velocity > 0)
                    {
                        //Rechts
                        newPlayerPosition.X = blockHitbox.MinX - size.X / 2;
                        velocity = 0;
                    }
                    else if(velocity < 0)
                    {
                        //Links
                        newPlayerPosition.X = blockHitbox.MaxX + size.X / 2;
                        velocity = 0;
                    }
                }
            }

            //Falls eine Slope/Treppe entdeckt wurde, soll die Playerhitbox auf der Slope auch nochmal geprüft werden
            if (slope == true)
            {
                bool trueSlope = true;

                Hitbox playerHitboxSlope = new Hitbox(newPlayerPositionSlope, size, Hitbox.HitboxType.Player);

                for (int iy = (int)playerHitboxSlope.MinY; iy <= (int)playerHitboxSlope.MaxY; iy++)
                {
                    for (int ix = (int)playerHitboxSlope.MinX; ix <= (int)playerHitboxSlope.MaxX; ix++)
                    {
                        if (!GameExtentions.CheckIfInBound(ix, iy, world.WorldSize)) continue;
                        if (((ItemInfoWorld)MainModel.Item[_modelManager.World.GetWorld[ix, iy]]).Walkable) continue;

                        Hitbox blockHitbox = new Hitbox(new Vector2(ix, iy), new Vector2(1.0f, 1.0f), Hitbox.HitboxType.Block);
                        if (Intersects(playerHitboxSlope, blockHitbox))
                        {
                            trueSlope = false;
                        }
                    }
                }

                if (trueSlope)
                {
                    velocity = oldVelocity;
                    return newPlayerPositionSlope;
                }
            }
            
            return newPlayerPosition;
        }

        public Vector2 CheckCollisionY(Vector2 position, Vector2 size, ref float velocity, GameObject gameObject)
        {
            World world = _modelManager.World;
            Hitbox playerHitbox = new Hitbox(position, size, Hitbox.HitboxType.Player);
            Vector2 newPlayerPosition = new Vector2(position.X, position.Y);
            gameObject.SetGrounded(false);

            for (int iy = (int)playerHitbox.MinY; iy <= (int)playerHitbox.MaxY; iy++)
            {
                for (int ix = (int)playerHitbox.MinX; ix <= (int)playerHitbox.MaxX; ix++)
                {
                    if (!GameExtentions.CheckIfInBound(ix, iy, world.WorldSize)) continue;
                    ushort itemID = _modelManager.World.GetWorld[ix, iy];
                    if (((ItemInfoWorld)MainModel.Item[itemID]).Walkable) continue;

                    Hitbox blockHitbox = new Hitbox(new Vector2(ix, iy), new Vector2(1.0f, 1.0f), Hitbox.HitboxType.Block);
                    if (!Intersects(playerHitbox, blockHitbox)) continue;

                    if (velocity > 0)
                    {
                        //Oben
                        newPlayerPosition.Y = blockHitbox.MinY - size.Y / 2;
                        velocity = 0;
                    }
                    else if (velocity < 0)
                    {
                        //Unten
                        newPlayerPosition.Y = blockHitbox.MaxY + size.Y / 2;
                        gameObject.SetGrounded(true);
                        velocity = 0;
                    }
                }
            }

            return newPlayerPosition;
        }

        public void CheckPlayerWithDroppedItems(Player player)
        {
            var itemsList = _modelManager.World.GetDroppedItemsList();

            var itemsToPickup = from item in itemsList
                       where Intersects(new Hitbox(player.Position, player.Size, Hitbox.HitboxType.Player), new Hitbox(item.Position, item.Size, Hitbox.HitboxType.Player))
                       select item;
            

            foreach(WorldItem item in itemsToPickup.ToArray())
            {
                if (item.LayingTime < 3.0f) continue;
                player.ItemInventory.AddItemUnsorted(item.Item);
                _modelManager.World.RemoveDroppedItem(item);
            }
        }

        public bool Intersects(Hitbox hb1, Hitbox hb2)
        {
            if(((hb1.MinX) < (hb2.MaxX)) && 
                ((hb1.MaxX) > (hb2.MinX)) &&
                ((hb1.MinY) < (hb2.MaxY)) &&
                ((hb1.MaxY) > (hb2.MinY)))
            {
                return true;
            }
            return false;
        }

        private bool Intersects(Vector2 point, Hitbox hitbox)
        {
            if(point.X < hitbox.MaxX && point.X > hitbox.MinX && point.Y < hitbox.MaxY && point.Y > hitbox.MinY)
            {
                return true;
            }
            return false;
        }
    }
}
