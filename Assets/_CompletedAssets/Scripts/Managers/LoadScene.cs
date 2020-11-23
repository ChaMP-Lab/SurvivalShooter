using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{

IEnumerator LoadScreen(){
  yield return new WaitForSeconds(30);
  SceneManager.LoadScene(1);
}
    // Start is called before the first frame update
    void Start()
    {
      StartCoroutine("LoadScreen");
    }
}
