using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseManager : MonoBehaviour {

	private InputActions inputActions;

	public AudioMixerSnapshot paused;
	public AudioMixerSnapshot unpaused;

	Canvas canvas;
	CanvasGroup canvasGroup;

	void Awake()
	{
		inputActions = new InputActions();
		inputActions.Player.Pause.performed += context => Pause();
	}

	void OnEnable()
	{
		inputActions.Enable();
	}

	void OnDisable()
	{
		inputActions.Disable();
	}

	void Start()
	{
		canvas = GetComponent<Canvas>();
		canvasGroup = GetComponent<CanvasGroup>();

		// Start each level paused
		Pause();
	}

	public void Pause()
	{
		canvas.enabled = !canvas.enabled;
		canvasGroup.interactable = canvas.enabled;

		Time.timeScale = 1 - Time.timeScale;
		Lowpass ();
	}

	void Lowpass()
	{
		if (Time.timeScale == 0)
		{
			paused.TransitionTo(.01f);
		}
		else
		{
			unpaused.TransitionTo(.01f);
		}
	}

	public void Quit()
	{
		#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}
