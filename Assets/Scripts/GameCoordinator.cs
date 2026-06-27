using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameCoordinator : MonoBehaviour
    {
        [Description("Should get auto generated on game load")]
        public SpinCharacterController[] allPlayers;
        public void Awake()
        {
            allPlayers = GameObject.FindObjectsByType<SpinCharacterController>(FindObjectsSortMode.None);
            BattleEvents.OnToppleEvent += CheckForEndOfGame;
            BattleEvents.OnStartGameEvent += ResetWorld;
        }
        public void CheckForEndOfGame(SpinCharacterController deadUnit)
        {
            if (deadUnit.tag == "Player")
            {
                BattleEvents.InvokeEndGameEvent(gameWasWon: false);
            }
            else
            {
                BattleEvents.InvokeEndGameEvent(gameWasWon: true);
            }
        }

        public void ResetWorld()
        {
            foreach (var unit in allPlayers)
            {
                unit.ResetToOriginalState();
            }
        }
    }
}
