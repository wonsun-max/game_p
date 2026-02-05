using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PrismPulse.UI
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        private RawImage characterImage;
        private RectTransform rectTransform;
        private float pulseSpeed = 3f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            CreateCharacterVisuals();
        }

        private void CreateCharacterVisuals()
        {
            GameObject charObj = new GameObject("PrismMascot");
            Canvas hudCanvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (hudCanvas != null) charObj.transform.SetParent(hudCanvas.transform);

            rectTransform = charObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector3(0, -220, 0);
            rectTransform.sizeDelta = new Vector2(250, 250);

            characterImage = charObj.AddComponent<RawImage>();
            characterImage.texture = CreatePrismTexture();
            StartCoroutine(AnimatePulse());
        }

        private Texture2D CreatePrismTexture()
        {
            int res = 256; Texture2D tex = new Texture2D(res, res);
            for (int y = 0; y < res; y++) for (int x = 0; x < res; x++) {
                float u = (x / (float)res) * 2f - 1f; float v = (y / (float)res) * 2f - 1f;
                float d = Mathf.Abs(u) + Mathf.Abs(v);
                if (d < 0.8f) {
                    float facet = Mathf.Sin(u * 12f) * Mathf.Cos(v * 12f);
                    float brightness = 0.85f + facet * 0.15f;
                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
                }
                else tex.SetPixel(x, y, Color.clear);
            }
            tex.Apply(); return tex;
        }

        private IEnumerator AnimatePulse()
        {
            float t = 0;
            while (true)
            {
                t += Time.deltaTime;
                // DRAMATIC ROTATION AND PULSE
                float s = 1.0f + Mathf.Sin(t * pulseSpeed) * 0.15f;
                rectTransform.localScale = new Vector3(s, s, 1f);
                rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.5f) * 15f);
                
                // RAINBOW SHIMMER
                Color.RGBToHSV(Color.cyan, out float h, out float sat, out float v);
                characterImage.color = Color.HSVToRGB((h + t * 0.05f) % 1.0f, 0.6f, 1f);
                yield return null;
            }
        }

        public void ReactToLineClear()
        {
            StopCoroutine("EnergySurge");
            StartCoroutine(EnergySurge());
        }

        private IEnumerator EnergySurge()
        {
            float d = 0.2f; float e = 0f;
            pulseSpeed = 10f; // Speed up mascot during clear
            while(e < d) {
                rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 2.5f, e/d);
                e += Time.deltaTime; yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            rectTransform.localScale = Vector3.one;
            pulseSpeed = 3f;
        }
    }
}