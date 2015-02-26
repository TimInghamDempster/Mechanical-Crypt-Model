using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace GameWorld
{
    class SpringArray
    {
        public List<float> RestLengths;
        public List<float> Stiffnesses;
        public List<uint> StartPointIndices;
        public List<uint> EndPointIndices;

        public SpringArray()
        {
            RestLengths = new List<float>();
            Stiffnesses = new List<float>();
            StartPointIndices = new List<uint>();
            EndPointIndices = new List<uint>();
        }
    }
}
