using UnityEngine;
using PrismPulse.Utils;

namespace PrismPulse.Core
{
    [RequireComponent(typeof(Camera))]
    public class CameraScalar : MonoBehaviour
    {
        private void Start()
        {
            AdjustCamera();
        }

        private void AdjustCamera()
        {
            Camera cam = GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f, 1f); 

            float boardSize = Constants.BOARD_SIZE * Constants.CELL_SIZE;
            float currentAspect = (float)Screen.width / Screen.height;
            
            // PLATFORM SPECIFIC RATIO & PADDING
            float verticalPadding = 8.0f; // Default
            float sidePadding = 2.0f;

            #if UNITY_IOS
            // iOS devices often have narrower aspect ratios (e.g. 19.5:9) and notches
            verticalPadding = 9.5f; // More space for notch and home indicator
            sidePadding = 1.5f;
            #elif UNITY_ANDROID
            // Android varies, but we often see wider 20:9 or 21:9
            verticalPadding = 8.5f;
            sidePadding = 2.0f;
            #endif

            float totalHeightNeeded = boardSize + verticalPadding;
            float minWidthNeeded = boardSize + sidePadding;
            
            // Fit logic that accounts for extreme aspect ratios (e.g. iPad vs ultra-long Android)
            float sizeByHeight = totalHeightNeeded / 2.0f;
            float sizeByWidth = (minWidthNeeded / currentAspect) / 2.0f;
            
            cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
            
            // Camera position: perfectly centered on the board
            transform.position = new Vector3(0, 0, -10f);
        }
    }
}
