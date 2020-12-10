using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SubjectReadyScript : MonoBehaviour
{
    public Button readyButton;

    void Start()
    {
        if(readyButton)
        {
            SetButtonAlpha(0.0f);
            readyButton.onClick.AddListener(PlayerIsReady);
            readyButton.enabled = false;

            StartCoroutine(EnableButtonAfterDelay(3.0f));
        }

        // start the game paused
        Time.timeScale = 0;
    }

    void PlayerIsReady()
    {
        GetComponent<Canvas>().gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    void SetButtonAlpha(float alpha)
    {
        Image buttonImage = readyButton.GetComponent<Image>();
        Color color = buttonImage.color;
        color.a = alpha;
        buttonImage.color = color;
    }

    IEnumerator EnableButtonAfterDelay(float timeout)
    {
        if(readyButton)
        {
            yield return new WaitForSecondsRealtime(timeout);

            SetButtonAlpha(1.0f);
            readyButton.enabled = true;

            EventSystem.current.SetSelectedGameObject(readyButton.gameObject, null); // focus
        }
    }
}
