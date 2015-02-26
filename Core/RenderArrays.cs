using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class RenderArrays
    {
        public List<Vector2d> Positions;
        public List<Colour> Colours;

        public RenderArrays()
        {
            Positions = new List<Vector2d>();
            Colours = new List<Colour>();
        }
    }
}
