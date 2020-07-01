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

        public Vector2 CheckCollisionX(Vector2 position, Vector2 size, ref float velocity)
        {
            //Idee wieder mit Blöcke haben auch Hitboxen
            World world = _modelManager.World;
            Hitbox playerHitbox = new Hitbox(position, size, Hitbox.HitboxType.Player);
            Vector2 newPlayerPosition = new Vector2(position.X, position.Y);

            for(int iy = (int)playerHitbox.MinY; iy <= (int)playerHitbox.MaxY; iy++)
            {
                for(int ix = (int)playerHitbox.MinX; ix <= (int)playerHitbox.MaxX; ix++)
                {
                    if (!GameExtentions.CheckIfInBound(ix, iy, world.WorldSize)) continue;
                    if (MainModel.Item[_modelManager.World.GetWorld[ix, iy]].Walkable) continue;

                    Hitbox blockHitbox = new Hitbox(new Vector2(ix, iy), new Vector2(1.0f, 1.0f), Hitbox.HitboxType.Block);
                    if (!Intersects(playerHitbox, blockHitbox)) continue;

                    //Prüfen, ob geeignet für Stairs
                    if (MainModel.Item[_modelManager.World.GetWorld[(int)blockHitbox.Position.X, (int)blockHitbox.Position.Y + 1]].Walkable &&
                        MainModel.Item[_modelManager.World.GetWorld[(int)blockHitbox.Position.X, (int)blockHitbox.Position.Y + 2]].Walkable &&
                        MainModel.Item[_modelManager.World.GetWorld[(int)blockHitbox.Position.X, (int)blockHitbox.Position.Y + 3]].Walkable &&
                        position.Y > blockHitbox.MaxY && _modelManager.Player.Grounded)
                    {
                        newPlayerPosition.Y = blockHitbox.MaxY + size.Y / 2;
                        return newPlayerPosition;
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

            return newPlayerPosition;
        }

        public Vector2 CheckCollisionY(ref Vector2 position, Vector2 size, ref float velocity)
        {
            World world = _modelManager.World;
            Hitbox playerHitbox = new Hitbox(position, size, Hitbox.HitboxType.Player);
            Vector2 newPlayerPosition = new Vector2(position.X, position.Y);
            _modelManager.Player.SetGrounded(false);

            for (int iy = (int)playerHitbox.MinY; iy <= (int)playerHitbox.MaxY; iy++)
            {
                for (int ix = (int)playerHitbox.MinX; ix <= (int)playerHitbox.MaxX; ix++)
                {
                    if (!GameExtentions.CheckIfInBound(ix, iy, world.WorldSize)) continue;
                    ushort itemID = _modelManager.World.GetWorld[ix, iy];
                    if (MainModel.Item[itemID].Walkable) continue;

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
                        _modelManager.Player.SetGrounded(true);
                        velocity = 0;
                    }
                }
            }

            return newPlayerPosition;
        }

        public bool Intersects(Hitbox hb1, Hitbox hb2)
        {
            if(((hb1.MinX / 2) < (hb2.MaxX / 2)) && 
                ((hb1.MaxX / 2) > (hb2.MinX/ 2)) &&
                ((hb1.MinY / 2) < (hb2.MaxY / 2)) &&
                ((hb1.MaxY / 2) > (hb2.MinY / 2)))
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
