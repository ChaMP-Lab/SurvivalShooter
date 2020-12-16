using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class difficultyControl : MonoBehaviour
{
    //Difficulty parameters (default values)
    public Text cueText;
    public float spawnTime = 3f;            // The amount of time that ellapses between enemy spawns (3f).
    public int enemyStartingHealth = 100;   // Enemy characters' starting health (100).
    public int enemyAttackDamage = 10;      // The amount of health taken away per enemy attack (10).
    public float enemySpeed = 2.5f;         // Enemy characters' speed (2.5f).
    public int pointsEarned = 1;            // The number of points earned for eliminating enemies (1).
    public float playerSpeed = 6f;          // The player's speed (6f).
    public int lineOfSight = 30;            // The distance at which enemies can "see" the character (30).

    // Start is called before the first frame update
    void Start()
    {
      SetDifficulty();
      startScreen();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetDifficulty(){
      enemyStartingHealth = (int) SetConditions.difficultyArray[SetConditions.level];
      Debug.Log("New difficulty: " + enemyStartingHealth);
    }

    void startScreen(){
    }
}
