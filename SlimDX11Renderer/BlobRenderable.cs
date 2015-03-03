using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace SlimDX11Renderer
{
    public class BlobRenderable : Core.IRenderableItem, IRenderable
    {
        InputLayout layout_;
        Mesh mesh_;
        Effect effect_;
        EffectTechnique technique_;
        Matrix world_;
        float subDLevel_;

        Vector2 m_scale;
        Vector2 m_screenSize;
        Vector2 m_size;
        Vector2 m_screenPos;

        Vector4 m_colour;

        const bool m_isCameraRelative = false;

        public bool IsVisible { get; set; }

        public bool IsCameraRelative { get { return m_isCameraRelative; } }

        public Vector2 ScreenPos { get { return m_screenPos; } }

        public Vector4 Colour { get { return m_colour; } }

        public Matrix World
        {
            get
            {
                return world_;
            }
        }
        public EffectTechnique Technique
        {
            get
            {
                return technique_;
            }
        }
        public Effect Effect
        {
            get
            {
                return effect_;
            }
        }
        public Mesh Mesh
        {
            get
            {
                return mesh_;
            }
        }
        public InputLayout Layout
        {
            get
            {
                return layout_;
            }
        }
        public float Layer { get; set; }
        public float SubDLevel
        {
            get
            {
                return subDLevel_;
            }
            set
            {
                subDLevel_ = value;
                if (subDLevel_ < 1)
                {
                    subDLevel_ = 1;
                }
                else if (subDLevel_ > 64)
                {
                    subDLevel_ = 64;
                }
            }
        }

        public Vector2 Scale { get { return m_scale; } }
        public Vector2 Size
        {
            get
            {
                return m_size;
            }
            set
            {
                m_scale.X = value.X / m_screenSize.X;
                m_scale.Y = value.Y / m_screenSize.Y;
                m_size = value;
            }
        }



        public BlobRenderable(string effectFilename, string techniqueName, SlimDX.Direct3D11.Device device, Matrix world, Scene scene, SamplerState sampler, Vector2 screenSize, float scale)
        {
            mesh_ = Mesh.MakeQuadMesh(device, scale);
            var effect = scene.LoadEffect(effectFilename);
            technique_ = effect.GetTechniqueByName(techniqueName);
            layout_ = new InputLayout(device, technique_.GetPassByIndex(0).Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0) 
            });
            world_ = world;
            effect_ = effect;

            m_screenSize = screenSize;

            m_scale = new Vector2(1.0f, 1.0f);

            IsVisible = true; // Visible by default
        }


        public void SetColour(float red, float green, float blue, float alpha)
        {
            m_colour = new Vector4(red, green, blue, alpha);
        }

        public void Dispose()
        {
            layout_.Dispose();
            mesh_.Dispose();
        }

        public void SetPos(float x, float y)
        {
            m_screenPos = new Vector2((x / m_screenSize.X) * 2, (y / m_screenSize.Y) * 2);
        }

        public void SetSize(float width, float height)
        {
            Size = new Vector2(width, height);
        }

        public void SetTextures()
        {
        }

        public void SetSamplers()
        {
        }
    }
}
