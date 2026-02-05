using UnityEngine;
using PrismPulse.Utils;

namespace PrismPulse.Core
{
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }
        public GameMode CurrentMode { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                CurrentMode = GameMode.Classic;
            }
            else Destroy(gameObject);
        }

        public void SetMode(GameMode mode) { CurrentMode = mode; }
        public int GetPiecesToSpawnCount() { return CurrentMode == GameMode.Blind ? 1 : 3; }
        public bool ShouldRefillImmediately() { return CurrentMode == GameMode.Blind; }
        public string GetHighScoreKey() { return Constants.PREF_HIGHSCORE_PREFIX + CurrentMode.ToString(); }
    }
}