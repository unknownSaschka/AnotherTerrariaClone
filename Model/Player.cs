using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security;
using ITProject.Logic;

namespace ITProject.Model
{
    public class Player
    {
        public enum PlayerLoadingType { SaveLoad, NewPlayer, LoadPlayer }

        public Inventory ItemInventory;

        public Vector2 Position;
        public Vector2 OldPosition; //Um den Bewegungsablauf für das CDS nachzuvollziehen
        public Vector2 Size = new Vector2(1.5f, 2.8f);
        public float WalkSpeed;
        public float MaxWalkSpeed = 10f;
        public Vector2 Velocity;
        public float Gravity = -20f;
        public bool Grounded;
        public float SlidingPower = 6.0f;

        private float _jumpPower = 18f;
        private float jumpDuration = 0f;
        private float maxJumpDuration = 0.3f;
        private float _horizontalJumpPower = 0.2f;
        private int _jumpState = 0;
        private bool _jumpHold = false;
        private float _maxVelocityY = 25f;
        private float _maxVelocityX = 7f;

        //Ideen: Walljumps, 
        
        public Player()
        {
            
        }

        public Player(float posX, float posY, Vector2 size)
        {
            Position = new Vector2(posX, posY);
            OldPosition = new Vector2(posX, posY);
            Size = size;
            WalkSpeed = 5f;
            Velocity = new Vector2(0f, 0f);
            SetGrounded(false);

            /*
            ItemInventory = new Inventory();

            ItemInventory.SetItem(2, 2, new Item(2, 29));
            ItemInventory.SetItem(2, 3, new Item(1, 15));
            */
        }

        public Player(PlayerLoadingType loadingType, int saveSlot, Vector2 position)
        {

            switch (loadingType)
            {
                case PlayerLoadingType.NewPlayer:
                    InitPlayer(position.X, position.Y + Size.Y / 2);
                    ItemInventory = new Inventory();
                    SavePlayer(saveSlot);
                    LoadPlayer(saveSlot);
                    break;
                case PlayerLoadingType.SaveLoad:
                    //Gleich wie new aber mit vielleicht anderen Startitems, etc.
                    InitPlayer(position.X, position.Y);
                    ItemInventory = new Inventory();
                    SavePlayer(saveSlot);
                    LoadPlayer(saveSlot);
                    break;
                case PlayerLoadingType.LoadPlayer:
                    LoadPlayer(saveSlot);
                    break;
            }
        }

        public void Update(double deltaTime, CollisionHandler collisions)
        {
            UpdatePhysics(deltaTime, collisions);
        }

        public void UpdatePhysics(double deltaTime, CollisionHandler collisions)
        {
            Vector2 newPlayerPosition = new Vector2(Position.X, Position.Y);

            //Bewegungen
            float appliedWalkspeed = WalkSpeed;                     //Der Spieler beschleunigt schneller, wenn er langsamer ist (unterhalb der Schwelle)
            if(Velocity.X >= 0 && Velocity.X < 5f)
            {
                appliedWalkspeed = WalkSpeed * 2;
            }
            else if(Velocity.X <= 0 && Velocity.X > -5f)
            {
                appliedWalkspeed = WalkSpeed * 2;
            }

            if(WalkSpeed == 0)      //Wenn der Spieler steht
            {
                Velocity.X = -Velocity.X * (float)deltaTime * SlidingPower + Velocity.X;
            }
            else
            {
                Velocity.X = appliedWalkspeed * (float)deltaTime + Velocity.X;
            }
            
            if (Velocity.X > _maxVelocityX) Velocity.X = _maxVelocityX;
            if (Velocity.X < -_maxVelocityX) Velocity.X = -_maxVelocityX;

            newPlayerPosition.X = Velocity.X * (float)deltaTime + Position.X;
            newPlayerPosition = collisions.CheckCollisionX(newPlayerPosition, Size, ref Velocity.X);

            //Gravitation
            if (Velocity.Y > -5f)
            {
                Velocity.Y = 2f * Gravity * (float)deltaTime + Velocity.Y;
            }
            else
            {
                Velocity.Y = Gravity * (float)deltaTime + Velocity.Y;
            }

            if (Velocity.Y < -_maxVelocityY) Velocity.Y = -_maxVelocityY;               //Damit die Geschwindigkeit beim Fallen nicht ins unendliche wächst

            newPlayerPosition.Y = Velocity.Y * (float)deltaTime + newPlayerPosition.Y;
            newPlayerPosition = collisions.CheckCollisionY(ref newPlayerPosition, Size, ref Velocity.Y);

            UpdatePosition(newPlayerPosition);
        }

        public void Jump(double deltaTime)
        {
            jumpDuration += (float)deltaTime;

            if (!Grounded && _jumpState == 0)
            {
                _jumpState = 2;
            }

            if(_jumpHold && Grounded)
            {
                return;
            }

            if ((Grounded || jumpDuration <= maxJumpDuration) && _jumpState < 2)
            {
                Velocity.Y = _jumpPower + Math.Abs(Velocity.X) * _horizontalJumpPower;
                _jumpState = 1;
            }

            _jumpHold = true;
        }

        public void NoJump()
        {
            _jumpHold = false;
            if(_jumpState == 1)
            {
                _jumpState = 2;
            }
        }

        public void SetGrounded(bool grounded)
        {
            if (grounded)
            {
                jumpDuration = 0f;
                _jumpState = 0;
            }
            Grounded = grounded;
        }

        public void Walk(bool direction)
        {
            if (direction)
            {
                WalkSpeed = MaxWalkSpeed;
            }
            else
            {
                WalkSpeed = -MaxWalkSpeed;
            }
        }

        public void UpdatePosition(float newX, float newY)
        {
            Position.X = newX;
            Position.Y = newY;
        }

        public void UpdatePosition(Vector2 newPosition)
        {
            Position = newPosition;
        }

        private bool LoadPlayer(int saveSlot)
        {
            try
            {
                PlayerSave playerSave = SaveManagement.LoadPlayer(saveSlot);

                InitPlayer(playerSave.PosX, playerSave.PosY);
                ItemInventory = new Inventory(playerSave.Inventory);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public bool SavePlayer(int saveSlot)
        {
            return SaveManagement.SavePlayer(saveSlot, this);
        }

        private void InitPlayer(float posX, float posY)
        {
            Position = new Vector2(posX, posY);
            OldPosition = new Vector2(posX, posY);
            WalkSpeed = 5f;
            Velocity = new Vector2(0f, 0f);
            SetGrounded(false);
        }
    }

    public class Hitbox
    {
        public enum HitboxType { Player, Block }

        public Vector2 Position;
        public Vector2 Size;
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
        public HitboxType Type;

        public Hitbox(Vector2 position, Vector2 size, HitboxType type)
        {
            if(type == HitboxType.Player)
            {
                Position = position;
                Size = size;
                MinX = position.X - size.X / 2;
                MaxX = position.X + size.X / 2;
                MinY = position.Y - size.Y / 2;
                MaxY = position.Y + size.Y / 2;
                Type = type;
            }
            else if(type == HitboxType.Block)
            {
                Position = position;
                Size = size;
                MinX = position.X;
                MaxX = position.X + size.X;
                MinY = position.Y;
                MaxY = position.Y + size.Y;
                Type = type;
            }
            
        }
    }

    [Serializable]
    public class PlayerSave
    {
        public string Name;
        //public Vector2 Position;
        public float PosX;
        public float PosY;
        public Item[,] Inventory;

        public PlayerSave(Vector2 position, Item[,] inventory)
        {
            PosX = position.X;
            PosY = position.Y;
            Inventory = inventory;
        }
    }

    [Serializable]
    public class PlayerSaves
    {
        public PlayerSave[] Saves = new PlayerSave[10];
    }
}
