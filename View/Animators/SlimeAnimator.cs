using OpenTK;
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
        private AnimationInfo _idleAnimationSmall = new AnimationInfo(0, 1);
        private AnimationInfo _idleAnimationMedium = new AnimationInfo(2, 1);
        private AnimationInfo _idleAnimationLarge = new AnimationInfo(4, 1);

        private AnimationInfo _jumpAnimationSmall = new AnimationInfo(1, 5);
        private AnimationInfo _jumpAnimationMedium = new AnimationInfo(3, 5);
        private AnimationInfo _jumpAnimationLarge = new AnimationInfo(5, 5);

        public SlimeAnimator()
        {

        }

        public void PlayIdleAnimation(double deltaTime, ref double currentframeTime, float speed, SlimeSize size, ref SlimeAnimation lastSlimeAnimation, out Vector2 texMin, out Vector2 texMax)
        {
            if(lastSlimeAnimation == SlimeAnimation.Jump)
            {
                currentframeTime = 0;
                GetTextureCoord(_jumpAnimationMedium.Position, 4, new Vector2(6, 6), out texMin, out texMax, 0f);
                return;
            }

            currentframeTime += deltaTime * speed;
            lastSlimeAnimation = SlimeAnimation.Idle;

            if(currentframeTime >= 0 && currentframeTime < 1f)
            {
                GetTextureCoord(_idleAnimationMedium.Position, 0, new Vector2(6, 6), out texMin, out texMax, 0f);
                return;
            }
            else
            {
                GetTextureCoord(_jumpAnimationMedium.Position, 0, new Vector2(6, 6), out texMin, out texMax, 0f);
            }

            if(currentframeTime > 2f)
            {
                currentframeTime = 0f;
            }
        }

        public void PlayJumpAnimation(double deltaTime, ref double currentframeTime, float speed, SlimeSize size, ref SlimeAnimation lastSlimeAnimation, out Vector2 texMin, out Vector2 texMax)
        {
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

            GetTextureCoord(_jumpAnimationMedium.Position, (int)currentframeTime, new Vector2(6, 6), out texMin, out texMax, 0f);
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
