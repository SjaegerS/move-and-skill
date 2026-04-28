using UnityEngine;

namespace UltimateBackgroundsCollection
{
    [ExecuteInEditMode]
    public class MainCamera : MonoBehaviour
    {
        private Transform player;

        public bool smoothCamera = true;
        public bool lockVerticalAxis = false;
        public bool lockCameraSize = false;
        public float cameraSize = 5f;
        public float playerOffsetY = 0.7f;
        public float playerOffsetX = 0.0f;

        private void Start()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        }

        private void Update()
        {
            Camera.main.orthographicSize = lockCameraSize ? 5f : cameraSize;

            float playerDistanceY = Camera.main.orthographicSize * playerOffsetY;
            float playerDistanceX = Camera.main.orthographicSize * -playerOffsetX;
            float smoothSpeed = 5.0f;

            Vector3 desiredPosition = new Vector3(player.position.x + playerDistanceX, lockVerticalAxis ? playerDistanceY : player.position.y + playerDistanceY, -10f);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            transform.position = smoothCamera ? smoothedPosition : desiredPosition;
        }
    }
}
