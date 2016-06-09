using UnityEngine;
using System.Collections;

namespace dbga
{
    public class GameplayController : MonoBehaviour
    {
        public static readonly string GameWillBegin = "GameControllerGameWillBegin";
        public static readonly string GameWillEnd = "GameControllerGameWillEnd";

        public enum GameStates
        {
            None = 0,
            Prepare,
            StartRound,
            Playing,
            Evaluation,
            GameOver
        }

        [SerializeField]
        private UIGameplayController uiGameplayController;
        [SerializeField]
        private PlayerController playerController;
        [SerializeField]
        private UnitsCoordinator unitsCoordinator;
        [SerializeField]
        private Spawner playerBulletSpawner;
        [SerializeField]
        private Spawner enemyExplosionsSpawner;
        [SerializeField]
        private Spawner enemyBulletSpawner;

        [SerializeField]
        private float evaluationStillTime = 1.0f;

        [SerializeField]
        private AudioClip enemyKilledSound;

        private int scorePlayerOne = 0;
        public int ScorePlayerOne
        {
            get { return scorePlayerOne; }
        }
        private int scorePlayerTwo = 0;
        public int ScorePlayerTwo
        {
            get { return scorePlayerTwo; }
        }
        private int highScore = 0;
        public int HighScore
        {
            get { return highScore; }
        }

        private int livesPlayerOne = 3;
        public int LivesPlayerOne
        {
            get { return livesPlayerOne; }
        }

        private int numberOfRounds;
        public int NumberOfRounds
        {
            get { return numberOfRounds; }
        }

        private bool gameIsOver;
        public bool IsGameOver
        {
            get { return gameIsOver; }
        }

        private GameStates currentState = GameStates.None;
        private GameStates nextState = GameStates.None;
        private bool pendingStateChange;

        private float evaluationTimer;

        private int enemiesCount;

        private bool pendingSerialize;
        private bool paused;

        private AudioSource enemyKilledAudioSource;

        void Awake()
        {
            SubscribeToNotifications();
        }

        void Start()
        {
            pendingSerialize = false;
            paused = false;
            gameIsOver = false;

            enemyKilledAudioSource = gameObject.AddComponent<AudioSource>();
            enemyKilledAudioSource.playOnAwake = false;

            NewGame();
        }

        void Update()
        {
            switch(currentState)
            {
                case GameStates.Prepare:
                    unitsCoordinator.Setup();

                    UpdateUI();

                    RegisterStateChange(GameStates.StartRound);
                    break;

                case GameStates.StartRound:
                    StartRound();
                    break;

                case GameStates.Playing:
                    if (!paused)
                    {
                        playerController.UpdateLogic();
                        unitsCoordinator.UpadeLogic();

                        if (enemiesCount <= 0)
                        {
                            evaluationTimer = evaluationStillTime;

                            RegisterStateChange(GameStates.Evaluation);
                        }

                        if (livesPlayerOne <= 0)
                        {
                            GameOver();
                        }
                    }
                    break;

                case GameStates.Evaluation:
                    evaluationTimer -= Time.deltaTime;
                    if (evaluationTimer <= 0)
                    {
                        RegisterStateChange(GameStates.StartRound);
                    }
                    break;

                case GameStates.GameOver:
                    break;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePauseStatus();
            }
        }

        void LateUpdate()
        {
            ProcessChangeState();

            SerializeIfNeeded();
        }

        public void NewGame()
        {
            Deserialize();

            BeginGameplay();
        }

        public void GameOver()
        {
            EndGameplay();

            SerializeIfNeeded();
        }

        public void RegisterStateChange(GameStates state)
        {
            nextState = state;
            pendingStateChange = true;
        }

        public void TogglePauseStatus()
        {
            paused = !paused;
            Time.timeScale = paused ? 0.0f : 1.0f;
        }

        private void ProcessChangeState()
        {
            if (pendingStateChange)
            {
                currentState = nextState;
                nextState = GameStates.None;
                pendingStateChange = false;
            }
        }

        private void SubscribeToNotifications()
        {
            NotificationCenter.DefaultCenter.AddSubscriber(Enemy.WillBeDestroyed, this, "EnemyWillBeDestroyedEvent");
        }

        private void BeginGameplay()
        {
            scorePlayerOne = 0;
            scorePlayerTwo = 0;
            numberOfRounds = 0;

            livesPlayerOne = 3;

            gameIsOver = false;
            paused = false;
            pendingSerialize = false;

            NotificationCenter.DefaultCenter.AddSubscriber(PlayerController.DidCollideWithEnemyBullet, this, "PlayerDidCollideWithEnemyBulletEvent");
            NotificationCenter.DefaultCenter.AddSubscriber(PlayerController.DidCollideWithEnemy, this, "PlayerDidCollideWithEnemyEvent");

            RegisterStateChange(GameStates.Prepare);
        }

        private void EndGameplay()
        {
            gameIsOver = true;

            NotificationCenter.DefaultCenter.RemoveSubscriber(PlayerController.DidCollideWithEnemyBullet, this, "PlayerDidCollideWithEnemyBulletEvent");
            NotificationCenter.DefaultCenter.RemoveSubscriber(PlayerController.DidCollideWithEnemy, this, "PlayerDidCollideWithEnemyEvent");

            int bestScore = scorePlayerOne > scorePlayerTwo ? scorePlayerOne : scorePlayerTwo;
            if (bestScore > highScore)
            {
                highScore = bestScore;
            }

            pendingSerialize = true;

            playerBulletSpawner.DespawnAll();
            enemyExplosionsSpawner.DespawnAll();
            enemyBulletSpawner.DespawnAll();
            unitsCoordinator.DespawnAll();

            NotificationCenter.DefaultCenter.PublishNotification(new Notification(this, GameWillEnd));

            RegisterStateChange(GameStates.GameOver);
        }

        private void StartRound()
        {
            unitsCoordinator.Reset();
            playerBulletSpawner.DespawnAll();
            enemyExplosionsSpawner.DespawnAll();

            playerController.Reset();

            //@note: we should update number of enemies after the units status reset!
            enemiesCount = unitsCoordinator.NumberOfEnemies;
            numberOfRounds++;

            UpdateUI();

            RegisterStateChange(GameStates.Playing);
        }

        private void UpdateUI()
        {
            if (uiGameplayController != null)
            {
                uiGameplayController.UpdateView(this);
            }
        }

        private void EnemyWillBeDestroyedEvent(Notification notification)
        {
            Enemy enemy = (Enemy)notification.Publisher;
            if (enemy != null)
            {
                scorePlayerOne += enemy.GetScoreValue();
                UpdateUI();

                enemiesCount--;

                unitsCoordinator.UpdateMovementDelay();

                if (enemyKilledAudioSource != null)
                {
                    enemyKilledAudioSource.PlayOneShot(enemyKilledSound);
                }
            }
        }

        private void SerializeIfNeeded()
        {
            if (pendingSerialize)
            {
                PlayerPrefs.SetInt("si_highscore", highScore);

                pendingSerialize = false;
            }
        }

        private void Deserialize()
        {
            highScore = PlayerPrefs.GetInt("si_highscore", 0);
        }

        private void PlayerDidCollideWithEnemyBulletEvent(Notification notification)
        {
            livesPlayerOne--;
            if (livesPlayerOne < 0)
            {
                livesPlayerOne = 0;
            }

            UpdateUI();
        }

        private void PlayerDidCollideWithEnemyEvent(Notification notification)
        {
            GameOver();
        }
    }
}
