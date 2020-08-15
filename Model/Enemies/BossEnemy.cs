using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model.Enemies
{
    public class BossEnemy : Enemie
    {
        private EnemyManager _enemyManager;
        private Vector2 _lastPlayerPosition;

        private float _projectileSpeed = 10f;

        //Idee Bossgegner: Normale Hits und zyklisch laser/Kugeln schießen lassen aus Augen?

        public enum BossPhase { Idle, Attacking, Shooting, Damage }
        public BossPhase CurrentBossPhase;
        public BossPhase LastAnimation = BossPhase.Idle;
        public double CurrentPhaseTime;
        //Für Animationen
        public double CurrentFrameTime = 0f;

        private double _idlePhaseTime = 2d;
        private double _attackingPhaseTime = 1d;
        private double _shootingPhaseTime = 5d;

        private double _shootingTimer = 0d;
        private double _shootingPeriod = 1f;    //Gegner schießt pro Sekunde ein mal

        private double _jumpTimer;

        public BossEnemy(Vector2 position, EnemyManager enemyManager)
        {
            _enemyManager = enemyManager;
            Position = position;
            Init();
        }

        public void Init()
        {
            MaxHealth = 800;
            Health = MaxHealth;
            _neededToolLevel = 3;
            _swordNeeded = true;

            _jumpTimer = 5f;

            Size = new Vector2(2.0f, 3.3f);
            Damage = 30;

            RemoveAtDistance = false;
        }

        public void UpdatePlayerPosition(Vector2 playerPosition)
        {
            _lastPlayerPosition = playerPosition;
        }

        public override void Update(double deltaTime, CollisionHandler collisions)
        {
            base.Update(deltaTime, collisions);

            if(_lastPlayerPosition.X > Position.X)
            {
                Direction = true;
            }
            else
            {
                Direction = false;
            }

            UpdateBossMovement(deltaTime);
        }

        protected override void UpdateMovement(double deltaTime)
        {
            
        }

        private void UpdateBossMovement(double deltaTime)
        {
            Jump(deltaTime);
            CurrentPhaseTime += deltaTime;

            switch (CurrentBossPhase)
            {
                case BossPhase.Idle:
                    IdlePhase(deltaTime);
                    break;
                case BossPhase.Attacking:
                    AttackingPhase(deltaTime);
                    break;
                case BossPhase.Shooting:
                    ShootingPhase(deltaTime);
                    break;
            }
        }

        private void IdlePhase(double deltaTime)
        {
            if(CurrentPhaseTime > _idlePhaseTime)
            {
                CurrentBossPhase = BossPhase.Shooting;
                CurrentFrameTime = 0d;
            }
        }

        private void AttackingPhase(double deltaTime)
        {
            if(CurrentPhaseTime > _attackingPhaseTime)
            {
                CurrentBossPhase = BossPhase.Idle;
            }
        }

        private void ShootingPhase(double deltaTime)
        {
            if(CurrentPhaseTime > _shootingPhaseTime)
            {
                CurrentBossPhase = BossPhase.Idle;
                CurrentPhaseTime = 0d;
            }
            //Console.WriteLine(CurrentPhaseTime);

            _shootingTimer += deltaTime;
            if(_shootingTimer > _shootingPeriod)
            {
                Shoot();
                _shootingTimer = 0d;
            }
        }

        private void Shoot()
        {
            Vector2 direction;
            Vector2 size = new Vector2(1f, 0.5f);

            if (Direction) direction = new Vector2(1f, 0f);
            else direction = new Vector2(-1f, 0f);

            _enemyManager.NewProjectile(new Vector2(Position.X, Position.Y + 1.0f), size, direction, _projectileSpeed);
            //_enemyManager.LaserProjectiles.Add(new LaserProjectile(new Vector2(Position.X, Position.Y + 0.5f), direction, _projectileSpeed));
        }

        private void Jump(double deltaTime)
        {
            _jumpTimer -= deltaTime;
            if(_jumpTimer < 0d)
            {
                Velocity.Y = 18f;
                _jumpTimer = (MainModel.Random.NextDouble() + 0.5) * 4;     //Zwischen 2 und 6 Sekunden soll der gegner Random springen
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
        public int Damage = 15;

        private float _speed;

        private int _maxRange = 20;
        private Vector2 _startPosition;

        public LaserProjectile(Vector2 position, Vector2 size, Vector2 direction, float speed)
        {
            Position = position;
            Size = size;
            _startPosition = position;
            ShootingDirection = direction;
            Velocity = new Vector2(_speed, 0f);

            if (ShootingDirection.X > 0) Direction = true;
            else Direction = false;

            _speed = speed * direction.X;
        }

        public override void Update(double deltaTime, CollisionHandler collisions)
        {
            Position.X = Position.X + _speed * (float)deltaTime;
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

        public Hitbox GetHitbox()
        {
            return new Hitbox(Position, Size, Hitbox.HitboxType.Player);
        }
    }
}
