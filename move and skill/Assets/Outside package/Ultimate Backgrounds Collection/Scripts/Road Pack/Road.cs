using UnityEngine;

namespace UltimateBackgroundsCollection
{
    public class Road : MonoBehaviour
    {
        private Transform vehicle;

        private void Start()
        {
            vehicle = GameObject.FindWithTag("Player").GetComponent<Transform>();
        }

        private void Update()
        {
            transform.position = new Vector2(vehicle.position.x, transform.position.y);
        }
    }
}

