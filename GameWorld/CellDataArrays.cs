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
        public List<float> OriginalSurfaceArea;
        public List<float> CurrentSeparationMultiplier;

        public const float IntraCellBlobSeparation = 50.0f;
        public const float CellIdealRadii = 200.0f;
        public const int BoundaryElementsInCell = 16;
        public const float BoundarySpringStrength = 0.0001f;
        public const float AreaSpringStrength = 0.001f;

        public CellDataArrays()
        {
            BlobIndices = new List<int>();
            SpringIndices = new List<int>();
            IsGrowing = new List<bool>();
            OriginalSurfaceArea = new List<float>();
            CurrentSeparationMultiplier = new List<float>();
        }
        
        // Uses a circle based on the closest blob to the cell center as an approximation
        // for cell area.
        public void UpdateAreaSprings(BlobArrays blobs)
        {
            foreach(int startIndex in BlobIndices)
            {
                Vector2d averagePos = new Vector2d();
                for(int i = 0; i < BoundaryElementsInCell; i++)
                {
                    averagePos += blobs.Positions[startIndex + i];
                }
                averagePos /= CellDataArrays.BoundaryElementsInCell;

                float minRadius = float.MaxValue;
                for (int i = 0; i < BoundaryElementsInCell; i++)
                {
                    Vector2d localPos = averagePos - blobs.Positions[startIndex + i];
                    float blobRadius = localPos.Length();
                    minRadius = Math.Min(minRadius, blobRadius);
                }

                float compression = CellIdealRadii - minRadius;
                compression = Math.Max(compression, 0.0f);
                compression *= compression;

                for (int i = 0; i < BoundaryElementsInCell; i++)
                {
                    Vector2d localPos = averagePos - blobs.Positions[startIndex + i];
                    Vector2d force = localPos / localPos.Length();
                    force *= AreaSpringStrength * compression;

                    blobs.Velocities[startIndex + i] -= force;
                }
            }
        }
    }
}
