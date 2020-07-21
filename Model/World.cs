using ITProject.Logic;
using ITProject.WorldGeneratorStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    public class World
    {
        public enum WorldLoadType { TestLoad, NewWorld, LoadWorld }

        public ushort[,] GetWorld
        {
            get { return _world; }
        }

        public ushort[,] GetWorldBack
        {
            get { return _worldBack; }
        }

        public Vector2 WorldSize
        {
            get { return new Vector2(_width, _height); }
        }

        private List<WorldItem> _droppedItems = new List<WorldItem>();
        private Dictionary<Vector2, Chest> _worldChests;

        public bool WorldChanged = false;

        private int _width, _height;
        private ushort[,] _world;
        private ushort[,] _worldBack;

        private Random rng = new Random();

        public World(int width, int height, WorldLoadType loadType, int seed, ModelManager manager)
        {
            if (loadType == WorldLoadType.NewWorld)
            {
                _width = width;
                _height = height;
                _world = new UInt16[width, height];
                _worldChests = new Dictionary<Vector2, Chest>();
                NewWorld(seed);
            }
            else if(loadType == WorldLoadType.LoadWorld)
            {
                _width = width;
                _height = height;
                WorldLoader.LoadWorld(out width, out height, out _world, out _worldBack, out _worldChests, manager);

                if(_world.Length == 0)
                {
                    NewWorld(seed);
                }
            }
            else if(loadType == WorldLoadType.TestLoad)
            {
                _world = new UInt16[width, height];
                _width = width;
                _height = height;
                _worldChests = new Dictionary<Vector2, Chest>();
                NewWorld(seed);
                SaveWorld();
                WorldLoader.LoadWorld(out width, out height, out _world, out _worldBack, out _worldChests, manager);
            }
        }

        public void Update(double deltaTime, Player player, CollisionHandler collisionHandler)
        {
            foreach(WorldItem worldItem in _droppedItems)
            {
                //Items ziehen sich an den Spieler herran, wenn dieser nahe genug ist
                if(worldItem.LayingTime >= 3f)
                {
                    if(Vector2.Distance(player.Position, worldItem.Position) <= 5f)
                    {
                        Vector2 playerDirection = player.Position - worldItem.Position;
                        worldItem.SetVelocity(playerDirection);
                    }
                }

                worldItem.LayingTime += (float)deltaTime;

                //Physikupdate machen
                worldItem.Update(deltaTime, collisionHandler);
            }
        }

        public void SaveWorld()
        {
            WorldLoader.SaveWorld(_width, _height, _world, _worldBack, _worldChests);
        }

        public void NewWorld(int? seed)
        {
            int worldGenSeed = 1337;

            if(seed != null)
            {
                worldGenSeed = (int)seed;
            }

            GeneratorSettings settings = InitGeneratorSettings(_width, _height, worldGenSeed);
            WorldGenerator worldGenerator = new WorldGenerator(settings);
            worldGenerator.NewWorld(out _world, out _worldBack);
        }

        public void NewWorldOld()
        {
            int i = 0;
            for(int iy = 0; iy < _height; iy++)
            {
                for(int ix = 0; ix < _width; ix++)
                {
                    if (iy < 1500)
                    {
                        _world[ix, iy] = 1;     //Stein
                    }
                    else if(iy > 1510)
                    {
                        _world[ix, iy] = 1;     //Stein
                    }
                    else
                    {
                        _world[ix, iy] = 0;     //Luft
                    }
                    i++;
                }
            }

            //_world[3005, 1501] = 1;
            _world[3005, 1502] = 1;
            _world[3005, 1503] = 1;
            _world[3005, 1504] = 1;
            _world[3005, 1505] = 1;
            _world[3005, 1506] = 1;

            _world[3003, 1501] = 1;
            _world[3003, 1503] = 1;
            _world[3003, 1505] = 1;

            _world[2995, 1500] = 1;
            _world[2994, 1500] = 1;
            _world[2994, 1501] = 1;
            _world[2993, 1500] = 1;
            _world[2993, 1501] = 1;
            _world[2993, 1502] = 1;
        }

        public GeneratorSettings InitGeneratorSettings(int width, int height, int seed)
        {
            GeneratorSettings settings = new GeneratorSettings();
            settings.WorldWidth = width;
            settings.WorldHeight = height;
            settings.MinStoneHeight = 450;
            settings.MaxStoneHeight = 550;
            settings.OverworldStoneThreshold = 140;

            settings.ShowViewType = GeneratorSettings.ViewType.World;

            settings.DirtHeight = 30;

            settings.Overworld = true;
            settings.WorldGeneratorType = WorldGenerator.GeneratorType.NoiseWorldV1;
            settings.NoiseType = FastNoise.NoiseType.PerlinFractal;
            settings.FractalType = FastNoise.FractalType.FBM;
            settings.Interpolation = FastNoise.Interp.Quintic;
            settings.Seed = seed;
            settings.Frequency = 0.01f;
            settings.FractalOctaves = 6;
            settings.FractalGain = 0.5f;
            settings.FractalLacunarity = 2.0f;

            settings.WorldUndergroundGeneratorType = GeneratorSettings.UndergroundGeneratorType.Cellular_Automata;
            settings.Underground = true;
            settings.RockPercentage = 0.5f;
            settings.NeighbourCells = 2;
            settings.Generations = 4;
            settings.UndergroundStoneThreshold = 12;
            settings.rockCellsPercentageAbove = 0.55f;

            settings.UndergroundNoiseThreshold = 200;
            return settings;
        }

        public int SearchGround(int posX)
        {
            for(int iy = _height - 1; iy >= 0; iy--)
            {
                if(_world[posX, iy] != 0)
                {
                    return iy + 1;
                }
            }

            return 0;
        }

        public UInt16 GetBlockType(Vector2 position)
        {
            if (!GameExtentions.CheckIfInBound((int)position.X, (int)position.Y, WorldSize)) return 0;
            return _world[(int)position.X, (int)position.Y];
        }

        public bool GetWalkable(Vector2 position)
        {
            if (!GameExtentions.CheckIfInBound((int)position.X, (int)position.Y, WorldSize)) return false;
            return MainModel.Item[_world[(int)position.X, (int)position.Y]].Walkable;
        }

        //Gibt den Block zurück, der Abgebaut wurde
        public ushort RemoveBlock(Vector2 blockPosition)
        {
            if (GameExtentions.CheckIfInBound((int)blockPosition.X, (int)blockPosition.Y, WorldSize))
            {
                ushort removedItem = _world[(int)blockPosition.X, (int)blockPosition.Y];

                if(removedItem == 12)   //Chest
                {
                    Chest chest;
                    _worldChests.TryGetValue(new Vector2((int)blockPosition.X, (int)blockPosition.Y), out chest);

                    if(chest != null)
                    {
                        if (!chest.IsEmpty())
                        {
                            return 0;
                        }
                    }

                    _worldChests.Remove(new Vector2((int)blockPosition.X, (int)blockPosition.Y));
                }

                if (removedItem >= 70 && removedItem <= 77)
                {
                    RemoveTreeUpwards((int)blockPosition.X, (int)blockPosition.Y);
                    removedItem = 0;
                }

                _world[(int)blockPosition.X, (int)blockPosition.Y] = 0;
                WorldChanged = true;

                return removedItem;
            }
            else
            {
                return 0;
            }
        }

        //Gibt true zurück, wenn ein Block gesetzt werden konnte
        public bool PlaceBlock(Vector2 blockPosition, ushort blockType, ModelManager manager)
        {
            if (GameExtentions.CheckIfInBound((int)blockPosition.X, (int)blockPosition.Y, WorldSize))
            {
                if(_world[(int)blockPosition.X, (int)blockPosition.Y] == 0 || _world[(int)blockPosition.X, (int)blockPosition.Y] == 8)  //Luft und Wasser
                {
                    _world[(int)blockPosition.X, (int)blockPosition.Y] = blockType;

                    if(blockType == 12)     //Chest
                    {
                        _worldChests.Add(new Vector2((int)blockPosition.X, (int)blockPosition.Y), new Chest(manager));
                    }

                    WorldChanged = true;
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            else
            {
                return false;
            }
        }

        public bool AddDroppedItem(WorldItem item)
        {
            if(!MainModel.Item[_world[(int)item.Position.X, (int)item.Position.Y]].Walkable)
            {
                Console.WriteLine("Item in Block");
                Vector2? newPos = SearchForItemPlace((int)item.Position.X, (int)item.Position.Y, 0);
                if(newPos != null)
                {
                    item.Position = new Vector2(newPos.Value.X, newPos.Value.Y);
                }
                
            }

            _droppedItems.Add(item);
            return true;
        }

        public IEnumerable<WorldItem> GetDroppedItems(Vector2 min, Vector2 max)
        {
            var enumarator = from droppedItem in _droppedItems
                             where CheckWithin(droppedItem.Position, min, max)
                             select droppedItem;
            return enumarator;
        }

        public bool RemoveDroppedItem(WorldItem item)
        {
            return _droppedItems.Remove(item);
        }

        private bool CheckWithin(Vector2 position, Vector2 min, Vector2 max)
        {
            if(position.X > min.X && position.X < max.X && position.Y > min.Y && position.Y < max.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<WorldItem> GetDroppedItemsList()
        {
            return _droppedItems;
        }

        public Chest GetChest(Vector2 position)
        {
            Chest chest;
            _worldChests.TryGetValue(position, out chest);
            return chest;
        }

        public bool HasInventory(Vector2 position)
        {
            return MainModel.Item[_world[(int)position.X, (int)position.Y]].HasInventory;
        }

        private void RemoveTreeUpwards(int x, int y)
        {
            if (!GameExtentions.CheckIfInBound(x, y, new Vector2(_width, _height))) return;

            if (_world[x, y] >= 70 && _world[x, y] <= 77)
            {
                AddDroppedItem(new WorldItem(new Vector2(x + MainModel.DropItemSize.X / 2, y + MainModel.DropItemSize.Y / 2), new Vector2(((float)(rng.NextDouble() - 0.5d) * 8f), (float)(rng.NextDouble()) * 4f), MainModel.DropItemSize, new Item(4, (short)((rng.NextDouble() * 2d) + 1)), 2f));
                _world[x, y] = 0;
            }
            else
            {
                return;
            }

            RemoveTreeUpwards(x + 1, y);
            RemoveTreeUpwards(x - 1, y);
            RemoveTreeUpwards(x, y + 1);
        }

        private Vector2? SearchForItemPlace(int x, int y, int step)
        {
            if (step > 2 || !GameExtentions.CheckIfInBound(x, y, new Vector2(_width, _height)))
            {
                return null;
            }

            if(MainModel.Item[_world[x, y]].Walkable)
            {
                return new Vector2(x, y);
            }

            Vector2? newPos = SearchForItemPlace(x, y + 1, step + 1);
            if(newPos != null)
            {
                return newPos;
            }

            newPos = SearchForItemPlace(x, y - 1, step + 1);
            if (newPos != null)
            {
                return newPos;
            }

            newPos = SearchForItemPlace(x - 1, y, step + 1);
            if (newPos != null)
            {
                return newPos;
            }

            newPos = SearchForItemPlace(x + 1, y - 1, step + 1);
            if (newPos != null)
            {
                return newPos;
            }

            return null;
        }
    }

    public class WorldItem : GameObject
    {
        public Item Item;
        public float LayingTime;

        public WorldItem(Vector2 position, Vector2 velocity, Vector2 size, Item item)
        {
            Position = position;
            Size = size;
            Velocity = velocity;
            Item = item;
            LayingTime = 0f;
        }

        public WorldItem(Vector2 position, Vector2 velocity, Vector2 size, Item item, float layingTime)
        {
            Position = position;
            Size = size;
            Velocity = velocity;
            Item = item;
            LayingTime = layingTime;
        }

        public void SetVelocity(Vector2 velocity)
        {
            Velocity = velocity;
        }

        public Hitbox GetHitbox()
        {
            return new Hitbox(Position, Size, Hitbox.HitboxType.Player);
        }
    }
}
