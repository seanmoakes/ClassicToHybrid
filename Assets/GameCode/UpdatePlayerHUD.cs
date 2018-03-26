using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace TwoStickClassicExample
{
    [AlwaysUpdateSystem]
    public class UpdatePlayerHUD : ComponentSystem
    {
        public struct PlayerData
        {
            public int Length;
            public EntityArray Entity;
            public ComponentArray<PlayerInput> Input;
            public ComponentArray<Health> Health;
        }

        [Inject] PlayerData m_Players;
        // needs initial value
        private float m_CachedHealth = Int32.MinValue;

        public Button NewGameButton;
        public Text HealthText;

        private void Start()
        {
            // Find these in the game.
            NewGameButton = GameObject.Find("NewGameButton").GetComponent<Button>();
            HealthText = GameObject.Find("HealthText").GetComponent<Text>();
            NewGameButton.onClick.AddListener(TwoStickBootstrap.NewGame);
        }


        protected override void OnUpdate()
        {
            //if (player != null)
            if(m_Players.Length > 0)
            {
                UpdateAlive();
            }
            else
            {
                UpdateDead();
            }
        }

        private void UpdateDead()
        {
            if (HealthText != null)
            {
                HealthText.gameObject.SetActive(false);
            }
            if (NewGameButton != null)
            {
                NewGameButton.gameObject.SetActive(true);
            }
        }

        //private void UpdateAlive(PlayerInput playerInput)
        private void UpdateAlive()
        {
            HealthText.gameObject.SetActive(true);
            NewGameButton.gameObject.SetActive(false);
            
            //var displayedHealth = 0;
            //if (playerInput != null)
            //{
            //  displayedHealth = (int) playerInput.GetComponent<Health>().Value;
            //}
            
            /*
             SM - no need to check playerInput, would not be able to
             get here if it was null.
             */
            int displayedHealth = (int) m_Players.Health[0].Value;

            if (m_CachedHealth != displayedHealth)
            {
                if (displayedHealth > 0)
                    HealthText.text = $"HEALTH: {displayedHealth}";
                // else
                //    HealthText.text = "GAME OVER";
                /*
                 SM - The game immediately returns to the start screen,
                 therefore it is unnecessary to display "GAME OVER".
                 */
                m_CachedHealth = displayedHealth;
            }
        }
    }
}