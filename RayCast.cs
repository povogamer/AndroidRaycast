using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Java.Util;
using Microsoft.Xna.Framework;

namespace AndroidProj
{
    public class RayCast
    {
        public Vector2 point { get; set; }
        public Vector2 dir { get; set; }
        public double angle;

        public void LookAt(Vector2 a)
        {
            dir = new Vector2(a.X - point.X, a.Y - point.Y);
            dir.Normalize();
        }
        public void LookAt(double a)
        {
            dir = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
        }
    }
}
