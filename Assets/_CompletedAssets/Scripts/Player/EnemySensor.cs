using System.Collections.Generic;
using UnityEngine;

using XInputDotNetPure;

public enum Direction
{
    Left, Right, Up, Down
}

public class Cue
{
    public Direction direction;
    public float strength;

    public Cue(Direction direction, float strength)
    {
        this.direction = direction;
        this.strength = strength;
    }
}

public class EnemySensor : MonoBehaviour
{
    public bool tactileCuesEnabled = true;
    public bool visualCuesEnabled = true;
    public float cueDuration = .5f;
    public float tactileCueIntensity = 1.0f;
    public GameObject playerZoneBarrier;

    protected Queue<Cue> cueQueue = new Queue<Cue>();
    protected List<GameObject> cuedEnemies = new List<GameObject>();

    protected Cue currentCue;
    protected float cueTimeRemaining;

    protected CompleteProject.EnemyManager enemyManager;

    void Start()
    {
        playerZoneBarrier = GameObject.Find("Barrier");
        enemyManager = GameObject.Find("EnemyManager").GetComponent<CompleteProject.EnemyManager>();
    }

    public void Update()
    {
        if(Time.deltaTime == 0.0f || !enabled)
        {
            // kill vibration if paused
            GamePad.SetVibration(0, 0, 0);
            return;
        }

        detectEnemies();
        updateCue();
        renderCue();
    }

    protected void detectEnemies()
    {
        foreach(GameObject enemy in enemyManager.enemies)
        {
            if(enemy.transform.position.z < playerZoneBarrier.transform.position.z && !cuedEnemies.Contains(enemy)){
                Direction direction = Direction.Left;
                if(enemy.transform.position.x > transform.position.x)
                {
                    direction = Direction.Right;
                }
                cueQueue.Enqueue(new Cue(direction, tactileCueIntensity));
                cuedEnemies.Add(enemy);
                Debug.Log("Queued " + direction);
            }
        }

    }

    protected void updateCue()
    {
        if(currentCue != null)
        {
            cueTimeRemaining -= Time.deltaTime;
            if(cueTimeRemaining <= 0)
            {
                Debug.Log("Cue done " + currentCue.direction);
                currentCue = null;
            }
        }

        if(currentCue == null && cueQueue.Count > 0)
        {
            currentCue = cueQueue.Dequeue();
            cueTimeRemaining = cueDuration;
            Debug.Log("Dequeue " + currentCue.direction);
        }
    }

    protected void renderCue()
    {
        if(currentCue != null){
            if(tactileCuesEnabled){
                if(currentCue.direction == Direction.Left)
                {
                    GamePad.SetVibration(0, currentCue.strength, 0);
                }else if(currentCue.direction == Direction.Right){
                    GamePad.SetVibration(0, 0, currentCue.strength);
                }
            }

            if(visualCuesEnabled){
                // @TODO: render visual cue
            }
        }else{
            GamePad.SetVibration(0, 0, 0);
            // @TODO: clear visual cue
        }
    }

    void OnApplicationQuit()
    {
        GamePad.SetVibration(0, 0, 0);
    }
}
