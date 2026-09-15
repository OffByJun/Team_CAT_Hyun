using UnityEngine;

namespace SSW
{
    public class CameraFollow : GameBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 1, -10);
        public float followSpeed = 5f;

        void LateUpdate()
        {
            if (target == null) return;
            FollowTarget();
        }

        private void FollowTarget()
        {
            Vector3 desiredPosition = target.position + offset;
            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
        }
    }
}