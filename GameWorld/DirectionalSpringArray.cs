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

        public void UpdateDirectionalSprings(List<Vector2d> positions, List<Vector2d> velocities, RenderArrays springRenderArrays, Colour springColour, List<int> cellIds, List<Vector2d> basis1Array, List<Vector2d> basis2Array)
        {
            for (uint i = 0; i < EndPointIndices.Count; i++)
            {
                int startIndex = (int)StartPointIndices[(int)i];
                int endIndex = (int)EndPointIndices[(int)i];

                Vector2d startPoint = positions[startIndex];
                Vector2d endPoint = positions[endIndex];
                Vector2d delta = endPoint - startPoint;


                // Assumes both spring points are inside the same cell
                Vector2d basis1 = basis1Array[cellIds[startIndex]];
                Vector2d basis2 = basis2Array[cellIds[startIndex]];
                Vector2d transformedDelta = delta;
                transformedDelta.X = Vector2d.DotProduct(delta, basis1);
                transformedDelta.Y = Vector2d.DotProduct(delta, basis2);

                Vector2d equilibriumVector = RestVectors[(int)i];
                float stiffness = Stiffnesses[(int)i];

                /*
                if (equilibriumVector.X > equilibriumVector.Y)
                {
                    if (transformedDelta.X < 0.0f)
                    {
                        //endPoint.X = startPoint.X;
                        positions[endIndex] = endPoint;
                    }
                }
                else
                {
                    if (transformedDelta.Y < 0.0f)
                    {
                        //endPoint.Y = startPoint.Y;
                        positions[endIndex] = endPoint;
                    }
                }
                
                delta = endPoint - startPoint;
                transformedDelta.X = Vector2d.DotProduct(delta, basis1);
                transformedDelta.Y = Vector2d.DotProduct(delta, basis2);*/


                Vector2d lengthBeyondEquilibrium = equilibriumVector - transformedDelta;
                Vector2d restoringForce = lengthBeyondEquilibrium * stiffness;

                // Assuming contant mass
                velocities[startIndex] -= restoringForce;
                velocities[endIndex] += restoringForce;

#if DEBUG
                springRenderArrays.Positions.Add((startPoint + endPoint) / 2.0f);
                springRenderArrays.Colours.Add(springColour);
#endif
            }
        }
    }
}
