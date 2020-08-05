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
    public GameObject visualCueObject;

    protected Queue<Cue> cueQueue = new Queue<Cue>();
    protected List<GameObject> cuedEnemies = new List<GameObject>();

    protected Cue currentCue;
    protected float cueTimeRemaining;

    protected CompleteProject.EnemyManager enemyManager;

    void Start()
    {
        enemyManager = GameObject.Find("EnemyManager").GetComponent<CompleteProject.EnemyManager>();
        visualCueObject.GetComponent<Renderer>().enabled = false;
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

            if(visualCuesEnabled && visualCueObject != null){
                visualCueObject.GetComponent<Renderer>().enabled = true;

                Vector3 currentScale = visualCueObject.transform.localScale;
                if(currentCue.direction == Direction.Left)
                {
                    currentScale.x = -Mathf.Abs(currentScale.x);
                }else if(currentCue.direction == Direction.Right){
                    currentScale.x = Mathf.Abs(currentScale.x);
                }
                visualCueObject.transform.localScale = currentScale;
            }
        }else{
            GamePad.SetVibration(0, 0, 0);
            visualCueObject.GetComponent<Renderer>().enabled = false;
        }
    }

    void OnApplicationQuit()
    {
        GamePad.SetVibration(0, 0, 0);
    }
}
