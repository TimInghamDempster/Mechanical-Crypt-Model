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
    public class Camera
    {
        Locator locator_;
        Matrix proj_;

        public Matrix ViewProj
        {
            get
            {
                return locator_.Transform * proj_;
            }
        }

        public Core.ILocator Locator
        {
            get
            {
                return locator_;
            }
        }

        public Camera(float Fov, float Aspect, float ZNear, float ZFar)
        {
            locator_ = new Locator(new Vector3(0),Vector3.UnitZ, Vector3.UnitY);
            locator_.Heading((float)Math.PI);
            proj_ = Matrix.PerspectiveFovLH(Fov, Aspect, ZNear, ZFar);
        }
    }
}
