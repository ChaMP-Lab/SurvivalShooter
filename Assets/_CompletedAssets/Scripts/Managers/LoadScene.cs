using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CompleteProject;

using UnityEngine.Events;


public class LoadScene : MonoBehaviour
{
  public float LevelDuration = 120;
  public GameObject EnemySensor;

  public UnityEvent TimeOverEvent = new UnityEvent();

  public float TimeInLevel { get; protected set; }
  protected Coroutine levelTimerRoutine;

  IEnumerator LevelTimer(){
    Debug.Log("StartTime: " + TimeInLevel);
    while (TimeInLevel <= LevelDuration){
        TimeInLevel += Time.deltaTime;
        yield return null;
    }

    TimeOverEvent.Invoke();
  }

  public void GotoNextLevel(){
    if(SetConditions.PopTrial()){
      TimeInLevel = 0;
      SceneManager.LoadScene(1);
    }else{
      SceneManager.LoadScene(2);
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    TrialParameters currentTrial = SetConditions.CurrentTrial();
    SetCues(currentTrial);
    StartLevelTimer();

    DataWriter.GetInstance().SetLevelTimeManager(this);
  }

  public void SetCues(TrialParameters trialParameters) {
    Debug.Log($"Setting cues: {trialParameters}");

    EnemySensor enemySensorComponent = EnemySensor.GetComponent<EnemySensor>();
    enemySensorComponent.tactileCuesEnabled = trialParameters.cue.HasFlag(CueMode.Tactile);
    enemySensorComponent.visualCuesEnabled = trialParameters.cue.HasFlag(CueMode.Visual);
    enemySensorComponent.auditoryCuesEnabled = trialParameters.cue.HasFlag(CueMode.Audible);
  }

  public float GetTimeRemaining()
  {
    return LevelDuration - TimeInLevel;
  }

  public void PauseLevelTimer()
  {
    StopCoroutine(levelTimerRoutine);
  }

  public void StartLevelTimer()
  {
    levelTimerRoutine = StartCoroutine("LevelTimer");
  }
}
