using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ITProject.Model.ItemInfoTools;

namespace ITProject.Model.Enemies
{
    public class EnemyManager
    {
        public List<Enemie> Enemies;
        public List<LaserProjectile> LaserProjectiles;
        public enum EnemyType { Slime, Boss }

        private AudioManager _audioManager;
        private Player _player;

        private float _enemyRemoveDistance = 80f;
        private float _enemySpawningDistance = 10f;

        public EnemyManager(AudioManager audioManager, Player player)
        {
            Enemies = new List<Enemie>();
            LaserProjectiles = new List<LaserProjectile>();
            _audioManager = audioManager;
            _player = player;
        }

        /// <summary>
        /// Spawned einen Gegner random innerhalb der Distanz zum Spieler
        /// </summary>
        /// <param name="enemyType">Gegnertyp der gespawned werden soll</param>
        /// <param name="position">Position des Spielers</param>
        /// <param name="distance">Radius, um den der Gegner gespawned werden soll</param>
        public void SpawnEnemie(EnemyType enemyType, Vector2 position, float distance)
        {
            //Gegner innerhalb der Distanz spawnen lassen
            if(enemyType != EnemyType.Boss)
            {
                var enemies = from enemie in Enemies
                              where Vector2.Distance(enemie.Position, position) < _enemySpawningDistance
                              select enemie;

                if(enemies.Count() != 0)
                {
                    return;
                }
            }

            switch (enemyType)
            {
                case EnemyType.Slime:
                    Enemies.Add(new Slime(position, Slime.SlimeSize.Medium));
                    break;
                case EnemyType.Boss:
                    Enemies.Add(new BossEnemy(position, this));
                    break;
            }
        }

        public void NewEnemie(Enemie enemie)
        {
            Enemies.Add(enemie);
        }

        public IEnumerable<Enemie> GetNearbyEnemies(Vector2 position, float distance)
        {
            return from s in Enemies
                   where Vector2.Distance(position, s.Position) < distance
                   select s;
        }

        public void Update(double deltaTime, CollisionHandler collisionHandler)
        {
            foreach(Enemie enemie in Enemies.ToList())
            {
                if (enemie.Dead)
                {
                    Enemies.Remove(enemie);
                    continue;
                }
                else if(enemie.RemoveAtDistance)
                {
                    if(Vector2.Distance(enemie.Position, _player.Position) > _enemyRemoveDistance)
                    {
                        Enemies.Remove(enemie);
                        continue;
                    }
                }

                if (enemie.GetType().Equals(typeof(BossEnemy)))
                {
                    ((BossEnemy)enemie).UpdatePlayerPosition(_player.Position);
                }

                enemie.Update(deltaTime, collisionHandler);
            }

            foreach(LaserProjectile projectile in LaserProjectiles.ToList())
            {
                projectile.Update(deltaTime, collisionHandler);

                if (projectile.CheckDistance())
                {
                    LaserProjectiles.Remove(projectile);
                }
            }
        }

        public void NewProjectile(Vector2 position, Vector2 size, Vector2 direction, float speed)
        {
            LaserProjectiles.Add(new LaserProjectile(position, size, direction, speed));
            _audioManager.PlaySound(AudioManager.SoundType.Shoot);
        }

    }

    public abstract class Enemie : GameObject
    {
        public int MaxHealth;
        public int Health;
        public int Damage;
        public bool Dead = false;
        public bool GotHitted = false;
        public bool RemoveAtDistance = true;
        protected int _neededToolLevel = 0;
        protected bool _swordNeeded = false;

        public override void Update(double deltaTime, CollisionHandler collisions)
        {
            UpdateMovement(deltaTime);
            GotHitted = false;
            base.Update(deltaTime, collisions);
        }

        public virtual void GetDamage(int Damage, ItemToolType toolType, int toolLevel)
        {
            if (toolLevel < _neededToolLevel) return;
            if (toolType != ItemToolType.Sword && _swordNeeded) return;

            Console.WriteLine("Enemy Damage");
            GotHitted = true;
            Health -= Damage;

            if(Health < 0)
            {
                Health = 0;
                Dead = true;
            }
        }

        public Hitbox GetHitbox()
        {
            return new Hitbox(Position, new Vector2(Size.X - 1f, Size.Y - 0.3f), Hitbox.HitboxType.Player);
        }

        public Hitbox GetCollisionHitbox()
        {
            return new Hitbox(Position, new Vector2(Size.X, Size.Y), Hitbox.HitboxType.Player);
        }

        protected abstract void UpdateMovement(double deltaTime);
    }
}
