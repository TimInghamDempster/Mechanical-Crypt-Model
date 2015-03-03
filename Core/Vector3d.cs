using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public struct Vector3d
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3d(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length()
        {
            return (float)Math.Sqrt(Vector3d.DotProduct(this, this));
        }

        public static Vector3d operator +(Vector3d vec1, Vector3d vec2)
        {
            return new Vector3d()
            {
                X = vec1.X + vec2.X,
                Y = vec1.Y + vec2.Y,
                Z = vec1.Z + vec2.Z
            };
        }

        public static Vector3d operator +(Vector3d vec1, float val)
        {
            return new Vector3d()
            {
                X = vec1.X + val,
                Y = vec1.Y + val,
                Z = vec1.Z + val
            };
        }

        public static Vector3d operator -(Vector3d vec1, float val)
        {
            return new Vector3d()
            {
                X = vec1.X - val,
                Y = vec1.Y - val,
                Z = vec1.Z - val
            };
        }

        public static Vector3d operator -(Vector3d vec1, Vector3d vec2)
        {
            return new Vector3d()
            {
                X = vec1.X - vec2.X,
                Y = vec1.Y - vec2.Y,
                Z = vec1.Z - vec2.Z
            };
        }

        public static Vector3d operator *(Vector3d vec1, float multiplier)
        {
            return new Vector3d()
            {
                X = vec1.X * multiplier,
                Y = vec1.Y * multiplier,
                Z = vec1.Z * multiplier
            };
        }

        public static Vector3d operator /(Vector3d vec1, float denominator)
        {
            System.Diagnostics.Debug.Assert(denominator != 0.0f);

            return new Vector3d()
            {
                X = vec1.X / denominator,
                Y = vec1.Y / denominator,
                Z = vec1.Z / denominator
            };
        }

        public static bool operator ==(Vector3d first, Vector3d second)
        {
            return (first.X == second.X && first.Y == second.Y && first.Z == second.Z);
        }

        public static bool operator !=(Vector3d first, Vector3d second)
        {
            return (first.X != second.X || first.Y != second.Y || first.Z != second.Z);
        }

        public static Vector3d MaxVector()
        {
            return new Vector3d() { X = float.MaxValue, Y = float.MaxValue, Z = float.MaxValue };
        }

        public static Vector3d MinVector()
        {
            return new Vector3d() { X = float.MinValue, Y = float.MinValue, Z = float.MaxValue };
        }

        public static Vector3d ComponentWiseMin(Vector3d vec1, Vector3d vec2)
        {
            return new Vector3d() { X = Math.Min(vec1.X, vec2.X), Y = Math.Min(vec1.Y, vec2.Y), Z = Math.Min(vec1.Z, vec2.Z) };
        }

        public static Vector3d ComponentWiseMax(Vector3d vec1, Vector3d vec2)
        {
            return new Vector3d() { X = Math.Max(vec1.X, vec2.X), Y = Math.Max(vec1.Y, vec2.Y), Z = Math.Max(vec1.Z, vec2.Z) };
        }

        public static float DotProduct(Vector3d vec1, Vector3d vec2)
        {
            return (vec1.X * vec2.X) + (vec1.Y * vec2.Y) + (vec1.Z * vec2.Z);
        }

        public bool EitherComponentLessThan(Vector3d largerVector)
        {
            return (this.X < largerVector.X || this.Y < largerVector.Y || this.Z < largerVector.Z);
        }

        public bool EitherComponentGreaterThan(Vector3d smallerVector)
        {
            return (this.X > smallerVector.X || this.Y > smallerVector.Y || this.Z > smallerVector.Z);
        }
    }
}
