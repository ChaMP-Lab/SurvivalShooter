using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;

using CompleteProject;

/**
  Enemy indicator cue modes
  These are not numbered 1-4; these are bit flags
 **/
[Flags]
public enum CueMode {
  None = 0,
  Tactile = 1,
  Visual = 2,
  Both = 3,
  Audible = 4,
}

/**
  Configuration for a single trial/"level"
 **/
public struct TrialParameters {
    public CueMode cue { get; }
    public bool isTutorial { get; }
    public int difficulty { get; }

    // enemy attack strength doesn't change according to the proposal, but it's included in the data log for some reason
    // perhaps for manipulation in future studies, so including here
    public int enemyAttackDamage { get; }

    public TrialParameters(CueMode cue, int difficulty, bool audioEnabled, bool isTutorial)
    {
      this.cue = cue;
      this.difficulty = difficulty;
      this.isTutorial = isTutorial;

      this.enemyAttackDamage = 10;

      if(audioEnabled){
        this.cue |= CueMode.Audible;
      }
    }

    public override string ToString() => $"TrialParameters(cue={cue}, difficulty={difficulty})";
}

/**
  Collection of trials/"levels"
*/
public struct TrialBlock {
  public string name { get; }
  public List<TrialParameters> trials { get; }
  public TrialBlock(string name, List<TrialParameters> trials = null)
  {
    this.name = name;
    this.trials = trials != null ? trials : new List<TrialParameters>();
  }

  /**
    Returns the cue presentation order for this block. Useful for logging.
  */
  public CueMode[] GetCueOrder()
  {
    CueMode[] cueOrder = new CueMode[trials.Count];
    for(int i=0; i<trials.Count; i++)
    {
      cueOrder[i] = trials[i].cue;
    }

    return cueOrder;
  }

  /**
    Returns the difficulty order for this block. Useful for logging.
  */
  public int[] GetDifficultyOrder()
  {
    int[] difficultyOrder = new int[trials.Count];
    for(int i=0; i<trials.Count; i++)
    {
      difficultyOrder[i] = trials[i].difficulty;
    }

    return difficultyOrder;
  }

  public override string ToString() => $"TrialBlock(name={name}, trials={trials.Count})";
}

public class SetConditions : MonoBehaviour{

  public InputField SSNtext;
  protected int trialsPerBlock = 5;

  public static int playerLives = 3;
  public static List<float> difficultyArray = new List<float>();

  protected CueMode[][] CUE_MODE_ORDERS = {
    new CueMode[] { CueMode.None,                   CueMode.Tactile,                CueMode.Tactile|CueMode.Visual, CueMode.Visual  },
    new CueMode[] { CueMode.Tactile,                CueMode.Visual,                 CueMode.Tactile|CueMode.Visual, CueMode.None    },
    new CueMode[] { CueMode.Visual,                 CueMode.Tactile|CueMode.Visual, CueMode.Tactile,                CueMode.None    },
    new CueMode[] { CueMode.Tactile|CueMode.Visual, CueMode.None,                   CueMode.Visual,                 CueMode.Tactile }
  };

  public static List<TrialBlock> trialBlocks;

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
  * This function shuffles the values of a list in place */
  static void reshuffle<T>(List<T> aList) {
    System.Random _random = new System.Random ();

    int n = aList.Count;
    for (int i = 0; i < n; i++)
    {
      // NextDouble returns a random number between 0 and 1.
      int r = i + (int)(_random.NextDouble() * (n - i));
      T tmp = aList[r];
      aList[r] = aList[i];
      aList[i] = tmp;
    }
  }

  /**
    Triggered after facilitator fills out SSN and clicks an audio condition button
    Sets up levels and starts the first tutorial
  **/
  public void OnAudioModeSelected(bool enableAudio){
    int ssn = int.Parse(SSNtext.text);
    CueMode[] conditionOrder = GetConditionOrder(ssn);
    int[] difficulties = GenerateDifficulties(conditionOrder.Count() * trialsPerBlock);

    TrialBlock tutorial = new TrialBlock("Tutorial");

    trialBlocks = new List<TrialBlock>();
    trialBlocks.Add(tutorial);
    int blockIDX = 0;
    foreach(CueMode cue in conditionOrder){
      if(cue != CueMode.None){
        tutorial.trials.Add(new TrialParameters(cue, 20, enableAudio, true));
      }

      TrialBlock block = new TrialBlock($"Block {blockIDX}: {cue}");
      for(int trialIDX=0; trialIDX<trialsPerBlock; trialIDX++){
        block.trials.Add(new TrialParameters(cue, difficulties[trialIDX], enableAudio, false));
      }

      trialBlocks.Add(block);
      blockIDX++;
    }

    // start data log
    DataWriter dataLog = DataWriter.Init(ssn, CurrentTrial(), conditionOrder, difficulties);
    SceneManager.LoadScene (1, LoadSceneMode.Single);
  }

  /**
    Determines the correct cue order depending on SSN
  **/
  protected CueMode[] GetConditionOrder(int ssn){
    int setIndex = ssn % CUE_MODE_ORDERS.GetLength(0);
    return CUE_MODE_ORDERS[setIndex];
  }

  /**
    Generates a set of difficulty values
  **/
  protected int[] GenerateDifficulties(int count){
    List<int> difficulties = new List<int>(count);

    for (int i = 1; i < count + 1; i++) {
      float difficulty = HaltonSeq(i, 3)*380 + 20f;
      difficulties.Add((int)difficulty); // note: these values will provide #'s > 45 if array size is extended.
    }
    reshuffle<int>(difficulties);

    return difficulties.ToArray();
  }

  /**
    Returns the current trial
  **/
  public static TrialParameters CurrentTrial(){
    return trialBlocks[0].trials[0];
  }

  /**
    Removes the current trial and returns true if there is at least one more trial to process
  **/
  public static bool PopTrial(){
    playerLives = 3;

    trialBlocks[0].trials.RemoveAt(0);
    while(trialBlocks.Count > 0 && trialBlocks[0].trials.Count == 0){
      trialBlocks.RemoveAt(0);
    }

    return trialBlocks.Count > 0;
  }
}
