using UnityEngine;
using UnityEngine.UI;

namespace PrismPulse.UI
{
    public class BackgroundManager : MonoBehaviour
    {
        public static BackgroundManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            CreateBackground();
        }

        private void CreateBackground()
        {
            GameObject bgCanvasObj = new GameObject("PremiumBackgroundCanvas");
            Canvas canvas = bgCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -100;

            CanvasScaler scaler = bgCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            GameObject bgObj = new GameObject("ProBackground");
            bgObj.transform.SetParent(bgCanvasObj.transform);
            Image img = bgObj.AddComponent<Image>();
            
            RectTransform rect = bgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // Apple Pro Dark: Solid Deep Gray
            img.color = new Color(0.05f, 0.05f, 0.07f, 1f); 
            
            // Subtle glow
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(bgObj.transform);
            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.sprite = CreateRadialSprite();
            glowImg.color = new Color(1, 1, 1, 0.03f); 
            
            RectTransform glowRect = glowObj.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0, 0.2f);
            glowRect.anchorMax = new Vector2(1, 0.8f);
            glowRect.sizeDelta = Vector2.zero;
        }

        private Sprite CreateRadialSprite()
        {
            int res = 256;
            Texture2D tex = new Texture2D(res, res);
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(res / 2f, res / 2f)) / (res / 2f);
                    float alpha = Mathf.Clamp01(1.0f - d);
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha * alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        }
    }
}
