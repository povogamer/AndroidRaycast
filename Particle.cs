using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidProj
{
    public class Particle
    {
        public Vector2 pos { get; set; }
        // originally set to 36 for 36 rays to circle 360 degress.
        public RayCast[] rays = new RayCast[40];
        public void RAYS()
        {
            int a = 0;
            double angle;
            for(a = 0; a < rays.Length; a++)
            {
                rays[a] = new RayCast();
                angle = (Math.PI / (double)180) * ((double)a);
                rays[a].point = pos;
                rays[a].LookAt(angle);
            }
        }
    }
}
