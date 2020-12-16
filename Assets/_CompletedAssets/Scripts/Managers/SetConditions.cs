using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;

using CompleteProject;


[Flags]  // These are not numbered 1-4; these are bit flags
public enum CueMode {
  None = 0,
  Tactile = 1,
  Visual = 2,
  Audible = 4,
}

public struct TrialParameters {
    public CueMode cue { get; }
    public float difficulty { get; }
    public TrialParameters(CueMode cue, float difficulty, bool audioEnabled)
    {
      this.cue = cue;
      this.difficulty = difficulty;

      if(audioEnabled){
        this.cue |= CueMode.Audible;
      }
    }

    public override string ToString() => $"TrialParameters(cue={cue}, difficulty={difficulty})";
}

public struct TrialBlock {
  public string name { get; }
  public List<TrialParameters> trials { get; }
  public TrialBlock(string name, List<TrialParameters> trials = null)
  {
    this.name = name;
    this.trials = trials != null ? trials : new List<TrialParameters>();
  }

  public override string ToString() => $"TrialBlock(name={name}, trials={trials.Count})";
}

public class SetConditions : MonoBehaviour{

  public InputField SSNtext;
  protected int trialsPerBlock = 5;

  //-----------------------------//
  // Setting Gameplay Variables //
  //-----------------------------//

  public static int SSN = -1;
  public static string fileName;
  public static float currentTime;

  public static int playerLives = 3;

  //public static string CueMode;

  public static List<float> difficultyArray = new List<float>();
  public static string audioCondition;

  protected CueMode[][] CUE_MODE_ORDERS = {
    new CueMode[] { CueMode.None,                   CueMode.Tactile,                CueMode.Tactile|CueMode.Visual, CueMode.Visual  },
    new CueMode[] { CueMode.Tactile,                CueMode.Visual,                 CueMode.Tactile|CueMode.Visual, CueMode.None    },
    new CueMode[] { CueMode.Visual,                 CueMode.Tactile|CueMode.Visual, CueMode.Tactile,                CueMode.None    },
    new CueMode[] { CueMode.Tactile|CueMode.Visual, CueMode.None,                   CueMode.Visual,                 CueMode.Tactile }
  };

  public static List<TrialBlock> trialBlocks;

  //-----------------------------//
  // Updating Gameplay Variables //
  //-----------------------------//

  public List<float> generateDifficulties(int count){
    List<float> difficulties = new List<float>(count);

    for (int i = 1; i < count + 1; i++) {
      difficulties.Add(HaltonSeq(i, 3)*380 + 20f); // note: these values will provide #'s > 45 if array size is extended.
    }
    difficulties = reshuffle(difficulties);

    // @TODO: figure out what this was for
    /*
    for (int i = 0; i < totalLevels; i++) {
      difficultyArray.Add (Mathf.RoundToInt(difficultyArray [i]));
    }
    */
    return difficulties;
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
      System.IO.File.WriteAllText(fileName, "VARIABLES:, SSN, CueMode, currentTime, totalLevels, TimeInTrial, ETC..." + System.Environment.NewLine);
      // @TODO: write starting vars
    }

  public void OnAudioModeSelected(bool enableAudio){
    CueMode[] conditionOrder = getConditionOrder();
    List<float> difficulties = generateDifficulties(conditionOrder.Count() * trialsPerBlock);
    saveArray("Difficulty: ", difficulties);

    TrialBlock tutorial = new TrialBlock("Tutorial");

    trialBlocks = new List<TrialBlock>();
    trialBlocks.Add(tutorial);
    int blockIDX = 0;
    foreach(CueMode cue in conditionOrder){
      if(cue != CueMode.None){
        tutorial.trials.Add(new TrialParameters(cue, 20, enableAudio));
      }

      TrialBlock block = new TrialBlock($"Block {blockIDX}: {cue}");
      for(int trialIDX=0; trialIDX<trialsPerBlock; trialIDX++){
        float difficulty = difficulties[0];
        difficulties.RemoveAt(0);

        block.trials.Add(new TrialParameters(cue, difficulty, enableAudio));
      }

      trialBlocks.Add(block);
      blockIDX++;
    }


    string logBuffer = "Trials:\n";
    foreach(TrialBlock block in trialBlocks){
      logBuffer += $"\t{block}\n";
      foreach(TrialParameters trial in block.trials){
        logBuffer += $"\t\tTrial {trial}\n";
      }
    }
    Debug.Log(logBuffer);

    SceneManager.LoadScene (1, LoadSceneMode.Single);
  }
  protected CueMode[] getConditionOrder(){
    if(SSN == -1){
      setSSN();
    }
    int setIndex = SSN % CUE_MODE_ORDERS.GetLength(0);
    return CUE_MODE_ORDERS[setIndex];
  }

  public static TrialParameters CurrentTrial(){
    return trialBlocks[0].trials[0];
  }

  // Returns true if there is at least one more trial to process
  public static bool PopTrial(){

    trialBlocks[0].trials.RemoveAt(0);
    while(trialBlocks.Count > 0 && trialBlocks[0].trials.Count == 0){
      trialBlocks.RemoveAt(0);
    }

    return trialBlocks.Count > 0;
  }
}
