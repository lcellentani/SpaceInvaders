using UnityEngine;
using System.Collections;

namespace dbga
{
    public class WorldBoundsProxy : MonoBehaviour
    {
        public static WorldBoundsProxy SharedInstance;

        private float screenWidth;
        public float ScreeWidth
        {
            get { return screenWidth; }
        }
        private float screenHeight;
        public float ScreenWidth
        {
            get { return screenHeight; }
        }

        private float screenHalfWidth;
        public float ScreenHalfWidth
        {
            get { return screenHalfWidth; }
        }
        private float screenHalfHeight;
        public float ScreenHalfHeight
        {
            get { return screenHalfHeight; }
        }

        void Awake()
        {
            SharedInstance = this;

            CalculateWorldBounds();

        }

        private void CalculateWorldBounds()
        {
            float ratio = (float)Screen.width / (float)Screen.height;

            screenHalfHeight = Camera.main.orthographicSize;
            screenHalfWidth = Mathf.RoundToInt(Camera.main.orthographicSize * ratio);

            screenHeight = screenHalfHeight * 2;
            screenWidth = screenHalfWidth * 2;
        }
    }
}
