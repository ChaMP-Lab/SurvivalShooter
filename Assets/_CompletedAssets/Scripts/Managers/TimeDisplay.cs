using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class TimeDisplay : MonoBehaviour
{
    public LoadScene loadSceneObject;

    Text text;
    // Start is called before the first frame update
    void Awake ()
    {
        text = GetComponent <Text> ();
    }

    // Update is called once per frame
    void Update()
    {
        if(loadSceneObject)
        {
            text.text = "" + (int)(1 + loadSceneObject.GetTimeRemaining());
        }
    }
}
