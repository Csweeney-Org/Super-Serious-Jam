using TMPro;
using UnityEngine;

namespace Assets.Scripts.IngameUI
{
    public class EndGameMenu : MonoBehaviour
    {
        //Making the music triggers public
        public AK.Wwise.Event Win_State;
        public AK.Wwise.Event Lose_State;
    
        public TextMeshProUGUI PanelText;
        public void Awake()
        {
            BattleEvents.OnEndGameEvent += ShowEndScreen;
            BattleEvents.OnStartGameEvent += Hide;
            this.Hide();
        }
        public void ShowEndScreen(bool gameIsWon)
        {
            this.gameObject.SetActive(true);
            AkUnitySoundEngine.PostEvent("Mus_Battle_WinState", gameObject);
            PanelText.text = gameIsWon
                ? "You have toppled the enemy! You Win!"
                : "You have toppled! Game Over!";

        }
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public void ResetGameButtonFunc()
        {
            BattleEvents.InvokeStartGameEvent();
        }
        public void ExitGameButtonFunc()
        {
            Application.Quit();
        }
        public void OnDestroy()
        {
            BattleEvents.OnEndGameEvent -= ShowEndScreen;
            BattleEvents.OnStartGameEvent -= Hide;
        }
    }
}
