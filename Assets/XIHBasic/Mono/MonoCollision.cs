using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XIHBasic {
    public class MonoCollision : MonoDotBase
    {
        public Action<Collision> onCollisionEnter;
        public Action<Collision> onCollisionExit;
        public Action<Collider> onTriggerEnter;
        public Action<Collider> onTriggerExit;
        private void OnCollisionEnter(Collision collision)
        {
            onCollisionEnter?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            onCollisionExit?.Invoke(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            onTriggerEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            onTriggerExit?.Invoke(other);
        }
    }
}
