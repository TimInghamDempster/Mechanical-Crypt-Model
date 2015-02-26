using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using SlimDX.D3DCompiler;
using Device = SlimDX.Direct3D11.Device;
using System.Collections.Generic;

namespace SlimDX11Renderer
{
    public class Renderer : Core.IRenderer
    {

        Device device_;
        RenderTargetView renderView_;
        DepthStencilView m_depthView;

        SwapChain swapChain_;

        DateTime lastTime_;
        RenderForm form_;
        Texture2D backBuffer_;
        Texture2D m_depthBuffer;
        RasterizerState m_rasterizerState;
        Dictionary<string, SamplerState> samplerStates_;

        List<Scene> m_scenes;
        UInt32 m_currentSceneIndex;

        public Renderer(String title)
        {
            m_scenes = new List<Scene>();

            samplerStates_ = new Dictionary<string, SamplerState>();

            lastTime_ = DateTime.Now;

            form_ = new RenderForm(title);
            form_.Width = 1024;
            form_.Height = 768;
           
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(form_.ClientSize.Width, form_.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form_.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, desc, out device_, out swapChain_);

            Factory factory = swapChain_.GetParent<Factory>();
            factory.SetWindowAssociation(form_.Handle, WindowAssociationFlags.IgnoreAll);

            backBuffer_ = Texture2D.FromSwapChain<Texture2D>(swapChain_, 0);
            renderView_ = new RenderTargetView(device_, backBuffer_);

            
            var depthDesc = new Texture2DDescription()
            {
                Width = form_.Width,
                Height = form_.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription()
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = 0,
                OptionFlags = 0
            };
            m_depthBuffer = new Texture2D(device_, depthDesc);

            device_.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, form_.ClientSize.Width, form_.ClientSize.Height, 0.1f, 1.0f));
            
            RasterizerStateDescription descRast = new RasterizerStateDescription()
            {
               FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                IsDepthClipEnabled = true
            };

            m_rasterizerState = RasterizerState.FromDescription(device_, descRast);

            DepthStencilStateDescription dsStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,

                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,

                FrontFace = new DepthStencilOperationDescription()
                {
                    Comparison = Comparison.Always,
                    DepthFailOperation = StencilOperation.Increment,
                    FailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    Comparison = Comparison.Always,
                    DepthFailOperation = StencilOperation.Increment,
                    FailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep
                },
            };

            DepthStencilState dsState = DepthStencilState.FromDescription(device_, dsStateDesc);

            device_.ImmediateContext.OutputMerger.DepthStencilState = dsState;
            device_.ImmediateContext.Rasterizer.State = m_rasterizerState;

            DepthStencilViewDescription DSVDesc = new DepthStencilViewDescription()
            {
                Format = Format.D24_UNorm_S8_UInt,
                Dimension = DepthStencilViewDimension.Texture2D,
                MipSlice = 0
            };

            m_depthView = new DepthStencilView(device_, m_depthBuffer, DSVDesc);

            dsState.Dispose();

            device_.ImmediateContext.OutputMerger.SetTargets(m_depthView, renderView_);

            CreateSamplers();
        }

        public void SetCurrentScene(Core.IRenderableScene newCurrentScene)
        {
            for(UInt32 i = 0; i < m_scenes.Count; i++)
            {
                if (m_scenes[(int)i] == newCurrentScene)
                {
                    m_currentSceneIndex = i;
                }
            }
        }

        void CreateSamplers()
        {
            SamplerDescription desc = new SamplerDescription();
            desc.AddressU = TextureAddressMode.Mirror;
            desc.AddressV = TextureAddressMode.Mirror;
            desc.AddressW = TextureAddressMode.Mirror;
            desc.Filter = Filter.MinMagMipLinear;
            desc.MaximumAnisotropy = 16;
            desc.MaximumLod = float.MaxValue;
            SamplerState s = SamplerState.FromDescription(device_, desc);
            samplerStates_.Add("minMagMipLinear", s);
        }

        /// <summary>
        /// Run the game by allowing SlimDx to run the message pump and call
        /// anything that registers for a per frame update.  Not ideal but
        /// apparently best practice.
        /// </summary>
        /// <param name="updateFunctions"></param>
        public void Run(Core.UpdateFunctions updateFunctions)
        {
            //updateFunctions.AddUpdateFunction(Render, new TimeSpan(0));
            updateFunctions.AddUpdateFunction(RenderBlobArray, new TimeSpan(0));

            MessagePump.Run(form_, () =>
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan timeSinceLastUpdate = currentTime - lastTime_;
                lastTime_ = currentTime;
                updateFunctions.CallUpdateFunction(timeSinceLastUpdate);
            });
        }

        public void Dispose()
        {
            foreach (var scene in m_scenes)
            {
                scene.Dispose();
            }
            foreach (var s in samplerStates_)
            {
                s.Value.Dispose();
            }
            m_rasterizerState.Dispose();
            renderView_.Dispose();
            backBuffer_.Dispose();
            device_.Dispose();
            swapChain_.Dispose();
            m_depthBuffer.Dispose();
            m_depthView.Dispose();
        }

        public void RenderBlobArray()
        {
            if (m_currentSceneIndex < m_scenes.Count)
            {
                Scene scene = m_scenes[(int)m_currentSceneIndex];
                Camera camera = scene.GetCurrentCamera();

                device_.ImmediateContext.ClearRenderTargetView(renderView_, Color.CornflowerBlue);
                device_.ImmediateContext.ClearDepthStencilView(m_depthView, DepthStencilClearFlags.Depth, 1.0f, 0x00);

                var renderable = (BlobRenderable)scene.BlobRenderable;
                device_.ImmediateContext.InputAssembler.InputLayout = renderable.Layout;
                device_.ImmediateContext.InputAssembler.PrimitiveTopology = renderable.Mesh.Topology;
                device_.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderable.Mesh.Vertices, renderable.Mesh.Stride, renderable.Mesh.Offset));

                var effect = renderable.Effect;
                effect.GetVariableByName("scale").AsVector().Set(renderable.Scale);

                foreach (var rArrays in scene.RenderArrays)
                {
                    for (int i = 0; i < rArrays.Colours.Count; i++)
                    {
                        var colour = rArrays.Colours[i];
                        var pos = rArrays.Positions[i];

                        effect.GetVariableByName("colour").AsVector().Set(new Vector4(colour.R, colour.G, colour.B, colour.A));
                        effect.GetVariableByName("entityPosition").AsVector().Set(new Vector2(pos.X / scene.Width, pos.Y / scene.Height));


                        for (int j = 0; j < renderable.Technique.Description.PassCount; ++j)
                        {
                            renderable.Technique.GetPassByIndex(j).Apply(device_.ImmediateContext);
                            device_.ImmediateContext.Draw(renderable.Mesh.VertexCount, renderable.Mesh.StartVertex);
                        }
                    }
                }

                swapChain_.Present(0, PresentFlags.None);
            }
        }

        void Render()
        {
            if (m_currentSceneIndex < m_scenes.Count)
            {
                Scene scene = m_scenes[(int)m_currentSceneIndex];
                Camera camera = scene.GetCurrentCamera();

                device_.ImmediateContext.ClearRenderTargetView(renderView_, Color.Blue);

                foreach (IRenderable renderable in scene.GetObjectsToDraw())
                {
                    if (!renderable.IsVisible)
                    {
                        continue;
                    }

                    device_.ImmediateContext.InputAssembler.InputLayout = renderable.Layout;
                    device_.ImmediateContext.InputAssembler.PrimitiveTopology = renderable.Mesh.Topology;
                    device_.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderable.Mesh.Vertices, renderable.Mesh.Stride, renderable.Mesh.Offset));
                    /*
                    renderable.Effect.GetVariableByName("worldViewProj").AsMatrix().SetMatrix(renderable.World * camera.ViewProj);
                    renderable.Effect.GetVariableByName("div").AsScalar().Set(renderable.SubDLevel);
                    renderable.Effect.GetVariableByName("eyePos").AsVector().Set(((SlimDX11Renderer.Locator)camera.Locator).Position);
                    renderable.Effect.GetVariableByName("layer").AsScalar().Set(renderable.Layer);
                    renderable.Effect.GetVariableByName("lightDir").AsVector().Set(((SlimDX11Renderer.Locator)lights_[lightIndex_].Direction).Direction);
                    */

                    renderable.Effect.GetVariableByName("scale").AsVector().Set(renderable.Scale);
                    renderable.Effect.GetVariableByName("colour").AsVector().Set(renderable.Colour);
                    renderable.Effect.GetVariableByName("entityPosition").AsVector().Set(renderable.ScreenPos);

                    if (renderable.IsCameraRelative)
                    {
                        renderable.Effect.GetVariableByName("cameraPos").AsVector().Set(Vector2.Zero);
                    }
                    else
                    {
                        renderable.Effect.GetVariableByName("cameraPos").AsVector().Set(scene.CameraPos);
                    }

                    renderable.SetSamplers();
                    renderable.SetTextures();

                    for (int i = 0; i < renderable.Technique.Description.PassCount; ++i)
                    {
                        renderable.Technique.GetPassByIndex(i).Apply(device_.ImmediateContext);
                        device_.ImmediateContext.Draw(renderable.Mesh.VertexCount, renderable.Mesh.StartVertex);
                    }
                }

                swapChain_.Present(0, PresentFlags.None);
            }
        }

        public Core.IRenderableScene GetNewScene()
        {
            var scene = new Scene(device_, samplerStates_["minMagMipLinear"], new Vector2(form_.Width, form_.Height));
            m_scenes.Add(scene);
            return scene;
        }
    }
}

