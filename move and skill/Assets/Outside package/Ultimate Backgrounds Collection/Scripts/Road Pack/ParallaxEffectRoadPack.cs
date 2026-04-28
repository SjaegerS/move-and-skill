using UnityEngine;

namespace UltimateBackgroundsCollection
{
    public class ParallaxEffectRoadPack : MonoBehaviour
    {
        private Transform mainCamera;

        public float parallaxIntensityX;
        public float independantSpeed;

        private float cameraSize;
        private float spriteWidth;
        private Vector2 initialPos;
        private float translationOffset = 0;

        private void Start()
        {
            mainCamera = Camera.main.transform;
            cameraSize = Camera.main.orthographicSize;
            spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x / 3;

            transform.position = new Vector2(mainCamera.position.x, mainCamera.position.y - cameraSize);
            initialPos = transform.position;
        }

        private void LateUpdate()
        {
            translationOffset += independantSpeed * Time.deltaTime * parallaxIntensityX;

            float parallaxOffsetX = (mainCamera.position.x * (1 - (parallaxIntensityX / 2))) + translationOffset;

            transform.position = new Vector2(initialPos.x + parallaxOffsetX, initialPos.y);

            float cameraOffsetX = mainCamera.position.x - transform.position.x;

            if (cameraOffsetX > spriteWidth / 2)
                initialPos.x += spriteWidth;
            else if (cameraOffsetX < -spriteWidth / 2)
                initialPos.x -= spriteWidth;
        }
    }
}

