using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace dbga
{
    public class UIGameplayController : MonoBehaviour
    {
        [SerializeField]
        private Text scoreValuePlayerOne;
        [SerializeField]
        private Text scoreValuePlayerTwo;
        [SerializeField]
        private Text highscoreValue;
        [SerializeField]
        private Text roundsInfoText;
        [SerializeField]
        private Image[] livesPlayerOne;
        [SerializeField]
        private Text gameoverText;

        void Awake()
        {
            SubscribeToNotifications();
        }

        void Start()
        {
            if (gameoverText != null)
            {
                gameoverText.enabled = false;
            }
        }

        public void UpdateView(GameplayController gameplayController)
        {
            if (scoreValuePlayerOne != null)
            {
                scoreValuePlayerOne.text = string.Format("{0}", gameplayController.ScorePlayerOne);
            }
            if (scoreValuePlayerTwo != null)
            {
                scoreValuePlayerTwo.text = string.Format("{0}", gameplayController.ScorePlayerTwo);
            }
            if (highscoreValue != null)
            {
                highscoreValue.text = string.Format("{0}", gameplayController.HighScore);
            }
            if (roundsInfoText != null)
            {
                roundsInfoText.text = string.Format("ROUND {0}", gameplayController.NumberOfRounds);
            }

            for(int i = 0; i < livesPlayerOne.Length; i++)
            {
                bool visible = (i < gameplayController.LivesPlayerOne);
                if (livesPlayerOne[i] != null)
                {
                    livesPlayerOne[i].enabled = visible;
                }
            }
        }

        private void SubscribeToNotifications()
        {
            NotificationCenter.DefaultCenter.AddSubscriber(GameplayController.GameWillEnd, this, "GameWillEndEvent");
        }

        private void GameWillEndEvent(Notification notification)
        {
            if (gameoverText != null)
            {
                gameoverText.enabled = true;
            }
        }
    }
}