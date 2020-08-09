using OpenTK;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ITProject.Model.Enemies.Slime;
using static ITProject.View.InGameView;

namespace ITProject.View.Animators
{
    class SlimeAnimator
    {
        private double _damageAnimationTime = 5f;
        private Vector2 _textureSize = new Vector2(5, 9);

        private AnimationInfo _idleAnimationSmall = new AnimationInfo(0, 1);
        private AnimationInfo _idleAnimationMedium = new AnimationInfo(3, 1);
        private AnimationInfo _idleAnimationLarge = new AnimationInfo(6, 1);

        private AnimationInfo _jumpAnimationSmall = new AnimationInfo(1, 5);
        private AnimationInfo _jumpAnimationMedium = new AnimationInfo(4, 5);
        private AnimationInfo _jumpAnimationLarge = new AnimationInfo(7, 5);

        private AnimationInfo _damageAnimationMedium = new AnimationInfo(2, 1);

        public SlimeAnimator()
        {

        }

        public void PlayIdleAnimation(double deltaTime, ref double currentframeTime, float speed, SlimeSize size, ref SlimeAnimation lastSlimeAnimation, out Vector2 texMin, out Vector2 texMax)
        {
            if (lastSlimeAnimation == SlimeAnimation.Damage)
            {
                PlayDamageAnimation(deltaTime, ref currentframeTime, speed, size, ref lastSlimeAnimation, out texMin, out texMax);
                return;
            }

            if (lastSlimeAnimation == SlimeAnimation.Jump)
            {
                currentframeTime = 0;
                GetTextureCoord(_jumpAnimationMedium.Position, 4, _textureSize, out texMin, out texMax, 0f);
                lastSlimeAnimation = SlimeAnimation.Idle;
                return;
            }

            currentframeTime += deltaTime * speed;
            lastSlimeAnimation = SlimeAnimation.Idle;

            if(currentframeTime >= 0 && currentframeTime < 1f)
            {
                GetTextureCoord(_idleAnimationMedium.Position, 0, _textureSize, out texMin, out texMax, 0f);
                return;
            }
            else
            {
                GetTextureCoord(_jumpAnimationMedium.Position, 1, _textureSize, out texMin, out texMax, 0f);
            }

            if(currentframeTime > 2f)
            {
                currentframeTime = 0f;
            }
        }

        public void PlayJumpAnimation(double deltaTime, ref double currentframeTime, float speed, SlimeSize size, ref SlimeAnimation lastSlimeAnimation, out Vector2 texMin, out Vector2 texMax)
        {
            if(lastSlimeAnimation == SlimeAnimation.Damage)
            {
                PlayDamageAnimation(deltaTime, ref currentframeTime, speed, size, ref lastSlimeAnimation, out texMin, out texMax);
                return;
            }

            if(lastSlimeAnimation == SlimeAnimation.Idle)
            {
                currentframeTime = 0f;
            }

            currentframeTime += deltaTime * speed;
            lastSlimeAnimation = SlimeAnimation.Jump;

            if (currentframeTime > _jumpAnimationMedium.FrameCount - 2)
            {
                currentframeTime = 0;
            }

            GetTextureCoord(_jumpAnimationMedium.Position, (int)currentframeTime, _textureSize, out texMin, out texMax, 0f);
        }

        public void PlayDamageAnimation(double deltaTime, ref double currentframeTime, float speed, SlimeSize size, ref SlimeAnimation lastSlimeAnimation, out Vector2 texMin, out Vector2 texMax)
        {
            if(lastSlimeAnimation != SlimeAnimation.Damage)
            {
                currentframeTime = 0;
            }

            lastSlimeAnimation = SlimeAnimation.Damage;
            currentframeTime += deltaTime * _damageAnimationTime;

            if(currentframeTime > 1f)
            {
                currentframeTime = 0;
                lastSlimeAnimation = SlimeAnimation.Idle;
            }

            GetTextureCoord(_damageAnimationMedium.Position, 0, _textureSize, out texMin, out texMax, 0f);
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
