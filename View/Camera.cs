using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.View
{
    public class Camera
    {
        public Vector3 Position;
        public float Zoom;

        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;
        private Vector3 _front = Vector3.UnitZ;
        public Camera(Vector3 position, float zoom)
        {
            Position = position;
            Zoom = zoom;
        }


    }
}
