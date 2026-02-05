using System.Collections.Generic;
using UnityEngine;
using PrismPulse.Pieces;
using PrismPulse.Utils;
using PrismPulse.Data;

namespace PrismPulse.Core
{
    public class PieceSpawner : MonoBehaviour
    {
        public static PieceSpawner Instance { get; private set; }

        [SerializeField] private List<BlockPiece> allBlockPieces;
        [SerializeField] private Transform[] spawnPoints; 
        
        private GameObject[] activePieces;
        
        public int GetPieceIndex(BlockPiece piece) { return allBlockPieces != null ? allBlockPieces.IndexOf(piece) : -1; }
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
            InitializeSpawner();
        }

        private void InitializeSpawner()
        {
            if (allBlockPieces == null || allBlockPieces.Count == 0)
                allBlockPieces = new List<BlockPiece>(Resources.LoadAll<BlockPiece>("Pieces"));

            if (spawnPoints == null || spawnPoints.Length == 0 || spawnPoints[0] == null)
            {
                spawnPoints = new Transform[3];
                for (int i = 0; i < 3; i++)
                {
                    GameObject slot = GameObject.Find($"TraySlot_{i}");
                    if (slot != null) spawnPoints[i] = slot.transform;
                }
            }
            activePieces = new GameObject[spawnPoints.Length];
        }

        public void SpawnPieces()
        {
            if (activePieces == null || activePieces.Length == 0) InitializeSpawner();
            int count = GameModeManager.Instance.GetPiecesToSpawnCount();
            if (count == 1) SpawnSinglePiece(1);
            else for (int i = 0; i < spawnPoints.Length; i++) if (activePieces[i] == null) SpawnSinglePiece(i);
        }

        private void SpawnSinglePiece(int index)
        {
            if (index < 0 || index >= spawnPoints.Length) return;
            if (spawnPoints[index] == null || activePieces[index] != null) return;
            BlockPiece pieceData = GetSmartPiece();
            SpawnPieceAtSlot(index, pieceData);
        }

        private BlockPiece GetSmartPiece()
        {
            if (allBlockPieces == null || allBlockPieces.Count == 0) return null;
            List<BlockPiece> fits = new List<BlockPiece>();
            foreach (var piece in allBlockPieces) {
                for (int x = 0; x < 8; x++) {
                    for (int y = 0; y < 8; y++) {
                        if (BoardManager.Instance.CanPlace(piece, new Vector2Int(x, y))) { fits.Add(piece); goto NextPiece; }
                    }
                }
                NextPiece:;
            }
            return fits.Count > 0 ? fits[Random.Range(0, fits.Count)] : allBlockPieces[Random.Range(0, allBlockPieces.Count)];
        }

        public void SpawnSpecificPiece(int slotIndex, int pieceLibraryIndex)
        {
            if (activePieces == null || slotIndex < 0 || slotIndex >= activePieces.Length) return;
            SpawnPieceAtSlot(slotIndex, allBlockPieces[pieceLibraryIndex]);
        }

        private void SpawnPieceAtSlot(int index, BlockPiece pieceData)
        {
            GameObject pieceObj = new GameObject($"Piece_{index}");
            pieceObj.transform.SetParent(spawnPoints[index]);
            pieceObj.transform.localPosition = Vector3.zero;
            pieceObj.transform.localScale = Vector3.one;

            // PRE-EMPTIVE COMPONENT ADDITION to prevent MissingComponentException
            pieceObj.AddComponent<BoxCollider2D>();
            var visual = pieceObj.AddComponent<PieceVisual>();
            activePieces[index] = pieceObj;
            visual.Initialize(pieceData, index, this);
        }
        
        public void ClearAllPieces()
        {
            if (activePieces == null) return;
            for(int i=0; i<activePieces.Length; i++) {
                if(activePieces[i] != null) DestroyImmediate(activePieces[i]);
                activePieces[i] = null;
            }
        }

        public void OnPieceUsed(int index)
        {
            if (index >= 0 && index < activePieces.Length) activePieces[index] = null;
            if (GameModeManager.Instance.ShouldRefillImmediately()) SpawnPieces();
            else if (!HasActivePieces()) SpawnPieces();
        }
        
        public bool HasActivePieces() { if (activePieces == null) return false; foreach (var p in activePieces) if (p != null) return true; return false; }

        public bool CanAnyPieceBePlaced()
        {
             if (activePieces == null) return false;
             foreach (var pObj in activePieces) {
                 if (pObj == null) continue;
                 var visual = pObj.GetComponent<PieceVisual>();
                 if (visual != null && visual.GetPieceData() != null)
                     for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++)
                         if (BoardManager.Instance.CanPlace(visual.GetPieceData(), new Vector2Int(x, y))) return true;
             }
             return false;
        }

        public List<PieceSaveData> GetActivePiecesSnapshots()
        {
            List<PieceSaveData> list = new List<PieceSaveData>();
            if (activePieces == null) return list;
            for (int i = 0; i < activePieces.Length; i++) {
                if (activePieces[i] != null) {
                     var visual = activePieces[i].GetComponent<PieceVisual>();
                     if (visual != null && visual.GetPieceData() != null) {
                         int libIndex = GetPieceIndex(visual.GetPieceData());
                         if (libIndex != -1) list.Add(new PieceSaveData { SlotIndex = i, PieceLibraryIndex = libIndex });
                     }
                }
            }
            return list;
        }
    }
}
