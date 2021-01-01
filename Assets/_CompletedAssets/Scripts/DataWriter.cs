using UnityEngine;

using System;
using System.IO;

namespace CompleteProject
{
    public enum DataTag
    {
        Start,              // experimenter has moved past the start screen.
        CueOrder,           // cue order for that participant
        DifficultyOrder,    // order of difficulty values for that participant
        LevelStart,         // a new level has started
        EnemyDamage,        // player has taken damage from an enemy character
        EnemyAttack,        // player has successfully attacked an enemy character
        LevelEnd            // a level has ended
    }

    /**
        Represents data recorded on every event
    */
    public struct DataRecord
    {
        public DataTag tag;
        public int ssn;                       // participant Subject number from start screen
        public TrialParameters currentTrial;  // the current trial parameters
        public int playerHealth;              // player's current health
        public int playerLives;               // player's current number of lives
        public int kills;                     // number of enemies the player has killed in that level
        public int currentLevel;              // current level
        public float timeInLevel;             // elapsed time since the player started a new level (including level time spanning multiple lives)
        public float timeInGame;              // elapsed time since the experimenter clicked on a start screen button

        public static string HeadingString() {
            string[] orderedFields = new string[]{
                "Write line",
                "SSN",
                "Audio condition",
                "Cue condition",
                "Enemy HP",
                "Enemy damage",
                "Player health",
                "Player lives",
                "Kills",
                "Tutorial?",
                "Current level",
                "Time-in-level",
                "Time-in-game"
            };

            return string.Join(",", orderedFields);
        }

        public override string ToString() {
            CueMode cue = currentTrial.cue;

            object[] orderedValues = {
                tag,
                ssn,
                cue.HasFlag(CueMode.Audible) ? 1 : 0,
                (cue & ~CueMode.Audible),
                currentTrial.difficulty,
                currentTrial.enemyAttackDamage,
                playerHealth,
                playerLives,
                kills,
                currentTrial.isTutorial ? 1 : 0,
                currentLevel,
                timeInLevel,
                timeInGame
            };

            return string.Join(",", orderedValues);
        }
    }

    /**
        Singleton class to manage data logging functionality
    **/
    public class DataWriter
    {
        protected static DataWriter instance;

        protected StreamWriter writer;
        protected DataRecord recordTemplate;
        protected float gameStartTime = -1;
        protected float levelStartTime = -1;

        protected PlayerHealth playerHealthManager;
        protected LoadScene levelTimeManager;

        protected DataWriter(int SSN){
            string path = $"data/{SSN}.csv";
            if(!File.Exists(path))
            {
                System.IO.Directory.CreateDirectory("data");

                writer = File.CreateText(path);
                writer.WriteLine(DataRecord.HeadingString());
            }
            else
            {
                writer = File.AppendText(path);
            }
            writer.AutoFlush = true;
        }

        /**
            Initialize the DataWriter for logging

            After calling this
            * The folder and data file will be created if they don't already exist
            * The heading and initial records will be written to the file
            * Future calls to @GetInstance() will return the same value that this call returns
        **/
        public static DataWriter Init(int ssn, TrialParameters initialTrial, CueMode[] cueOrder, int[] difficultyOrder)
        {
            instance = new DataWriter(ssn);

            instance.gameStartTime = Time.time;
            instance.recordTemplate.ssn = ssn;

            instance.WriteRecord(DataTag.Start);
            instance.writer.WriteLine($"{DataTag.CueOrder.ToString()},{string.Join(",", cueOrder)}");
            instance.writer.WriteLine($"{DataTag.DifficultyOrder},{string.Join(",", difficultyOrder)}");

            return instance;
        }

        /**
            Returns the current DataWriter singleton

            Note: @Init must be called before this will return non-null
        **/
        public static DataWriter GetInstance()
        {
            return instance;
        }

        /**
            Writes a record to the data log
        **/
        public void WriteRecord(DataTag tag)
        {
            float now = Time.time;

            DataRecord record = recordTemplate;
            record.tag = tag;

            // @TODO: Remove circular dependency by decoupling this class from PlayerHealth
            if(playerHealthManager != null)
            {
                record.playerHealth = playerHealthManager.currentHealth;
            }

            if(levelTimeManager != null)
            {
                record.timeInLevel = levelTimeManager.TimeInLevel;
            }

            // @TODO: Remove circular dependency by decoupling this class from SetConditions
            record.playerLives = SetConditions.playerLives;

            record.timeInGame = (gameStartTime == -1) ? -1 : (now - gameStartTime);

            writer.WriteLine(record);
        }

        /**
            Updates the template with new trial parameters and logs a LevelStart event
        **/
        public void WriteLevelStartRecord(TrialParameters currentTrial)
        {
            float now = Time.time;
            levelStartTime = now;

            recordTemplate.currentTrial = currentTrial;
            recordTemplate.currentLevel++;

            recordTemplate.kills = 0;

            WriteRecord(DataTag.LevelStart);
        }

        /**
            Updates the template with new enemy kill count (if slain = true) and logs an EnemyAttack event
        **/
        public void WriteEnemyDamagedRecord(bool slain)
        {
            if(slain)
            {
                // we track kills here instead of using score
                // because some enemies could be configured to count as more than one point
                recordTemplate.kills++;
            }
            WriteRecord(DataTag.EnemyAttack);
        }

        /**
            Sets the PlayerHealth manager for tracking health
         **/
        public void SetPlayerHealthManager(PlayerHealth manager)
        {
            playerHealthManager = manager;
        }

        /**
            Sets the PlayerHealth manager for tracking health
         **/
        public void SetLevelTimeManager(LoadScene manager)
        {
            levelTimeManager = manager;
        }
    }
}