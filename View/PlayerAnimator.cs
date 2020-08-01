using ITProject.Logic;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.View
{
    class PlayerAnimator
    {
        private struct AnimationInfo
        {
            public int Position;
            public int FrameCount;

            public AnimationInfo(int position, int frameCount)
            {
                Position = position;
                FrameCount = frameCount;
            }
        }

        private Vector2 _gridSize = new Vector2(9, 11);

        private int _currentIdleAnimation = 0;

        private int _normalAnimation = 0;
        private AnimationInfo _walkAnimation = new AnimationInfo(6, 8);
        private AnimationInfo _jumpAnimation = new AnimationInfo(7, 5);
        private AnimationInfo _useAnimation = new AnimationInfo(8, 4);
        private AnimationInfo _jumpUseAnimation = new AnimationInfo(9, 4);
        private AnimationInfo _damageAnimation = new AnimationInfo(10, 2);
        private AnimationInfo[] _idleAnimations = { new AnimationInfo(1, 5),
                                                    new AnimationInfo(2, 3),
                                                    new AnimationInfo(3, 5),
                                                    new AnimationInfo(5, 6) };

        private enum AnimationType { Idle, Walking, Jumping, Using, JumpUsing, Damage }

        private AnimationType CurrentAnimation;
        private double CurrentFrameTime;

        public PlayerAnimator()
        {
            CurrentAnimation = AnimationType.Idle;
            CurrentFrameTime = 0f;
            _currentIdleAnimation = 0;
        }

        public void PlayWalkAnimation(double deltaTime, float speed, out Vector2 min, out Vector2 max)
        {
            if(CurrentAnimation != AnimationType.Walking)
            {
                CurrentFrameTime = 0;
            }

            CurrentAnimation = AnimationType.Walking;
            CurrentFrameTime += deltaTime * speed;

            int currentFrame = (int)(CurrentFrameTime) % _walkAnimation.FrameCount;

            GetTextureCoord(_walkAnimation.Position, currentFrame, _gridSize, out min, out max, 0.0f);
        }

        public void PlayIdleAnimation(double deltaTime, float speed, out Vector2 min, out Vector2 max)
        {
            //Logik für verschiedene Idle Animations einbauen

            if (CurrentAnimation != AnimationType.Idle)
            {
                if(CurrentAnimation == AnimationType.Jumping)
                {
                    if(CurrentFrameTime < 5.0d)
                    {
                        if (CurrentFrameTime < 4.0d) CurrentFrameTime = 4.0d;
                        CurrentFrameTime += deltaTime;
                        int currentJumpFrame = (int)CurrentFrameTime % _jumpAnimation.FrameCount;
                        GetTextureCoord(_jumpAnimation.Position, currentJumpFrame, _gridSize, out min, out max, 0.0f);
                        return;
                    }
                }
                else
                {
                    CurrentFrameTime = 0;
                }
            }

            CurrentAnimation = AnimationType.Idle;
            CurrentFrameTime += deltaTime;

            int currentFrame = (int)CurrentFrameTime % _idleAnimations[_currentIdleAnimation].FrameCount;
            GetTextureCoord(_idleAnimations[_currentIdleAnimation].Position, currentFrame, _gridSize, out min, out max, 0.0f);

            if(CurrentFrameTime > _idleAnimations[_currentIdleAnimation].FrameCount)
            {
                CurrentFrameTime = 0;
                _currentIdleAnimation++;

                if(_currentIdleAnimation >= _idleAnimations.Length)
                {
                    _currentIdleAnimation = 0;
                }
            }
        }

        public void PlayJumpAnimation(double deltaTime, float speed, float velocityY, out Vector2 min, out Vector2 max)
        {
            if (CurrentAnimation != AnimationType.Jumping)
            {
                CurrentFrameTime = 0;
            }

            CurrentAnimation = AnimationType.Jumping;
            CurrentFrameTime += deltaTime * speed;

            if(CurrentFrameTime > 2.2f)
            {
                if(velocityY > 0f)
                {
                    CurrentFrameTime = 2.2f;
                }
                else
                {
                    CurrentFrameTime = 3.2f;
                }
            }
            

            int currentFrame = (int)CurrentFrameTime % _jumpAnimation.FrameCount;
            GetTextureCoord(_jumpAnimation.Position, currentFrame, _gridSize, out min, out max, 0.0f);
        }

        public void PlayDamageAnimation(double deltaTime)
        {

        }

        public void PlayUseAnimation(double deltaTime)
        {

        }

        public void PlayJumpUseAnimation(double deltaTime)
        {

        }

        private void GetTextureCoord(int animation, int position, Vector2 gridSize, out Vector2 minTexCoord, out Vector2 maxTexCoord, float textureOffset)
        {
            minTexCoord = new Vector2();
            maxTexCoord = new Vector2();

            Vector2 tileSize = new Vector2();
            tileSize.X = 1 / gridSize.X;
            tileSize.Y = 1 / gridSize.Y;

            minTexCoord.X = (position * tileSize.X) + textureOffset;
            maxTexCoord.X = ((position + 1) * tileSize.X) - textureOffset;
            minTexCoord.Y = (animation * tileSize.Y) + textureOffset;
            maxTexCoord.Y = ((animation + 1) * tileSize.Y) - textureOffset;


            /*
            minTexCoord = new Vector2();
            maxTexCoord = new Vector2();

            Vector2 tileSize = new Vector2();
            tileSize.X = 1 / gridSize.X;
            tileSize.Y = 1 / gridSize.Y;
            int x = (int)(position % gridSize.X);
            int y = (int)(position / gridSize.X);

            minTexCoord.X = (x * tileSize.X) + textureOffset;
            maxTexCoord.X = ((x + 1) * tileSize.X) - textureOffset;
            minTexCoord.Y = (y * tileSize.Y) + textureOffset;
            maxTexCoord.Y = ((y + 1) * tileSize.Y) - textureOffset;
            */
        }
    }
}
