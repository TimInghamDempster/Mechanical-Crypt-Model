using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class RenderArrays3d
    {
        public List<Vector3d> Positions;
        public List<Colour> Colours;
        public List<bool> Visible; // Assumes all are true if null

        public RenderArrays3d()
        {
            Positions = new List<Vector3d>();
            Colours = new List<Colour>();
        }
    }
}
