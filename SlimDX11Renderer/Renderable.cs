using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace SlimDX11Renderer
{
    public class Renderable : Core.IRenderableItem, IRenderable
    {
        InputLayout layout_;
        Mesh mesh_;
        Effect effect_;
        EffectTechnique technique_;
        Matrix world_;
        float subDLevel_;
        Dictionary<EffectResourceVariable, ShaderResourceView> boundTextures_;
        Dictionary<EffectSamplerVariable, SamplerState> boundSamplers_;

        Vector2 m_scale;
        Vector2 m_screenSize;
        Vector2 m_size;
        Vector2 m_screenPos;

        bool m_isCameraRelative;

        public bool IsVisible { get; set; }

        public bool IsCameraRelative { get { return m_isCameraRelative; } }

        public Vector2 ScreenPos { get { return m_screenPos; } }

        public Vector4 Colour { get { return new Vector4(255.0f, 255.0f, 255.0f, 255.0f); } }

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



        public Renderable(string effectFilename, string techniqueName, SlimDX.Direct3D11.Device device, Matrix world, string textureFilename, Scene scene, SamplerState sampler, Vector2 screenSize, bool isCameraRelative)
        {
            mesh_ = Mesh.MakeQuadMesh(device);
            var effect = scene.LoadEffect(effectFilename);
            technique_ = effect.GetTechniqueByName(techniqueName);
            layout_ = new InputLayout(device, technique_.GetPassByIndex(0).Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0) 
            });
            world_ = world;
            effect_ = effect;

            boundSamplers_ = new Dictionary<EffectSamplerVariable, SamplerState>();
            boundTextures_ = new Dictionary<EffectResourceVariable, ShaderResourceView>();

            var texture = scene.LoadTexture(textureFilename);

            BindSampler("mainSampler", sampler);
            BindTexture("quadImage", texture);

            m_screenSize = screenSize;
            m_isCameraRelative = isCameraRelative;

            IsVisible = true; // Visible by default
        }

        public void SetColour(float red, float green, float blue, float alpha)
        {
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
            foreach (var t in boundTextures_)
            {
                // Bound textures is: Dictionary<EffectSamplerVariable, SamplerState>
                t.Key.SetResource(t.Value);
            }
        }

        public void SetSamplers()
        {
            foreach (var s in boundSamplers_)
            {
                // Bound samplers is: Dictionary<EffectResourceVariable, ShaderResourceView>
                s.Key.SetSamplerState(0, s.Value);
            }
        }

        public void BindSampler(string nameInShader, SamplerState samplerState)
        {
            var sampler = effect_.GetVariableByName(nameInShader).AsSampler();
            if (boundSamplers_.ContainsKey(sampler))
            {
                boundSamplers_[sampler] = samplerState;
            }
            else
            {
                boundSamplers_.Add(sampler, samplerState);
            }
        }

        public void BindTexture(string nameInShader, ShaderResourceView texture)
        {
            var resource = effect_.GetVariableByName(nameInShader).AsResource();
            if (boundTextures_.ContainsKey(resource))
            {
                boundTextures_[resource] = texture;
            }
            else
            {
                boundTextures_.Add(resource, texture);
            }
        }
    }
}
