using UnityEngine;
using PrismPulse.Core;
using PrismPulse.Utils;
using PrismPulse.Audio;
using PrismPulse.Effects;
using System.Collections;

namespace PrismPulse.Pieces
{
    public class PieceVisual : MonoBehaviour
    {
        private BlockPiece pieceData;
        private Vector3 startPos;
        private bool isDragging = false;
        private int slotIndex = -1;
        private PieceSpawner spawner;
        private Transform visuals;

        public void Initialize(BlockPiece data, int idx, PieceSpawner sp)
        {
            pieceData = data; slotIndex = idx; spawner = sp; startPos = transform.position;
            ConstructVisuals();
            transform.localScale = Vector3.zero;
            StartCoroutine(ScaleAnim(Vector3.one * Constants.PIECE_START_SCALE, 0.2f));
        }

        private void ConstructVisuals()
        {
            foreach (Transform child in transform) DestroyImmediate(child.gameObject);
            
            // Logic: Calculate center offset
            Vector2 min = new Vector2(10, 10); Vector2 max = new Vector2(-10, -10);
            foreach (var off in pieceData.shapeOffsets) { 
                min.x = Mathf.Min(min.x, off.x); max.x = Mathf.Max(max.x, off.x);
                min.y = Mathf.Min(min.y, off.y); max.y = Mathf.Max(max.y, off.y);
            }
            Vector2 center = (min + max) / 2f;

            GameObject container = new GameObject("Visuals");
            visuals = container.transform;
            visuals.SetParent(transform);
            // Pivot offset
            visuals.localPosition = new Vector3(-center.x * Constants.CELL_SIZE, -center.y * Constants.CELL_SIZE, 0);

            Sprite blockSprite = BoardManager.Instance.GetBlockSprite();
            foreach (var off in pieceData.shapeOffsets)
            {
                GameObject block = new GameObject("Block");
                block.transform.SetParent(visuals);
                block.transform.localPosition = new Vector3(off.x * Constants.CELL_SIZE, off.y * Constants.CELL_SIZE, 0);
                var sr = block.AddComponent<SpriteRenderer>();
                sr.sprite = blockSprite;
                Color.RGBToHSV(pieceData.blockColor, out float h, out float s, out float v);
                sr.color = Color.HSVToRGB(h, Mathf.Clamp01(s * 1.4f), Mathf.Max(v, 0.8f));
                sr.sortingOrder = 20;
                block.transform.localScale = Vector3.one * Constants.BOARD_VISUAL_SCALE;
            }
        }

        private void Update()
        {
            if (isDragging)
            {
                Vector3 mouse = Input.mousePosition; mouse.z = 10f;
                Vector3 cur = Camera.main.ScreenToWorldPoint(mouse);
                transform.position = Vector3.Lerp(transform.position, new Vector3(cur.x, cur.y + 2.0f, 0), Time.deltaTime * Constants.ANIM_SPEED);
            }
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            isDragging = true;
            if (SoundManager.Instance) SoundManager.Instance.PlayPop();
            StartCoroutine(ScaleAnim(Vector3.one, 0.1f));
        }

        private void OnMouseUp()
        {
            if (!isDragging) return; isDragging = false;
            
            Vector2Int gridPos = BoardManager.Instance.WorldToGrid(transform.position);
            bool canPlace = BoardManager.Instance.CanPlace(pieceData, gridPos);

            if (!canPlace) {
                // Area Snap
                float minD = 2f; Vector2Int best = gridPos; bool found = false;
                for (int x=-2; x<=2; x++) for (int y=-2; y<=2; y++) {
                    Vector2Int near = gridPos + new Vector2Int(x, y);
                    if (BoardManager.Instance.CanPlace(pieceData, near)) {
                        float d = Vector3.Distance(transform.position, BoardManager.Instance.GetWorldPosition(near.x, near.y));
                        if (d < minD) { minD = d; best = near; found = true; }
                    }
                }
                if (found) { gridPos = best; canPlace = true; }
            }

            if (canPlace) {
                if (SoundManager.Instance) SoundManager.Instance.PlayPlace();
                BoardManager.Instance.PlacePiece(pieceData, gridPos);
                if (spawner) spawner.OnPieceUsed(slotIndex);
                GameManager.Instance.ProcessTurn(BoardManager.Instance.CheckAndClearLines().Count, pieceData.shapeOffsets.Count);
                Destroy(gameObject);
            } else {
                StartCoroutine(ReturnToTray());
            }
        }

        private IEnumerator ReturnToTray() {
            float e = 0; Vector3 c = transform.position;
            while (e < 0.2f) { transform.position = Vector3.Lerp(c, startPos, e/0.2f); transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * Constants.PIECE_START_SCALE, e/0.2f); e += Time.deltaTime; yield return null; }
            transform.position = startPos; transform.localScale = Vector3.one * Constants.PIECE_START_SCALE;
        }

        private IEnumerator ScaleAnim(Vector3 t, float d) {
            float e = 0; Vector3 s = transform.localScale;
            while (e < d) { transform.localScale = Vector3.Lerp(s, t, e/d); e += Time.deltaTime; yield return null; }
            transform.localScale = t;
        }

        public BlockPiece GetPieceData() => pieceData;
    }
}