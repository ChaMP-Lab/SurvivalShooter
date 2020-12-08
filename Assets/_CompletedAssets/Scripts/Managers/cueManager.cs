using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cueManager : MonoBehaviour
{
  Text text;

  void Awake ()
  {
      // Set up the reference.
      text = GetComponent <Text> ();
  }


  void Update ()
  {
      // Set the displayed text to be the word "Score" followed by the score value.
      text.text = "Enemies will be denoted by " + SetConditions.cueCondition[SetConditions.cueIndex] + " cues.";
  }
}
