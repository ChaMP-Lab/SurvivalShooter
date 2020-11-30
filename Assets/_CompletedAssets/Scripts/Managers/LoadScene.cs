using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CompleteProject;

public class LoadScene : MonoBehaviour
{
public static float TimeInLevel;
public GameObject EnemySensor;


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
  while (TimeInLevel <= 120){
      TimeInLevel += Time.deltaTime;
      yield return null;
  }
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
  switch (cueCondition){
    case "tactile":
    EnemySensor.GetComponent<EnemySensor>().tactileCuesEnabled = true;
    EnemySensor.GetComponent<EnemySensor>().visualCuesEnabled = false;
    Debug.Log("Setting Cues");
    break;

    case "visual":
    EnemySensor.GetComponent<EnemySensor>().tactileCuesEnabled = false;
    EnemySensor.GetComponent<EnemySensor>().visualCuesEnabled = true;
    Debug.Log("Setting Cues");
    break;

    case "both":
    EnemySensor.GetComponent<EnemySensor>().tactileCuesEnabled = true;
    EnemySensor.GetComponent<EnemySensor>().visualCuesEnabled = true;
    Debug.Log("Setting Cues");
    break;

    case "none":
    EnemySensor.GetComponent<EnemySensor>().tactileCuesEnabled = false;
    EnemySensor.GetComponent<EnemySensor>().visualCuesEnabled = false;
    Debug.Log("Setting Cues");
    break;
  }

  switch(audioCondition){
    case "Audio":
    EnemySensor.GetComponent<EnemySensor>().auditoryCuesEnabled = true;
    break;

    case "noAudio":
    EnemySensor.GetComponent<EnemySensor>().auditoryCuesEnabled = false;
    break;
  }
}

}
