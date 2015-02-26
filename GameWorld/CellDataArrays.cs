using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

namespace GameWorld
{
    class CellDataArrays
    {
        public List<int> BlobIndices;
        public List<int> SpringIndices;
        public List<bool> IsGrowing;
        public List<float> OriginalSeparation;
        public List<float> CurrentSeparationMultiplier;
        public List<Vector2d> Basis1;
        public List<Vector2d> Basis2;

        public CellDataArrays()
        {
            BlobIndices = new List<int>();
            SpringIndices = new List<int>();
            IsGrowing = new List<bool>();
            OriginalSeparation = new List<float>();
            CurrentSeparationMultiplier = new List<float>();
            Basis1 = new List<Vector2d>();
            Basis2 = new List<Vector2d>();
        }
    }
}
