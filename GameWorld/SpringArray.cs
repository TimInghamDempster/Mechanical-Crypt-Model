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

        public void UpdateSprings(RenderArrays springDebugArrays, Colour springColour, List<Vector2d> positions, List<Vector2d> velocities)
        {
#if DEBUG
            springDebugArrays.Positions.Clear();
            springDebugArrays.Colours.Clear();
#endif

            for (uint i = 0; i < EndPointIndices.Count; i++)
            {
                int startIndex = (int)StartPointIndices[(int)i];
                int endIndex = (int)EndPointIndices[(int)i];

                Vector2d startPoint = positions[startIndex];
                Vector2d endPoint = positions[endIndex];
                Vector2d delta = startPoint - endPoint;

                float equilibriumLength = RestLengths[(int)i];
                float stiffness = Stiffnesses[(int)i];
                float currentLenght = (float)Math.Sqrt(Vector2d.DotProduct(delta, delta));
                if (currentLenght == 0)
                {
                    currentLenght = float.MinValue;
                }
                float lengthBeyondEquilibrium = currentLenght - equilibriumLength;
                float restoringForce = lengthBeyondEquilibrium * stiffness * -1.0f;

                if (lengthBeyondEquilibrium > equilibriumLength * 2.0f)
                {
                    EndPointIndices.RemoveAt((int)i);
                    RestLengths.RemoveAt((int)i);
                    StartPointIndices.RemoveAt((int)i);
                    Stiffnesses.RemoveAt((int)i);
                    i--;
                    continue;
                }

                Vector2d direction = delta / currentLenght;

                // Assuming contant mass
                velocities[startIndex] += direction * restoringForce;
                velocities[endIndex] -= direction * restoringForce;

#if DEBUG
                springDebugArrays.Positions.Add((startPoint + endPoint) / 2.0f);
                springDebugArrays.Colours.Add(springColour);
#endif
            }
        }
    }
}
