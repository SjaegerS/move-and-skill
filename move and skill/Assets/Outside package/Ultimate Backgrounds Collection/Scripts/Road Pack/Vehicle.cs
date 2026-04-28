using UnityEngine;

namespace UltimateBackgroundsCollection
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Vehicle : MonoBehaviour
    {
        private Rigidbody2D rb;

        [SerializeField]
        [Range(4.0f, 10.0f)]
        private float horizontalSpeed = 4.0f;

        [SerializeField]
        [Range(2.0f, 4.0f)]
        private float verticalSpeed = 2.0f;

        [SerializeField]
        private bool endlessMove = false;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            rb.linearVelocity = new Vector2(endlessMove ? horizontalSpeed : horizontal * horizontalSpeed, vertical * verticalSpeed);
        }
    }
}

