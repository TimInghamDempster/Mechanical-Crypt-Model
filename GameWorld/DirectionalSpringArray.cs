using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace GameWorld
{
    class DirectionalSpringArray
    {
        public List<Vector2d> RestVectors;
        public List<Vector2d> OriginalRestVectors;
        public List<float> Stiffnesses;
        public List<uint> StartPointIndices;
        public List<uint> EndPointIndices;

        public DirectionalSpringArray()
        {
            RestVectors = new List<Vector2d>();
            OriginalRestVectors = new List<Vector2d>();
            Stiffnesses = new List<float>();
            StartPointIndices = new List<uint>();
            EndPointIndices = new List<uint>();
        }
    }
}
