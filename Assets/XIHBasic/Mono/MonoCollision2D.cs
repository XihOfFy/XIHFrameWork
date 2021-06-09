using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XIHBasic {
    public class MonoCollision2D : MonoDotBase
    {
        public Action<Collision2D> onCollisionEnter2D;
        public Action<Collision2D> onCollisionExit2D;
        public Action<Collider2D> onTriggerEnter2D;
        public Action<Collider2D> onTriggerExit2D;
        private void OnCollisionEnter2D(Collision2D collision)
        {
            onCollisionEnter2D?.Invoke(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            onCollisionExit2D?.Invoke(collision);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            onTriggerEnter2D?.Invoke(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            onTriggerExit2D?.Invoke(other);
        }
    }
}
