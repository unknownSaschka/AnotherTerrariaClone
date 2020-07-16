using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    public abstract class GameObject
    {
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Velocity;

        public float Gravity = -20f;
        public float SlidingPower = 6.0f;
        public bool Grounded = false;

        public virtual void Update(double deltaTime, CollisionHandler collisions)
        {
            UpdatePhysics(deltaTime, collisions);
        }

        public virtual void SetGrounded(bool grounded)
        {
            Grounded = grounded;
        }

        private void UpdatePhysics(double deltaTime, CollisionHandler collisions)
        {
            Vector2 newPos = new Vector2(Position.X, Position.Y);

            //Moving for Left/Right
            Velocity.X = -Velocity.X * (float)deltaTime * SlidingPower + Velocity.X;

            newPos.X = Velocity.X * (float)deltaTime + Position.X;
            newPos = collisions.CheckCollisionX(newPos, Size, ref Velocity.X, this);

            //Apply Gravitation
            if (Velocity.Y > -5f)
            {
                Velocity.Y = 2f * Gravity * (float)deltaTime + Velocity.Y;
            }
            else
            {
                Velocity.Y = Gravity * (float)deltaTime + Velocity.Y;
            }
            newPos.Y = Velocity.Y * (float)deltaTime + newPos.Y;
            newPos = collisions.CheckCollisionY(newPos, Size, ref Velocity.Y, this);

            Position = newPos;
        }
    }
}
