﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model.Enemies
{
    public class EnemyManager
    {
        public List<Enemie> Enemies;
        public enum EnemyType { Slime, Boss }

        public EnemyManager()
        {
            Enemies = new List<Enemie>();
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

            switch (enemyType)
            {
                case EnemyType.Slime:
                    Enemies.Add(new Slime(position, Slime.SlimeSize.Medium));
                    break;
                case EnemyType.Boss:

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
            foreach(Enemie enemie in Enemies)
            {
                enemie.Update(deltaTime, collisionHandler);
            }
        }

    }

    public abstract class Enemie : GameObject
    {
        public int MaxHealth;
        public int Health;
        public int Damage;
        public bool Dead = false;

        public override void Update(double deltaTime, CollisionHandler collisions)
        {
            UpdateMovement(deltaTime);

            base.Update(deltaTime, collisions);
        }

        public void GetDamage(int Damage)
        {
            Health -= Damage;

            if(Health < 0)
            {
                Health = 0;
                Dead = true;
            }
        }

        protected abstract void UpdateMovement(double deltaTime);
    }
}
