using UnityEngine;

namespace Project.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothing = 0.15f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

        private Vector3 _vel;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothing);
        }
    }
}