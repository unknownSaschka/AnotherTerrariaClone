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

        public Slime(Vector2 position, SlimeSize size)
        {
            Position = position;
            Velocity = new Vector2();

            switch (size)
            {
                case SlimeSize.Small:
                    Size = new Vector2(1f, 1f);
                    SizeSlime = SlimeSize.Small;
                    break;
                case SlimeSize.Medium:
                    Size = new Vector2(2f, 2f);
                    SizeSlime = SlimeSize.Medium;
                    break;
                case SlimeSize.Large:
                    Size = new Vector2(4f, 4f);
                    SizeSlime = SlimeSize.Large;
                    break;
            }

            MaxHealth = 20;
            Health = MaxHealth;
            Damage = 20;

            _jumpPowerX = 4f;
            _jumpPowerY = 10f;
            _lastJump = 0f;
            _jumpPeriodTime = 4f;

            LastAnimation = SlimeAnimation.Idle;
            CurrentFrameTime = 0;
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
