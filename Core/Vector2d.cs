using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public struct Vector2d
    {
        public float X;
        public float Y;

        public Vector2d(float x, float y) { X = x; Y = y; }

        public float Length()
        {
            return (float)Math.Sqrt(Vector2d.DotProduct(this, this));
        }

        public static Vector2d operator +(Vector2d vec1, Vector2d vec2)
        {
            return new Vector2d()
            {
                X = vec1.X + vec2.X,
                Y = vec1.Y + vec2.Y
            };
        }

        public static Vector2d operator +(Vector2d vec1, float val)
        {
            return new Vector2d()
            {
                X = vec1.X + val,
                Y = vec1.Y + val
            };
        }

        public static Vector2d operator -(Vector2d vec1, float val)
        {
            return new Vector2d()
            {
                X = vec1.X - val,
                Y = vec1.Y - val
            };
        }

        public static Vector2d operator -(Vector2d vec1, Vector2d vec2)
        {
            return new Vector2d()
            {
                X = vec1.X - vec2.X,
                Y = vec1.Y - vec2.Y
            };
        }

        public static Vector2d operator *(Vector2d vec1, float multiplier)
        {
            return new Vector2d()
            {
                X = vec1.X * multiplier,
                Y = vec1.Y * multiplier
            };
        }

        public static Vector2d operator /(Vector2d vec1, float denominator)
        {
            System.Diagnostics.Debug.Assert(denominator != 0.0f);

            return new Vector2d()
            {
                X = vec1.X / denominator,
                Y = vec1.Y / denominator
            };
        }

        public static bool operator ==(Vector2d first, Vector2d second)
        {
            return (first.X == second.X && first.Y == second.Y);
        }

        public static bool operator !=(Vector2d first, Vector2d second)
        {
            return (first.X != second.X || first.Y != second.Y);
        }

        public static Vector2d MaxVector()
        {
            return new Vector2d() { X = float.MaxValue, Y = float.MaxValue };
        }

        public static Vector2d MinVector()
        {
            return new Vector2d() { X = float.MinValue, Y = float.MinValue };
        }

        public static Vector2d ComponentWiseMin(Vector2d vec1, Vector2d vec2)
        {
            return new Vector2d() { X = Math.Min(vec1.X, vec2.X), Y = Math.Min(vec1.Y, vec2.Y) };
        }

        public static Vector2d ComponentWiseMax(Vector2d vec1, Vector2d vec2)
        {
            return new Vector2d() { X = Math.Max(vec1.X, vec2.X), Y = Math.Max(vec1.Y, vec2.Y) };
        }

        public static float DotProduct(Vector2d vec1, Vector2d vec2)
        {
            return (vec1.X * vec2.X) + (vec1.Y * vec2.Y);
        }

        public bool EitherComponentLessThan(Vector2d largerVector)
        {
            return (this.X < largerVector.X || this.Y < largerVector.Y);
        }

        public bool EitherComponentGreaterThan(Vector2d smallerVector)
        {
            return (this.X > smallerVector.X || this.Y > smallerVector.Y);
        }
    }
}
