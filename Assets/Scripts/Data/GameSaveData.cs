using System.Collections.Generic;
using UnityEngine;

namespace PrismPulse.Data
{
    [System.Serializable]
    public class GameSaveData
    {
        public int Score;
        public int HighScore;
        public int GameMode;
        public int[] GridData;
        public List<PieceSaveData> ActivePieces;
    }

    [System.Serializable]
    public class PieceSaveData
    {
        public int SlotIndex;
        public int PieceLibraryIndex;
    }
}