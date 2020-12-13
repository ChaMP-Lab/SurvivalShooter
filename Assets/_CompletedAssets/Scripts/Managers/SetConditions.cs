using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;

using CompleteProject;

public class SetConditions : MonoBehaviour{

  public InputField SSNtext;

  //-----------------------------//
  // Setting Gameplay Variables //
  //-----------------------------//

  public static int SSN = -1;
  public static string fileName;
  public static int totalLevels = 20; // set to 3 for testing; will be higher for actual...
  public static float currentTime;
  public static int level = 0;
  public static int cueIndex = 0;
  public static int playerLives = 3;

  //public static string cueCondition;

  public static List<float> difficultyArray = new List<float>();
  public static List<string> cueCondition = new List<string>();
  public static string audioCondition;


  protected string[][] cueConditionOrders = {
    new string[] { "none",    "tactile", "both",    "visual"  },
    new string[] { "tactile", "visual",  "both",    "none"    },
    new string[] { "visual",  "both",    "tactile", "none"    },
    new string[] { "both",    "none",    "visual",  "tactile" }
  };

  //-----------------------------//
  // Updating Gameplay Variables //
  //-----------------------------//

  public void setDifficulty(){
    for (int i = 1; i < totalLevels + 1; i++) {
      float j = HaltonSeq(i, 3)*380 + 20f; // note: these values will provide #'s > 45 if array size is extended.
      difficultyArray.Add (j);
    }
    difficultyArray = reshuffle (difficultyArray);
    for (int i = 0; i < totalLevels; i++) {
      difficultyArray.Add (Mathf.RoundToInt(difficultyArray [i]));
    }
    saveArray ("Difficulty: ", difficultyArray);
  }

  void saveArray(string aString, List<float>aList){
    string z = "";
    int n = aList.Count;
    for (int i = 0; i < n; i++){
      z = z + aList[i] + ", ";
    // Debug.Log(z + " "); // FOR DEBUGGING ONLY.
    }
    System.IO.File.AppendAllText(fileName, aString + "," + z + "," + System.Environment.NewLine);
  }

  /* Halton Sequence function
  * The basis sets the frequency of pulls.
  * Multiplying the basis by a constant sets the range.
  * Adding a constant shifts the minimum value of the range. */
  float HaltonSeq(int ind, int basis) {
    float myresult=0;
    float f = (float) 1/basis;
    int i = ind;

    while (i > 0) {
      myresult += (float) f*(i % basis);
      i = (int) Mathf.Floor(i/basis);
      f = (float) f/basis;
    }
    return(myresult);
  }

  /* Fisher-Yates reshuffle function
  * This function shuffles the values of a list,
  * and returns a new list that can be used for reassignment. */
    public static List<float> reshuffle (List<float>aList) {

      System.Random _random = new System.Random ();

      float myGO;

      int n = aList.Count;
      for (int i = 0; i < n; i++)
      {
        // NextDouble returns a random number between 0 and 1.
        int r = i + (int)(_random.NextDouble() * (n - i));
        myGO = aList[r];
        aList[r] = aList[i];
        aList[i] = myGO;
      }
      return aList;
    }

  // setting SSN and file name...
    public void setSSN(){
      SSN = int.Parse(SSNtext.text);
      fileName = "data/" + SSNtext.text + ".txt";
      System.IO.File.WriteAllText(fileName, "VARIABLES:, SSN, cueCondition, currentTime, totalLevels, TimeInTrial, ETC..." + System.Environment.NewLine);
      System.IO.File.AppendAllText(fileName, "START:," + SSN + "," + cueCondition + "," + currentTime + totalLevels + ", TOT" + ", ETC" + System.Environment.NewLine);
    }

    public void OnAudioModeSelected(bool enableAudio){
      // TODO setAudioGameObject.AsActive(false);
      cueCondition.AddRange(getConditionOrder());

      audioCondition = enableAudio ? "Audio" : "noAudio";
      Debug.Log("Setting " + audioCondition + "Condition");
      setDifficulty();
      addTutorial();

      string logBuffer = "Trials:";
      for(int i=0; i<cueCondition.Count; i++){
        logBuffer += $"\n\tTrial {i} = {cueCondition[i]}, {difficultyArray[i]}";
      }
      Debug.Log(logBuffer);

      SceneManager.LoadScene (1, LoadSceneMode.Single);
    }

    protected void addTutorial(){
      string[] conditionOrder = getConditionOrder();
      foreach(String condition in Enumerable.Reverse(conditionOrder)){
        if(condition == "none"){
          continue;
        }

        cueCondition.Insert(0, condition);
        difficultyArray.Insert(0, 20);
      }
    }

    protected string[] getConditionOrder(){
      if(SSN == -1){
        setSSN();
      }
      int setIndex = SSN % cueConditionOrders.GetLength(0);
      return cueConditionOrders[setIndex];
    }
}
