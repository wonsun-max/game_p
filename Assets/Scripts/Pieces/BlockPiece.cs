using System.Collections.Generic;
using UnityEngine;

namespace PrismPulse.Pieces
{
    [CreateAssetMenu(fileName = "NewBlockPiece", menuName = "PrismPulse/BlockPiece")]
    public class BlockPiece : ScriptableObject
    {
        public List<Vector2Int> shapeOffsets;
        public Color blockColor = Color.white;
    }
}