using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model.Enemies
{
    class Slime : Enemie
    {
        public enum SlimeSize { Small, Medium, Large }
        public enum SlimeAnimation { Idle, Jump}

        public SlimeSize SizeSlime;
        public SlimeAnimation LastAnimation;
        public double CurrentFrameTime;

        private float _jumpPowerY;
        private float _jumpPowerX;
        private double _lastJump;
        private double _jumpPeriodTime;

        private Vector2 _lastPosition;
        private int _tries;
        private bool _inJump;

        public Slime(Vector2 position, SlimeSize size)
        {
            Position = position;
            Velocity = new Vector2();

            switch (size)
            {
                case SlimeSize.Small:
                    Size = new Vector2(0.9f, 0.9f);
                    SizeSlime = SlimeSize.Small;
                    break;
                case SlimeSize.Medium:
                    Size = new Vector2(1.9f, 1.9f);
                    SizeSlime = SlimeSize.Medium;
                    break;
                case SlimeSize.Large:
                    Size = new Vector2(3.9f, 3.9f);
                    SizeSlime = SlimeSize.Large;
                    break;
            }

            MaxHealth = 20;
            Health = MaxHealth;
            Damage = 20;

            _jumpPowerX = 2f;
            _jumpPowerY = 14f;
            _lastJump = 0f;
            _jumpPeriodTime = 4f;

            LastAnimation = SlimeAnimation.Idle;
            CurrentFrameTime = 0;

            _tries = 0;
            _inJump = false;

            if(MainModel.Random.Next(2) == 0)
            {
                Direction = false;
            }

            else
            {
                Direction = true;
            }
        }

        public override void Update(double deltaTime, CollisionHandler collisions)
        {
            base.Update(deltaTime, collisions);

            if (!Grounded && Direction)
            {
                Velocity.X = _jumpPowerX;
            }
            else if(!Grounded && !Direction)
            {
                Velocity.X = -_jumpPowerX;
            }

            if(Grounded && _inJump)     //Falls er zuvor in einem Sprung war und jetzt wieder Grounded ist
            {
                _inJump = false;

                if(_lastPosition == new Vector2((int)Position.X, (int)Position.Y))
                {
                    _tries++;
                }

                if(_tries >= 3)
                {
                    Direction = !Direction;
                    _tries = 0;
                }
            }

        }

        protected override void UpdateMovement(double deltaTime)
        {
            if (Grounded)
            {
                _lastJump += deltaTime;
            }
            
            if(_lastJump > _jumpPeriodTime)
            {
                _lastJump = 0d;
                Jump();
            }
        }

        private void Jump()
        {
            _inJump = true;
            _lastPosition = new Vector2((int)Position.X, (int)Position.Y);


            Velocity.Y = _jumpPowerY;

            if (Direction)
            {
                Velocity.X = _jumpPowerX;
            }
            else
            {
                Velocity.X = -_jumpPowerX;
            }
        }
    }
}
