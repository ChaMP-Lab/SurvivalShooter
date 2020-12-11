using System.Collections.Generic;
using UnityEngine;

namespace CompleteProject
{
    public class EnemyManager : MonoBehaviour
    {
        public PlayerHealth playerHealth;       // Reference to the player's heatlh.
        //public GameObject enemy;              // The enemy prefab to be spawned.
        //public float spawnTime = 3f;          // How long between each spawn.
        public Transform[] spawnPoints;         // An array of spawn points the enemies can spawn from.
        public GameObject[] enemyTypes;              // An array of different enemy types.

        public List<GameObject> enemies = new List<GameObject>();

        difficultyControl difficultyControl;

        private void Awake()
        {
            difficultyControl = GameObject.Find("DifficultyController").GetComponent<difficultyControl>();
        }

        void Start ()
        {
            // Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
            InvokeRepeating ("Spawn", difficultyControl.spawnTime, difficultyControl.spawnTime);
        }

        void Update () {
            for(int i=enemies.Count - 1; i > -1; i--)
            {
                GameObject enemy = enemies[i];
                CompleteProject.EnemyHealth healthComponent = (CompleteProject.EnemyHealth)enemy.GetComponentInChildren<CompleteProject.EnemyHealth>();
                if(healthComponent.isDead){
                    enemies.RemoveAt(i);
                }
            }
        }

        void Spawn ()
        {
            // If the player has no health left and there are fewer than 30 enemies in gameplay...
            if(playerHealth.currentHealth <= 0f || enemies.Count >= 30)
            {
                // ... exit the function.
                return;
            }

            // Find a random index between zero and one less than the number of spawn points.
            int spawnPointIndex = Random.Range (0, spawnPoints.Length);

            // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
            GameObject enemy = Instantiate (enemyTypes[Random.Range(0,enemyTypes.Length)], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
            enemies.Add(enemy);
            // ... and increase the enemycount by 1.
        }

        public void DestroyAllEnemies()
        {
            foreach(GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
            enemies.Clear();
        }

    }
}