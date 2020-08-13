using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ITProject.Model.Enemies.BossEnemy;
using static ITProject.View.InGameView;

namespace ITProject.View.Animators
{
    class BossAnimator
    {
        private AnimationInfo _idleAnimation = new AnimationInfo(1, 5);
        private AnimationInfo _shootAnimation = new AnimationInfo(5, 7);
        private AnimationInfo _attackAnimation = new AnimationInfo(4, 4);
        private AnimationInfo _hitAnimation = new AnimationInfo(7, 1);

        private Vector2 _textureGrid = new Vector2(8, 7);

        public void PlayIdleAnimation(double deltaTime, ref double currentFrameTime, ref BossPhase lastAnimation, float speed, out Vector2 texMin, out Vector2 texMax)
        {
            if(lastAnimation == BossPhase.Damage)
            {
                PlayDamageAnimation(deltaTime, ref currentFrameTime, ref lastAnimation, speed, out texMin, out texMax);
                return;
            }

            currentFrameTime += deltaTime * speed;

            if(currentFrameTime > _idleAnimation.FrameCount)
            {
                currentFrameTime = 0f;
            }

            GetTextureCoord(_idleAnimation.Position, (int)currentFrameTime, _textureGrid, out texMin, out texMax, 0f);
        }

        public void PlayShootAnimation(double deltaTime, double currentPhaseTime, ref double currentFrameTime, ref BossPhase lastAnimation, float speed, out Vector2 texMin, out Vector2 texMax)
        {
            //CurrentTime ist die CurrentPhaseTime, nicht AnimationTime
            /*
            if (lastAnimation == BossPhase.Damage)
            {
                PlayDamageAnimation(deltaTime, ref currentFrameTime, ref lastAnimation, speed, out texMin, out texMax);
                return;
            }
            */
            lastAnimation = BossPhase.Shooting;
            currentFrameTime += deltaTime * speed;
            //Console.WriteLine(currentPhaseTime);
            if(currentPhaseTime < 1f)
            {
                GetTextureCoord(_shootAnimation.Position, (int)currentFrameTime, _textureGrid, out texMin, out texMax, 0f);
            }
            else if(currentPhaseTime < 4f)
            {
                GetTextureCoord(_shootAnimation.Position, 3, _textureGrid, out texMin, out texMax, 0f);
                currentFrameTime = 3f;
            }
            else
            {
                GetTextureCoord(_shootAnimation.Position, (int)currentFrameTime, _textureGrid, out texMin, out texMax, 0f);
            }
        }

        public void PlayDamageAnimation(double deltaTime, ref double currentFrameTime, ref BossPhase lastAnimation, float speed, out Vector2 texMin, out Vector2 texMax)
        {
            if (lastAnimation == BossPhase.Damage)
            {
                PlayDamageAnimation(deltaTime, ref currentFrameTime, ref lastAnimation, speed, out texMin, out texMax);
                return;
            }

            currentFrameTime += deltaTime * speed;

            if(currentFrameTime > 1f)
            {
                lastAnimation = BossPhase.Idle;
            }

            GetTextureCoord(_attackAnimation.Position, 0, _textureGrid, out texMin, out texMax, 0f);
        }

        public void PlayAttackAnimation(double deltaTime, ref double currentFrameTime, ref BossPhase lastAnimation, float speed, out Vector2 texMin, out Vector2 texMax)
        {
            if (lastAnimation == BossPhase.Damage)
            {
                PlayDamageAnimation(deltaTime, ref currentFrameTime, ref lastAnimation, speed, out texMin, out texMax);
                return;
            }

            currentFrameTime += deltaTime;

            if(currentFrameTime > _attackAnimation.FrameCount)
            {
                lastAnimation = BossPhase.Idle;
            }

            GetTextureCoord(_attackAnimation.Position, (int)currentFrameTime, _textureGrid, out texMin, out texMax, 0f);
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
        }

    }
}
