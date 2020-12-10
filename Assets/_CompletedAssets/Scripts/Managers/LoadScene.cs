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

  protected float TimeInLevel;



  IEnumerator LoadScreen(){
    yield return new WaitForSeconds(3);
    if (SetConditions.playerLives == 0){
      SetConditions.playerLives = 3;
      SetConditions.level += 1;
      SceneManager.LoadScene(1);
    }
    SceneManager.LoadScene(1);
  }

  IEnumerator LevelTimer(){
    Debug.Log("StartTime: " + TimeInLevel);
    while (TimeInLevel <= LevelDuration){
        TimeInLevel += Time.deltaTime;
        yield return null;
    }

    TimeOverEvent.Invoke();
  }

  public void GotoNextLevel(){
    TimeInLevel = 0;
    SetConditions.playerLives = 3;
    SetConditions.level += 1;
    SceneManager.LoadScene(1);
  }

  // Start is called before the first frame update
  void Start()
  {
    if (SetConditions.level % 5 == 0){
      SetConditions.cueIndex += 1;
    }

    if (SetConditions.level == 21){
      Application.Quit();
    }
    Debug.Log(SetConditions.cueCondition[SetConditions.cueIndex]);
    Debug.Log(SetConditions.audioCondition);
    SetCues(SetConditions.cueCondition[SetConditions.cueIndex], SetConditions.audioCondition);
    switch (SceneManager.GetActiveScene().buildIndex){
      case 2:
      StartCoroutine("LoadScreen");
      break;

      case 1:
      StartCoroutine("LevelTimer");
      break;
    }
  }

  public void SetCues (string cueCondition, string audioCondition){
    EnemySensor enemySensorComponent = EnemySensor.GetComponent<EnemySensor>();
    switch (cueCondition){
      case "tactile":
        enemySensorComponent.tactileCuesEnabled = true;
        enemySensorComponent.visualCuesEnabled = false;
        Debug.Log("Setting Cues");
      break;

      case "visual":
        enemySensorComponent.tactileCuesEnabled = false;
        enemySensorComponent.visualCuesEnabled = true;
        Debug.Log("Setting Cues");
      break;

      case "both":
        enemySensorComponent.tactileCuesEnabled = true;
        enemySensorComponent.visualCuesEnabled = true;
        Debug.Log("Setting Cues");
      break;

      case "none":
        enemySensorComponent.tactileCuesEnabled = false;
        enemySensorComponent.visualCuesEnabled = false;
        Debug.Log("Setting Cues");
      break;
    }

    switch(audioCondition){
      case "Audio":
        enemySensorComponent.auditoryCuesEnabled = true;
      break;

      case "noAudio":
        enemySensorComponent.auditoryCuesEnabled = false;
      break;
    }
  }

  public float GetTimeRemaining()
  {
    return LevelDuration - TimeInLevel;
  }
}
