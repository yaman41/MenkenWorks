using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JointTest
{
    class JointPoint
    {
        private int pointx;
        private int pointy;
        private int pointz;

        public int getPointX()
        {
            return pointx;
        }
        public int getPointY()
        {
            return pointy;
        }
        public int getPointZ()
        {
            return pointz;
        }

        public void setPoint(int x, int y, int z)
        {
            this.pointx = x;
            this.pointy = y;
            this.pointz = z;
        }
    }
}
