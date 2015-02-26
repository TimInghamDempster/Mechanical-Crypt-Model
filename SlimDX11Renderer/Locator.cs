using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using SlimDX.D3DCompiler;
using Device = SlimDX.Direct3D11.Device;

namespace SlimDX11Renderer
{
    public class Locator : Core.ILocator
    {
        Vector3 position_;
        Vector3 hpb_;
        Matrix transform_;
        Matrix rotation_;

        public Vector3 Position
        {
            get
            {
                return position_;
            }
        }

        public Vector3 Direction
        {
            get
            {
                UpdateMatrices();
                var temp = Vector3.Transform(Vector3.UnitZ, rotation_);
                return new Vector3(temp.X, temp.Y, temp.Z);
            }
        }

        public Matrix Transform
        {
            get
            {
                UpdateMatrices();
                return transform_;
            }
        }

        public float X
        {
            get
            {
                return position_.X;
            }
        }

        public float Y
        {
            get
            {
                return position_.Y;
            }
        }

        public float Z
        {
            get
            {
                return position_.Z;
            }
        }

        public Locator(Vector3 InitialPosition, Vector3 InitialForward, Vector3 InitialUp)
        {
            position_ = InitialPosition;
            transform_ = Matrix.LookAtLH(InitialPosition, InitialForward, InitialUp);
        }

        public void Move(float X, float Y, float Z)
        {
            position_.X += X;
            position_.Y += Y;
            position_.Z += Z;
        }

        public void MoveLocal(float X, float Y, float Z)
        {
            UpdateMatrices();
            Vector3 temp = new Vector3(X, Y, Z);
            var tempLocal = Vector3.Transform(temp, rotation_);
            position_.X += tempLocal.X;
            position_.Y += tempLocal.Y;
            position_.Z += tempLocal.Z;
        }

        void UpdateMatrices()
        {
            rotation_ = Matrix.RotationY(hpb_.X);

            var pAxis4 = Vector3.Transform(Vector3.UnitX, rotation_);
            var pAxis3 = new Vector3(pAxis4.X, pAxis4.Y, pAxis4.Z);
            rotation_ *= Matrix.RotationAxis(pAxis3, hpb_.Y);

            var bAxis4 = Vector3.Transform(Vector3.UnitZ, rotation_);
            var bAxis3 = new Vector3(bAxis4.X, bAxis4.Y, bAxis4.Z);
            rotation_ *= Matrix.RotationAxis(bAxis3, hpb_.Z);

            var look4 = Vector3.Transform(Vector3.UnitZ, rotation_);
            var up4 = Vector3.Transform(Vector3.UnitY, rotation_);
            var look3 = position_ + new Vector3(look4.X, look4.Y, look4.Z);
            var up3 = new Vector3(up4.X, up4.Y, up4.Z);
            transform_ = Matrix.LookAtLH(position_, look3, up3);
        }

        public void Pitch(float amount)
        {
            hpb_.Y += amount;
        }

        public void Heading(float amount)
        {
            hpb_.X += amount;
        }
        public void Bank(float amount)
        {
            hpb_.Z += amount;
        }
    }
}
