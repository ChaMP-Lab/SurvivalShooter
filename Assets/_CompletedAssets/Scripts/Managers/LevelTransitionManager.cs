using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelTransitionManager : MonoBehaviour
{
    public Canvas levelStartCanvas;
    public Canvas levelFinishCanvas;
    public Canvas screenFadeCanvas;

    public PauseManager pauseManager;
    public LoadScene loadSceneObject;

    void Start()
    {
        if(levelStartCanvas)
        {
            Button readyButton = levelStartCanvas.GetComponentInChildren<Button>();
            if(readyButton)
            {
                readyButton.onClick.AddListener(PlayerIsReadyToStart);
                StartCoroutine(EnableButtonAfterDelay(3.0f, readyButton));
            }
        }

        if(pauseManager)
        {
            pauseManager.enabled = false;
        }

        // start the game paused
        Time.timeScale = 0;

        if(loadSceneObject)
        {
            loadSceneObject.TimeOverEvent.AddListener(OnTimeOver);
        }

        if(levelFinishCanvas)
        {
            levelFinishCanvas.gameObject.SetActive(false);
        }

        FadeIn();
    }

    void PlayerIsReadyToStart()
    {
        StartCoroutine(FadeEffect.FadeCanvas(levelStartCanvas.gameObject, 1.0f, 0.0f, 0.125f, BeginPlay));
    }

    void BeginPlay(){
        if(pauseManager)
        {
            Debug.Log("Pause manager re-enabled");
            pauseManager.enabled = true;
        }
        levelStartCanvas.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    void OnTimeOver()
    {
        if(pauseManager)
        {
            pauseManager.enabled = false;
        }
        Time.timeScale = 0;
        StartCoroutine(FadeEffect.FadeCanvas(levelFinishCanvas.gameObject, 0.0f, 1.0f, 1.0f));

        levelFinishCanvas.gameObject.SetActive(true);
        Button readyButton = levelFinishCanvas.GetComponentInChildren<Button>();
        if(readyButton)
        {
            readyButton.onClick.AddListener(PlayerIsReadyForNextLevel);
            StartCoroutine(EnableButtonAfterDelay(2.0f, readyButton));
        }
    }

    void PlayerIsReadyForNextLevel()
    {
        FadeOut(loadSceneObject.GotoNextLevel);
    }

    void RemoveScreenFadeCanvas()
    {
        if(screenFadeCanvas){
            screenFadeCanvas.gameObject.SetActive(false);
        }
    }

    void FadeIn()
    {
        if(screenFadeCanvas){
            screenFadeCanvas.gameObject.SetActive(true);
            StartCoroutine(FadeEffect.FadeCanvas(screenFadeCanvas.gameObject, 1.0f, 0.0f, 1.0f, RemoveScreenFadeCanvas));
        }else{
            RemoveScreenFadeCanvas();
        }
    }

    void FadeOut(FadeEffect.FadeFinished Then = null)
    {
        if(screenFadeCanvas){
            screenFadeCanvas.gameObject.SetActive(true);
            StartCoroutine(FadeEffect.FadeCanvas(screenFadeCanvas.gameObject, 0.0f, 1.0f, 1.0f, Then));
        }else{
            if(Then != null){
                Then();
            }
        }
    }

    IEnumerator EnableButtonAfterDelay(float timeout, Button button)
    {
        if(button)
        {
            button.GetComponent<CanvasGroup>().alpha = 0.0f;
            button.enabled = false;

            yield return new WaitForSecondsRealtime(timeout);

            button.enabled = true;
            EventSystem.current.SetSelectedGameObject(button.gameObject, null); // focus
            StartCoroutine(FadeEffect.FadeCanvas(button.gameObject, 0.0f, 1.0f, 0.25f));
        }
    }
}
