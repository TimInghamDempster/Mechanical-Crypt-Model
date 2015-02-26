using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

namespace GameWorld
{
    public class BlobArrays
    {
        public List<Vector2d> Positions;
        public List<Colour> Colours;
        public List<Vector2d> Velocities;
        public List<float> InteractionRadii;
        public List<int> CellIds;
        public List<int> CollisionCount;
        public List<bool> CollideableBlobs;
        public List<int> BBoxIndices;
        public List<HashSet<int>> PersistentCollisions;
        public List<HashSet<int>> CurrentCollisions;

        public BlobArrays()
        {
            Positions = new List<Vector2d>();
            Colours = new List<Colour>();
            Velocities = new List<Vector2d>();
            InteractionRadii = new List<float>();
            CellIds = new List<int>();
            CollisionCount = new List<int>();
            CollideableBlobs = new List<bool>();
            BBoxIndices = new List<int>();
            PersistentCollisions = new List<HashSet<int>>();
            CurrentCollisions = new List<HashSet<int>>();
        }

        public void AddBlob(Vector2d position, Colour colour, Vector2d velocity, float interactionRange, int cellIndex, bool collideable, int bboxId)
        {
            Positions.Add(position);
            Colours.Add(colour);
            Velocities.Add(velocity);
            InteractionRadii.Add(interactionRange);
            CellIds.Add(cellIndex);
            CollisionCount.Add(0);
            CollideableBlobs.Add(collideable);
            BBoxIndices.Add(bboxId);
            PersistentCollisions.Add(new HashSet<int>());
            CurrentCollisions.Add(new HashSet<int>());
        }
    }
}
