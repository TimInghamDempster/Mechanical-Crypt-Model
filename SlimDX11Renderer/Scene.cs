using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using SlimDX.D3DCompiler;
using Device = SlimDX.Direct3D11.Device;

namespace SlimDX11Renderer
{
    public class Scene : Core.IRenderableScene
    {
        List<IRenderable> objectsToDraw_;
        Dictionary<string, Mesh> meshes_;
        List<Camera> cameras_;
        List<DirectionalLight> lights_;
        Dictionary<string, Effect> effects_;
        Dictionary<string, ShaderResourceView> textures_;
        SamplerState m_sampler;

        int cameraIndex_;
        int lightIndex_;

        Device m_device;
        Vector2 m_screenSize;
        Vector2 m_cameraPos;

        Core.IRenderableItem m_blobRenderable;

        public Core.IRenderableItem BlobRenderable { get { return m_blobRenderable; } }
        public Vector2 CameraPos { get { return m_cameraPos; } }

        public float Width { get { return m_screenSize.X; } }
        public float Height { get { return m_screenSize.Y; } }

        public List<Core.RenderArrays> RenderArrays { get; set; }

        public Scene(Device device, SamplerState sampler, Vector2 screenSize)
        {
            meshes_ = new Dictionary<string, Mesh>();
            cameras_ = new List<Camera>();
            lights_ = new List<DirectionalLight>();
            objectsToDraw_ = new List<IRenderable>();
            effects_ = new Dictionary<string, Effect>();
            textures_ = new Dictionary<string, ShaderResourceView>();

            RenderArrays = new List<Core.RenderArrays>();

            cameraIndex_ = -1;

            m_device = device;
            m_sampler = sampler;
            m_screenSize = screenSize;

            m_blobRenderable = GetNewBlobRenderable("Content/Shaders/BlobRenderer.fx", "Blob");
            m_blobRenderable.SetSize(10.0f, 10.0f);
        }

        public void UpdateCameraPos(float x, float y)
        {
            m_cameraPos = new Vector2((x / m_screenSize.X) * 2, (y / m_screenSize.Y)*2);
        }

        public Core.IRenderableItem GetNewRenderable(string effectFilename, string techniqueName, string textureFilename, bool isCameraRelative)
        {
            var renderable = new Renderable(effectFilename, techniqueName, m_device, Matrix.Identity, textureFilename, this, m_sampler, m_screenSize, isCameraRelative);
            objectsToDraw_.Add(renderable);
            return renderable;
        }
        
        public Core.IRenderableItem GetNewBlobRenderable(string effectFilename, string techniqueName)
        {
            var renderable = new BlobRenderable(effectFilename, techniqueName, m_device, Matrix.Identity, this, m_sampler, m_screenSize);
            objectsToDraw_.Add(renderable);
            return renderable;
        }

        /// <summary>
        /// Add a camera to the scene
        /// </summary>
        /// <param name="cam">The camera to add</param>
        /// <returns>The index of the camera</returns>
        public int AddCamera(Camera cam)
        {
            cameras_.Add(cam);
            return cameras_.Count - 1;
        }

        public void CreateCamera()
        {
            Camera cam = new Camera(0.0f, 0.0f, 0.1f, 10.0f);
            AddCamera(cam);
        }

        /// <summary>
        /// Add a light to the scene
        /// </summary>
        /// <param name="cam">The light to add</param>
        /// <returns>The index of the light</returns>
        public int AddLight(DirectionalLight light)
        {
            lights_.Add(light);
            return lights_.Count - 1;
        }

        public void AddMesh(string name, Mesh mesh)
        {
            if (!meshes_.ContainsKey(name))
            {
                meshes_.Add(name, mesh);
            }
        }

        public ShaderResourceView LoadTexture(string fileName)
        {
            if (!textures_.ContainsKey(fileName))
            {
                Texture2D temp = Texture2D.FromFile(m_device, fileName);
                ShaderResourceView res = new ShaderResourceView(m_device, temp);
                textures_.Add(fileName, res);
                temp.Dispose();
            }
            return textures_[fileName];
        }

        public Effect LoadEffect(string fileName)
        {
            if (!effects_.ContainsKey(fileName))
            {
                ShaderBytecode sb = ShaderBytecode.CompileFromFile(fileName, "fx_5_0", ShaderFlags.None, EffectFlags.None);
                Effect effect = new Effect(m_device, sb);
                effects_.Add(fileName, effect);
                sb.Dispose();
            }
            return effects_[fileName];
        }

        /// <summary>
        /// Set the current camera.
        /// </summary>
        /// <param name="index">The index of the camera to set.</param>
        public void SetCurrentCamera(int index)
        {
            cameraIndex_ = index;
        }

        public Camera GetCurrentCamera()
        {
            return cameras_[cameraIndex_];
        }

        /// <summary>
        /// Set the current light.
        /// </summary>
        /// <param name="index">The index of the light to set.</param>
        public void SetCurrentLight(int index)
        {
            lightIndex_ = index;
        }

        public IList<IRenderable> GetObjectsToDraw()
        {
            return objectsToDraw_.AsReadOnly();
        }

        public void Dispose()
        {
            foreach (var m in meshes_)
            {
                m.Value.Dispose();
            }
            foreach (var e in effects_)
            {
                e.Value.Dispose();
            }
            foreach (var t in textures_)
            {
                t.Value.Dispose();
            }
            foreach (var r in objectsToDraw_)
            {
                r.Dispose();
            }
        }
    }
}
