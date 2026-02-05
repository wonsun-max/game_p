using UnityEngine;
using UnityEngine.UI;
using PrismPulse.Core;
using PrismPulse.Utils;
using PrismPulse.Audio;
using System.Collections;

namespace PrismPulse.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject gameOverPanel;

        private GameObject settingsPanel;
        private Button settingsButton;
        private RectTransform safeArea;
        private Text scoreTxt;
        private Text highTxt;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            BuildUI();
        }

        private void BuildUI()
        {
            // 1. Find or Create Root Canvas
            GameObject canvasObj = GameObject.Find("Canvas") ?? new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas c = canvasObj.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 100;
            
            CanvasScaler cs = canvasObj.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; cs.referenceResolution = new Vector2(1080, 1920);

            // 2. Safe Area Container
            GameObject saObj = new GameObject("SafeArea", typeof(RectTransform));
            saObj.transform.SetParent(canvasObj.transform, false);
            safeArea = saObj.GetComponent<RectTransform>();
            safeArea.anchorMin = Vector2.zero; safeArea.anchorMax = Vector2.one; safeArea.sizeDelta = Vector2.zero;
            
            Rect rect = Screen.safeArea;
            safeArea.anchorMin = new Vector2(rect.x / Screen.width, rect.y / Screen.height);
            safeArea.anchorMax = new Vector2((rect.x + rect.width) / Screen.width, (rect.y + rect.height) / Screen.height);

            // 3. Settings Button
            GameObject sb = new GameObject("SettingsBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            sb.transform.SetParent(safeArea, false);
            settingsButton = sb.GetComponent<Button>();
            RectTransform sbrt = sb.GetComponent<RectTransform>();
            sbrt.anchorMin = sbrt.anchorMax = sbrt.pivot = new Vector2(1, 1);
            sbrt.anchoredPosition = new Vector2(-40, -40); sbrt.sizeDelta = new Vector2(120, 120);
            sb.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            
            GameObject tObj = new GameObject("T", typeof(Text)); tObj.transform.SetParent(sb.transform, false);
            Text st = tObj.GetComponent<Text>(); st.text = "SET"; st.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            st.alignment = TextAnchor.MiddleCenter; st.fontSize = 35; st.fontStyle = FontStyle.Bold;
            ((RectTransform)tObj.transform).sizeDelta = sbrt.sizeDelta;
            settingsButton.onClick.AddListener(() => ToggleSettings(true));

            // 4. Score HUD
            GameObject scoreObj = new GameObject("ScoreHUD", typeof(RectTransform), typeof(Text));
            scoreObj.transform.SetParent(safeArea, false);
            scoreTxt = scoreObj.GetComponent<Text>(); scoreTxt.font = st.font;
            scoreTxt.fontSize = 120; scoreTxt.alignment = TextAnchor.UpperCenter; scoreTxt.color = Color.white;
            RectTransform scrt = scoreObj.GetComponent<RectTransform>();
            scrt.anchorMin = scrt.anchorMax = new Vector2(0.5f, 1f); scrt.anchoredPosition = new Vector2(0, -100);
            scrt.sizeDelta = new Vector2(600, 200);

            // 5. Settings Panel
            settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
            settingsPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform sprt = settingsPanel.GetComponent<RectTransform>();
            sprt.anchorMin = Vector2.zero; sprt.anchorMax = Vector2.one; sprt.sizeDelta = Vector2.zero;
            settingsPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.95f);
            settingsPanel.SetActive(false);
            
            CreateBtn(settingsPanel.transform, "RESUME", 0, () => ToggleSettings(false));
            CreateBtn(settingsPanel.transform, "EXIT", -150, () => GameManager.Instance.ExitToMenu());
        }

        private void CreateBtn(Transform p, string label, float y, UnityEngine.Events.UnityAction act) {
            GameObject b = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            b.transform.SetParent(p, false);
            RectTransform rt = b.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(450, 130); rt.anchoredPosition = new Vector2(0, y);
            b.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            b.GetComponent<Button>().onClick.AddListener(act);
            GameObject t = new GameObject("T", typeof(Text)); t.transform.SetParent(b.transform, false);
            Text tx = t.GetComponent<Text>(); tx.text = label; tx.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tx.alignment = TextAnchor.MiddleCenter; tx.fontSize = 40; tx.color = Color.white;
            ((RectTransform)t.transform).sizeDelta = rt.sizeDelta;
        }

        private void ToggleSettings(bool on) => settingsPanel.SetActive(on);

        private void Start()
        {
            GameManager.Instance.OnScoreChanged += (s) => { if(scoreTxt) scoreTxt.text = s.ToString(); };
            GameManager.Instance.OnStateChanged += UpdatePanels;
            UpdatePanels(GameManager.Instance.CurrentState);
        }

        private void UpdatePanels(GameState s) {
            if(startPanel) startPanel.SetActive(s == GameState.Start);
            if(gamePanel) gamePanel.SetActive(s == GameState.Playing);
            if(gameOverPanel) gameOverPanel.SetActive(s == GameState.GameOver);
            if(settingsButton) settingsButton.gameObject.SetActive(s == GameState.Playing);
            if(settingsPanel) settingsPanel.SetActive(false);
        }

        public void SpawnScorePopup(int a, Vector3 pos) { /* Simplified Animation */ }
    }
}