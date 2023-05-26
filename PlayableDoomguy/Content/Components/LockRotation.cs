using System;

namespace PlayableDoomguy.Components {
    public class LockRotation : MonoBehaviour {
        public void FixedUpdate() {
            Vector3 rot = (Camera.main.transform.position - base.transform.position).normalized;
            rot.y = 0;
            base.transform.forward = rot;
        }
    }
}