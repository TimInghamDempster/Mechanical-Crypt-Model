using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimDX11Renderer
{
    public interface IRenderable
    {
        bool IsVisible { get; }
        SlimDX.Direct3D11.Effect Effect { get; }
        SlimDX.Direct3D11.EffectTechnique Technique { get; }
        bool IsCameraRelative { get; }
        Mesh Mesh { get; }
        SlimDX.Vector2 Scale { get; }
        SlimDX.Vector2 ScreenPos { get; }
        SlimDX.Direct3D11.InputLayout Layout { get; }
        SlimDX.Vector4 Colour { get; }

        void Dispose();
        void SetSamplers();
        void SetTextures();
    }
}
