using System;
using UnityEngine;
using PrismPulse.Ads;
using PrismPulse.Utils;
using PrismPulse.Data;
using PrismPulse.Effects;
using PrismPulse.Audio;
using PrismPulse.UI;

namespace PrismPulse.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState CurrentState { get; private set; }
        public int Score { get; private set; }
        public int HighScore { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<GameState> OnStateChanged;

        [SerializeField] private PieceSpawner pieceSpawner;
        [SerializeField] private BoardManager boardManager;

        private void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        private void Start()
        {
            if (boardManager == null) boardManager = BoardManager.Instance;
            if (pieceSpawner == null) pieceSpawner = PieceSpawner.Instance;
            SetState(GameState.Start);
        }

        public void StartGame(GameMode mode)
        {
            SaveManager.DeleteSave();
            GameModeManager.Instance.SetMode(mode);
            Score = 0; LoadHighScore();
            OnScoreChanged?.Invoke(Score);
            
            boardManager.InitializeBoard();
            pieceSpawner.ClearAllPieces(); pieceSpawner.SpawnPieces();
            
            SetState(GameState.Playing);
        }

        public void ProcessTurn(int linesCleared, int blocksPlaced)
        {
            int p = blocksPlaced * Constants.SCORE_PER_CELL;
            if (linesCleared > 0)
            {
                int baseScore = linesCleared * Constants.SCORE_PER_LINE;
                if (linesCleared > 1) baseScore += (linesCleared - 1) * Constants.SCORE_MULTI_LINE_BONUS;
                p += baseScore;
                if (SoundManager.Instance) SoundManager.Instance.PlayLineClear();
            }
            if (boardManager.IsBoardEmpty()) p += 1000;
            
            Score += p;
            OnScoreChanged?.Invoke(Score);
            if (Score > HighScore) { HighScore = Score; PlayerPrefs.SetInt(GameModeManager.Instance.GetHighScoreKey(), HighScore); }

            if (UIManager.Instance && p > 0) UIManager.Instance.SpawnScorePopup(p, Vector3.zero);

            if (!pieceSpawner.CanAnyPieceBePlaced()) GameOver();
            else SaveGame();
        }

        public void GameOver() { SetState(GameState.GameOver); SaveManager.DeleteSave(); }
        public void RestartGame() => StartGame(GameModeManager.Instance.CurrentMode);
        public void ExitToMenu() { SaveGame(); UnityEngine.SceneManagement.SceneManager.LoadScene(0); }
        public void ContinueGame() { if (SaveManager.HasSaveFile()) LoadSavedGame(); }
        
        private void SetState(GameState s) { CurrentState = s; OnStateChanged?.Invoke(s); }
        private void LoadHighScore() { HighScore = PlayerPrefs.GetInt(GameModeManager.Instance.GetHighScoreKey(), 0); }
        private void SaveGame() { if (CurrentState != GameState.Playing) return; GameSaveData d = new GameSaveData { Score = Score, HighScore = HighScore, GameMode = (int)GameModeManager.Instance.CurrentMode, GridData = boardManager.GetGridState(), ActivePieces = pieceSpawner.GetActivePiecesSnapshots() }; SaveManager.Save(d); }
        private void LoadSavedGame() { /* Simplified */ }
    }
}
