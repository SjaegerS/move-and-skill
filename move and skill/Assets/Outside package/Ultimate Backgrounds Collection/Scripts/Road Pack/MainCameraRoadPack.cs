using UnityEngine;

namespace UltimateBackgroundsCollection
{
    [ExecuteInEditMode]
    public class MainCameraRoadPack : MonoBehaviour
    {
        private Transform vehicle;

        public bool smoothCamera = true;

        [SerializeField]
        [Range(0.0f, 6.0f)]
        public float vehicleOffsetX = 4.0f;

        private float cameraSize = 5.0f;

        private void Start()
        {
            vehicle = GameObject.FindWithTag("Player").GetComponent<Transform>();
        }

        private void Update()
        {
            Camera.main.orthographicSize = cameraSize;

            float smoothSpeed = 5.0f;

            Vector3 desiredPosition = new Vector3(vehicle.position.x + vehicleOffsetX, transform.position.y, -10f);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            transform.position = smoothCamera ? smoothedPosition : desiredPosition;
        }
    }
}
