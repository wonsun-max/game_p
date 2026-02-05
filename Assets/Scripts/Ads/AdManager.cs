using System;
using UnityEngine;

namespace PrismPulse.Ads
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ShowInterstitial(Action onComplete)
        {
            Debug.Log("AdManager: Show Interstitial");
            onComplete?.Invoke();
        }

        public void ShowRewarded(Action onSuccess, Action onFail)
        {
            Debug.Log("AdManager: Show Rewarded");
            onSuccess?.Invoke();
        }
    }
}