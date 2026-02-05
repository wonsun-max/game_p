using UnityEngine;
using PrismPulse.Utils;
using System.Collections;

namespace PrismPulse.Effects
{
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager Instance { get; private set; }

        private void Awake() 
        { 
            if (Instance == null) Instance = this; 
            else Destroy(gameObject); 
        }

        public void ShakeCamera(float intensity, float duration)
        {
            StartCoroutine(DoShake(intensity, duration));
        }

        private IEnumerator DoShake(float intensity, float duration)
        {
            Vector3 originalPos = Camera.main.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                Camera.main.transform.position = originalPos + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Camera.main.transform.position = originalPos;
        }

        public void PlayBlockClearEffect(Vector3 pos, Color color)
        {
            for (int i = 0; i < 8; i++)
            {
                GameObject shard = new GameObject("Shard");
                shard.transform.position = pos;
                var sr = shard.AddComponent<SpriteRenderer>();
                sr.sprite = CreateShardSprite();
                sr.color = color;
                sr.sortingOrder = 50;
                StartCoroutine(AnimateShard(shard));
            }
        }

        private Sprite CreateShardSprite()
        {
            int res = 32;
            Texture2D tex = new Texture2D(res, res);
            for (int y = 0; y < res; y++)
                for (int x = 0; x < res; x++)
                {
                    float u = (x / (float)res) * 2f - 1f;
                    float v = (y / (float)res) * 2f - 1f;
                    bool inside = (Mathf.Abs(u) + Mathf.Abs(v)) < 0.8f;
                    tex.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        }

        private IEnumerator AnimateShard(GameObject shard)
        {
            SpriteRenderer sr = shard.GetComponent<SpriteRenderer>();
            Vector3 velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized * Random.Range(3f, 6f);
            float rotationSpeed = Random.Range(-500f, 500f);
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                shard.transform.position += velocity * Time.deltaTime;
                shard.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
                float t = elapsed / duration;
                shard.transform.localScale = Vector3.one * (1f - t);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - t);
                velocity *= 0.95f;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(shard);
        }

        public void PlayPulse(Vector3 pos, Color color, float size = 4f)
        {
            GameObject p = new GameObject("Pulse");
            p.transform.position = pos;
            var sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = CreateShardSprite();
            sr.color = new Color(color.r, color.g, color.b, 0.4f);
            StartCoroutine(AnimatePulse(p, size));
        }

        private IEnumerator AnimatePulse(GameObject p, float size)
        {
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.4f;
                p.transform.localScale = Vector3.one * (t * size);
                p.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, (1f - t) * 0.4f);
                yield return null;
            }
            Destroy(p);
        }
    }
}