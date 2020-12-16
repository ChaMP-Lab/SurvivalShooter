using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeEffect : MonoBehaviour {
    public delegate void FadeFinished();

    public static IEnumerator FadeCanvas(GameObject gameObject, float startAlpha, float endAlpha, float duration, FadeFinished OnFinished = null)
    {
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if(!canvasGroup)
        {
            yield return null;
        }

         var now = Time.realtimeSinceStartup;
         var startTime = now;

         canvasGroup.alpha = startAlpha;
         while (now < startTime + duration)
         {
             now = Time.realtimeSinceStartup;

             var elapsedTime = now - startTime;
             var alpha = (elapsedTime/duration) * (endAlpha-startAlpha) + startAlpha;
             canvasGroup.alpha = alpha;

             yield return new WaitForSecondsRealtime(0.02f); // wait for the next frame before continuing the loop
        }

        canvasGroup.alpha = endAlpha;
        if(OnFinished != null)
        {
            OnFinished();
        }

        yield return null;
	}
}
