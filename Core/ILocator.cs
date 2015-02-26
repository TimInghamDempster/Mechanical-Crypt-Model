using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public interface ILocator
    {
        void Move(float X, float Y, float Z);
        void MoveLocal(float X, float Y, float Z);
        void Pitch(float amount);
        void Heading(float amount);
        void Bank(float amount);
        float X { get; }
        float Y { get; }
        float Z { get; }
    }
}
