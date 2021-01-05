using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CompleteProject
{

    public class LevelTransitionManager : MonoBehaviour
    {
        public float respawnTime = 30.0f;

        public Animator animator;

        public Canvas levelStartCanvas;
        public Canvas levelFinishCanvas;
        public Canvas screenFadeCanvas;

        public PauseManager pauseManager;
        public LoadScene loadSceneObject;
        public PlayerHealth playerHealth;
        public EnemyManager enemyManager;

        public Text livesLeftText;
        public Text respawnTimerText;

        protected Vector3 initialPosition;
        protected Quaternion initialRotation;

        void Start()
        {
            initialPosition = playerHealth.gameObject.transform.position;
            initialRotation = playerHealth.gameObject.transform.rotation;

            Button readyButton = levelStartCanvas.GetComponentInChildren<Button>();
            if(readyButton)
            {
                readyButton.onClick.AddListener(PlayerIsReadyToStart);
                StartCoroutine(EnableButtonAfterDelay(3.0f, readyButton));
            }
            pauseManager.enabled = false;

            // start the game paused
            Time.timeScale = 0;

            loadSceneObject.TimeOverEvent.AddListener(OnTimeOver);
            playerHealth.outOfHealthEvent.AddListener(OnPlayerDied);

            levelFinishCanvas.gameObject.SetActive(false);

            FadeIn();
        }

        void OnPlayerDied()
        {
            SetConditions.playerLives--;
            if(SetConditions.playerLives <= 0){
                DataWriter.GetInstance().WriteRecord(DataTag.LevelEnd);
            }

            livesLeftText.text = "" + (SetConditions.playerLives);

            loadSceneObject.PauseLevelTimer();
            StartCoroutine(DeathDelay());
        }

        void Update()
        {
            animator.SetInteger("Health", playerHealth.currentHealth);
            animator.SetInteger("Lives", SetConditions.playerLives);
        }

        void PlayerIsReadyToStart()
        {
            StartCoroutine(FadeEffect.FadeCanvas(levelStartCanvas.gameObject, 1.0f, 0.0f, 0.125f, BeginPlay));
        }

        void BeginPlay(){
            if(pauseManager)
            {
                pauseManager.enabled = true;
            }
            levelStartCanvas.gameObject.SetActive(false);
            Time.timeScale = 1;

            DataWriter.GetInstance().WriteLevelStartRecord(SetConditions.CurrentTrial());
        }

        void OnTimeOver()
        {
            DataWriter.GetInstance().WriteRecord(DataTag.LevelEnd);
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

        IEnumerator DeathDelay(){
            while (animator.GetCurrentAnimatorStateInfo(0).IsTag("NotReadyForTransition"))
            {
                yield return null;
            }

            if(SetConditions.playerLives > 0)
            {
                float now = Time.time;
                float startTime = now;
                while(now - startTime < respawnTime)
                {
                    respawnTimerText.text = "" + (int)(1 + respawnTime - (now - startTime));
                    now = Time.time;
                    yield return null;
                }
                respawnTimerText.text = "0";

                playerHealth.gameObject.transform.position = initialPosition;
                playerHealth.gameObject.transform.rotation = initialRotation;
                enemyManager.DestroyAllEnemies();

                playerHealth.ResetHealth();

                playerHealth.GetComponent<PlayerMovement>().enabled = true;
                playerHealth.GetComponent<PlayerShooting>().enabled = true;
                loadSceneObject.StartLevelTimer();
            }else{
                loadSceneObject.GotoNextLevel();
            }
        }
    }
}