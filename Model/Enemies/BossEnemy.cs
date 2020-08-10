using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model.Enemies
{
    class BossEnemy : Enemie
    {
        private EnemyManager _enemyManager;
        private Vector2 _lastPlayerPosition;

        //Idee Bossgegner: Normale Hits und zyklisch laser/Kugeln schießen lassen aus Augen?

        private enum BossPhase { Idle, Attacking , Shooting }
        private BossPhase _currentBossPhase;
        private double _currentPhaseTime;

        private double _idleTime = 2d;
        private double _attackingTime = 1d;
        private double _shootingTime = 5d;

        public BossEnemy(Vector2 position, EnemyManager enemyManager)
        {
            _enemyManager = enemyManager;
            Position = position;
            Init();
        }

        public void Init()
        {
            MaxHealth = 400;
            Health = MaxHealth;

            Size = new Vector2(2.5f, 3.8f);
            Damage = 30;
        }

        public void UpdatePlayerPosition(Vector2 playerPosition)
        {
            _lastPlayerPosition = playerPosition;
        }

        public override void Update(double deltaTime, CollisionHandler collisions)
        {

            base.Update(deltaTime, collisions);
        }

        protected override void UpdateMovement(double deltaTime)
        {
            _currentPhaseTime += deltaTime;

            switch (_currentBossPhase)
            {
                case BossPhase.Idle:

                    break;
                case BossPhase.Attacking:

                    break;
                case BossPhase.Shooting:

                    break;
            }
        }

        protected override void UpdateDirection()
        {
            if(_lastPlayerPosition.X > Position.X)
            {
                Direction = true;
            }
            else
            {
                Direction = false;
            }
        }
    }

    public class LaserProjectile : GameObject
    {
        public Vector2 ShootingDirection;
        private float _speed;

        private int _maxRange = 10;
        private Vector2 _startPosition;

        public LaserProjectile(Vector2 position, Vector2 direction, float speed)
        {
            Position = position;
            _startPosition = position;
            ShootingDirection = direction;
            Velocity = new Vector2(_speed, 0f);

            if (ShootingDirection.X > 0) Direction = true;
            else Direction = false;

            _speed = speed;
        }

        public override void Update(double deltaTime, CollisionHandler collisions)
        {
            Position.X = Position.X + Velocity.X * (float)deltaTime;
        }

        public bool CheckDistance()
        {
            if (Vector2.Distance(Position, _startPosition) > _maxRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
