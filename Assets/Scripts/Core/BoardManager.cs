using System.Collections.Generic;
using UnityEngine;
using PrismPulse.Pieces;
using PrismPulse.Utils;
using PrismPulse.Effects;
using System.Linq;

namespace PrismPulse.Core
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        private int[,] grid;
        private GameObject[,] cellVisuals;
        private Transform gridOrigin; 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
            
            grid = new int[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
            cellVisuals = new GameObject[Constants.BOARD_SIZE, Constants.BOARD_SIZE];

            // 1. Scene Purge
            PurgeLegacyObjects();

            // 2. Setup Center
            gridOrigin = new GameObject("GridOrigin").transform;
            gridOrigin.SetParent(transform);
            float offset = (Constants.BOARD_SIZE * Constants.CELL_SIZE) / 2f;
            gridOrigin.position = new Vector3(-offset, -offset, 0);
        }

        private void PurgeLegacyObjects()
        {
            GameObject[] all = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in all)
            {
                if (obj.name.Contains("Square") || obj.name.Contains("Background") || obj.name.Contains("New Game Object"))
                {
                    if (obj.transform.root.name != "Core" && obj.transform.root.name != "GameManager" && obj.name != "Canvas")
                        DestroyImmediate(obj);
                }
            }
        }

        public void InitializeBoard()
        {
            // Clear existing
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    if (cellVisuals[x, y] != null) DestroyImmediate(cellVisuals[x, y]);

            cellVisuals = new GameObject[8, 8];
            grid = new int[8, 8];

            CreateTraySlots();

            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    CreateCellVisual(x, y, new Color(1, 1, 1, 0.05f));
        }

        private void CreateTraySlots()
        {
            float[] xPos = { -3.2f, 0f, 3.2f };
            for (int i = 0; i < 3; i++)
            {
                GameObject slot = GameObject.Find($"TraySlot_{i}") ?? new GameObject($"TraySlot_{i}");
                slot.transform.position = new Vector3(xPos[i], -6.5f, 0);
                slot.transform.localScale = Vector3.one;
                foreach (Transform child in slot.transform) DestroyImmediate(child.gameObject);
            }
        }

        public void CreateCellVisual(int x, int y, Color color)
        {
            if (cellVisuals[x, y] != null) DestroyImmediate(cellVisuals[x, y]);
            GameObject obj = new GameObject($"C_{x}_{y}");
            obj.transform.position = GetWorldPosition(x, y);
            obj.transform.SetParent(gridOrigin);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBlockSprite();
            sr.color = color;
            sr.material = Canvas.GetDefaultCanvasMaterial();
            obj.transform.localScale = Vector3.one * Constants.BOARD_VISUAL_SCALE;
            cellVisuals[x, y] = obj;
        }

        public Sprite GetBlockSprite()
        {
            int res = 128; Texture2D tex = new Texture2D(res, res);
            for (int y = 0; y < res; y++) for (int x = 0; x < res; x++) {
                float u = (x / (float)res) * 2f - 1f; float v = (y / (float)res) * 2f - 1f;
                float d = Mathf.Max(Mathf.Abs(u), Mathf.Abs(v));
                if (d < 0.95f) {
                    float edge = d > 0.85f ? 0.5f : 1.0f;
                    tex.SetPixel(x, y, new Color(edge, edge, edge, 1f));
                } else tex.SetPixel(x, y, Color.clear);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
        }

        public Vector3 GetWorldPosition(int x, int y) => gridOrigin.position + new Vector3(x * Constants.CELL_SIZE + (Constants.CELL_SIZE/2), y * Constants.CELL_SIZE + (Constants.CELL_SIZE/2), 0);
        
        public Vector2Int WorldToGrid(Vector3 pos) {
            Vector3 local = pos - gridOrigin.position;
            return new Vector2Int(Mathf.FloorToInt(local.x/Constants.CELL_SIZE), Mathf.FloorToInt(local.y/Constants.CELL_SIZE));
        }

        public bool CanPlace(BlockPiece piece, Vector2Int pos) {
            if (piece == null) return false;
            foreach (var off in piece.shapeOffsets) {
                int tx = pos.x + off.x; int ty = pos.y + off.y;
                if (tx < 0 || tx >= 8 || ty < 0 || ty >= 8 || grid[tx, ty] != 0) return false;
            }
            return true;
        }

        public void PlacePiece(BlockPiece piece, Vector2Int pos) {
            Color.RGBToHSV(piece.blockColor, out float h, out float s, out float v);
            Color vibrant = Color.HSVToRGB(h, Mathf.Clamp01(s * 1.4f), Mathf.Max(v, 0.8f));
            foreach (var off in piece.shapeOffsets) {
                int tx = pos.x + off.x; int ty = pos.y + off.y;
                grid[tx, ty] = 1;
                CreateCellVisual(tx, ty, vibrant);
            }
        }

        public List<int> CheckAndClearLines() {
            List<int> rows = new List<int>(); List<int> cols = new List<int>();
            for (int y = 0; y < 8; y++) { bool full = true; for (int x = 0; x < 8; x++) if (grid[x, y] == 0) full = false; if (full) rows.Add(y); }
            for (int x = 0; x < 8; x++) { bool full = true; for (int y = 0; y < 8; y++) if (grid[x, y] == 0) full = false; if (full) cols.Add(x); }
            
            foreach (int y in rows) for (int x = 0; x < 8; x++) ClearCell(x, y);
            foreach (int x in cols) for (int y = 0; y < 8; y++) ClearCell(x, y);
            
            if (rows.Count > 0 || cols.Count > 0) EffectsManager.Instance.ShakeCamera(0.15f * (rows.Count + cols.Count), 0.2f);
            return rows.Concat(cols).ToList();
        }

        private void ClearCell(int x, int y) {
            if (cellVisuals[x, y] != null) {
                EffectsManager.Instance.PlayBlockClearEffect(cellVisuals[x, y].transform.position, cellVisuals[x, y].GetComponent<SpriteRenderer>().color);
                DestroyImmediate(cellVisuals[x, y]);
            }
            grid[x, y] = 0;
            CreateCellVisual(x, y, new Color(1, 1, 1, 0.05f));
        }

        public int[] GetGridState() { 
            int[] flat = new int[64]; 
            for (int y=0; y<8; y++) for (int x=0; x<8; x++) flat[y*8+x] = grid[x,y]; 
            return flat; 
        }

        public void ShowPreview(BlockPiece piece, Vector2Int pos) { /* Simplified for performance */ }
        public void ClearPreview() { }
        public bool IsBoardEmpty() { foreach(int i in grid) if(i != 0) return false; return true; }
    }
}
