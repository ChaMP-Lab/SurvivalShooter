using UnityEngine;
using System.Collections;

namespace CompleteProject
{
    public class EnemyMovement : MonoBehaviour
    {
        Transform player;                       // Reference to the player's position.
        Transform enemy;                        // Reference to the enemy's position.
        Ray distanceRay;
        PlayerHealth playerHealth;              // Reference to the player's health.
        EnemyHealth enemyHealth;                // Reference to this enemy's health.
        UnityEngine.AI.NavMeshAgent nav;        // Reference to the nav mesh agent.
        difficultyControl difficultyControl;    // Reference to the difficultyControl script.

        void Awake ()
        {
            // Set up the references.
            player = GameObject.FindGameObjectWithTag ("Player").transform;
            playerHealth = player.GetComponent <PlayerHealth> ();
            enemyHealth = GetComponent <EnemyHealth> ();
            difficultyControl = GameObject.Find("DifficultyController").GetComponent<difficultyControl>();
            nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
            nav.speed = difficultyControl.enemySpeed;
        }


        void Update ()
        {
           if(Vector3.Distance(transform.position, player.position) >= difficultyControl.lineOfSight)
            {
                return;
            }
            

            // If the enemy and the player have health left...
            if(enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
            {
                // ... set the destination of the nav mesh agent to the player.
                nav.SetDestination (player.position);
            }
            // Otherwise...
            else
            {
                // ... disable the nav mesh agent.
                nav.enabled = false;
            }
        }
    }
}