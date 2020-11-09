using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public bool auditoryCuesEnabled = true;

    public float cueDuration = .5f;
    public float tactileCueIntensity = 1.0f;
    public GameObject playerZoneBarrier;
    public GameObject visualCueObject;

    protected Queue<Cue> cueQueue = new Queue<Cue>();
    protected List<GameObject> cuedEnemies = new List<GameObject>();
    AudioSource audioSource;


    protected Cue currentCue;
    protected float cueTimeRemaining;
    protected bool newCue = false;

    protected CompleteProject.EnemyManager enemyManager;

    void Start()
    {
        enemyManager = GameObject.Find("EnemyManager").GetComponent<CompleteProject.EnemyManager>();
        visualCueObject.GetComponent<Renderer>().enabled = false;

        // Resample audio to make duration match `cueDuration` setting
        // audio source probably needs to be mono
        audioSource = GetComponent<AudioSource>();
        audioSource.clip.LoadAudioData();

        float[] samples = new float[audioSource.clip.samples];
        float sampleRate = audioSource.clip.samples / audioSource.clip.length;
        float[] stretchedSamples = new float[(int)(cueDuration * sampleRate)];
        audioSource.clip.GetData(samples, 0);

        for (int i=0; i<stretchedSamples.Length; i++)
        {
            int sampleIdx = (int)(((float)i/stretchedSamples.Length) * samples.Length);
            stretchedSamples[i] = samples[sampleIdx];
        }

        audioSource.clip.SetData(stretchedSamples, 0);
    }

    public void Update()
    {
        if(Time.deltaTime == 0.0f || !enabled)
        {
            // kill vibration if paused
            SetVibration(0, 0);
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
            newCue = true;
        }
    }

    protected void renderCue()
    {
        if(currentCue != null)
        {
            if(newCue)
            {
                newCue = false;
                if(auditoryCuesEnabled)
                {
                    audioSource.panStereo = (currentCue.direction == Direction.Left) ? -1 : 1;
                    audioSource.Play();
                }

                if(tactileCuesEnabled)
                {
                    if(currentCue.direction == Direction.Left)
                    {
                        SetVibration(currentCue.strength, 0);
                    }else if(currentCue.direction == Direction.Right){
                        SetVibration(0, currentCue.strength);
                    }
                }

                if(visualCuesEnabled && visualCueObject != null)
                {
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
            }
        }else{
            SetVibration(0, 0);
            visualCueObject.GetComponent<Renderer>().enabled = false;
        }
    }

    void OnApplicationQuit()
    {
        SetVibration(0, 0);
    }

    void SetVibration(float left, float right)
    {
        Gamepad gamepad = Gamepad.current;
        if(gamepad != null)
        {
            gamepad.SetMotorSpeeds(left, right);
        }
    }
}
