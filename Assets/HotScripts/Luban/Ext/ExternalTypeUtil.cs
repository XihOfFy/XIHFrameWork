using UnityEngine;

namespace Tmpl
{
    public class ExternalTypeUtil
    {
        public static Vector2 NewVector2(vector2 v2) {
            return new Vector2(v2.X,v2.Y);
        }
        public static Vector3 NewVector3(vector3 v3) {
            return new Vector3(v3.X,v3.Y,v3.Z);
        }
    }
}
