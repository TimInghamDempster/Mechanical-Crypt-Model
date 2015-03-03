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
    public class Mesh : IDisposable
    {
        SlimDX.Direct3D11.Buffer vertices_;
        PrimitiveTopology topology_;

        int stride_;
        int offset_;
        int vertCount_;
        int startVert_;

        public SlimDX.Direct3D11.Buffer Vertices
        {
            get
            {
                return vertices_;
            }
        }


        public PrimitiveTopology Topology
        {
            get
            {
                return topology_;
            }
        }
        
        public int Stride
        {
            get
            {
                return stride_;
            }
        }

        public int Offset
        {
            get
            {
                return offset_;
            }
        }

        public int VertexCount
        {
            get
            {
                return vertCount_;
            }
        }

        public int StartVertex
        {
            get
            {
                return startVert_;
            }
        }

        public Mesh(Device device, DataStream vertices,  PrimitiveTopology topology, int stride)
        {
            topology_ = topology;

            offset_ = 0;
            startVert_ = 0;
            stride_ = stride;
            vertCount_ = (int)vertices.Length / stride;

            vertices_ = new SlimDX.Direct3D11.Buffer(device, vertices, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)vertices.Length,
                Usage = ResourceUsage.Default
            });
        }
        
        public static DataStream MakeSpherePatches(int divTheta, int divPhi)
        {
            if (divTheta < 3)
            {
                throw new ArgumentException("divTheta must be greater than 2");
            }
            if(divPhi % 2 != 0)
            {
                throw new ArgumentException("divPhi must be an even number");
            }
            var array = new List<Vector4>();

            float thetaStep = (float)(Math.PI / (double)divTheta);
            float phiStep = (float)(2.0 * Math.PI / (double)divPhi);
            int pointCount = 0;

            // top cap
            for (int p = 0; p < divPhi; p += 2)
            {
                array.Add(new Vector4((float)(Math.Sin(thetaStep) * Math.Cos((float)p * phiStep)), (float)Math.Cos(thetaStep), (float)(Math.Sin(thetaStep) * Math.Sin((float)p * phiStep)), 1.0f));
                array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                array.Add(new Vector4((float)(Math.Sin(thetaStep) * Math.Cos(((float)p + 1.0f) * phiStep)), (float)Math.Cos(thetaStep), (float)(Math.Sin(thetaStep) * Math.Sin(((float)p + 1.0f) * phiStep)), 1.0f));
                array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                array.Add(new Vector4((float)(Math.Sin(thetaStep) * Math.Cos(((float)p + 2.0f) * phiStep)), (float)Math.Cos(thetaStep), (float)(Math.Sin(thetaStep) * Math.Sin(((float)p + 2.0f) * phiStep)), 1.0f));
                array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                array.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                pointCount += 4;
            }

            for (int p = 0; p < divPhi; p++)
            {
                for (int t = 1; t < divTheta-1; t++)
                {
                    float x = (float)(Math.Sin((float)t * thetaStep) * Math.Cos((float)p * phiStep));
                    float y = (float)Math.Cos((float)t * thetaStep);
                    float z = (float)(Math.Sin((float)t * thetaStep) * Math.Sin((float)p * phiStep));
                    array.Add(new Vector4(x, y, z, 1.0f));
                    array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));

                    x = (float)(Math.Sin((float)(t+1) * thetaStep) * Math.Cos((float)p * phiStep));
                    y = (float)Math.Cos((float)(t+1) * thetaStep);
                    z = (float)(Math.Sin((float)(t+1) * thetaStep) * Math.Sin((float)p * phiStep));
                    array.Add(new Vector4(x, y, z, 1.0f));
                    array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));

                    x = (float)(Math.Sin((float)(t+1) * thetaStep) * Math.Cos((float)(p+1) * phiStep));
                    y = (float)Math.Cos((float)(t+1) * thetaStep);
                    z = (float)(Math.Sin((float)(t+1) * thetaStep) * Math.Sin((float)(p+1) * phiStep));
                    array.Add(new Vector4(x, y, z, 1.0f));
                    array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));

                    x = (float)(Math.Sin((float)t * thetaStep) * Math.Cos((float)(p + 1) * phiStep));
                    y = (float)Math.Cos((float)(t) * thetaStep);
                    z = (float)(Math.Sin((float)t * thetaStep) * Math.Sin((float)(p + 1) * phiStep));
                    array.Add(new Vector4(x, y, z, 1.0f));
                    array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));


                    pointCount += 4;
                }
            }

            for (int p = 0; p < divPhi; p += 2)
            {

                array.Add(new Vector4((float)(Math.Sin(thetaStep * (float)(divTheta - 1)) * Math.Cos(((float)p + 1.0f) * phiStep)), (float)Math.Cos(thetaStep * (float)(divTheta - 1)), (float)(Math.Sin(thetaStep * (float)(divTheta - 1)) * Math.Sin(((float)p + 1.0f) * phiStep)), 1.0f));
                array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                array.Add(new Vector4((float)(Math.Sin(thetaStep * (float)(divTheta - 1)) * Math.Cos((float)p * phiStep)), (float)Math.Cos(thetaStep * (float)(divTheta - 1)), (float)(Math.Sin(thetaStep * (float)(divTheta - 1)) * Math.Sin((float)p * phiStep)), 1.0f));
                array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                array.Add(new Vector4(0.0f, -1.0f, 0.0f, 1.0f));
                array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                array.Add(new Vector4((float)(Math.Sin(thetaStep * (float)(divTheta - 1)) * Math.Cos(((float)p + 2.0f) * phiStep)), (float)Math.Cos(thetaStep * (float)(divTheta - 1)), (float)(Math.Sin(thetaStep * (float)(divTheta - 1)) * Math.Sin(((float)p + 2.0f) * phiStep)), 1.0f));
                array.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                
                
                pointCount += 4;
            }

            var stream = new DataStream(pointCount * 32, true, true);
            stream.WriteRange(array.ToArray());
            stream.Position = 0;
            return stream;
        }

        public static Mesh MakeQuadMesh(Device device, float size)
        {
            var data = new List<Vector4>();

            data.Add(new Vector4(-1.0f * size, -1.0f * size, 0.50f, 1.0f));
            data.Add(new Vector4(0.1f, 0.1f, 0.5f, 0.0f));

            data.Add(new Vector4(-1.0f * size, size, 0.50f, 1.0f));
            data.Add(new Vector4(0.1f, 0.1f, 0.5f, 0.0f));

            data.Add(new Vector4(size, -1.0f * size, 0.50f, 1.0f));
            data.Add(new Vector4(0.1f, 0.1f, 0.5f, 0.0f));

            data.Add(new Vector4(size, size, 0.50f, 1.0f));
            data.Add(new Vector4(0.1f, 0.1f, 0.5f, 0.5f));

            var stream = new DataStream(128, true, true);
            stream.WriteRange(data.ToArray());
            stream.Position = 0;

            Mesh mesh = new Mesh(device, stream, PrimitiveTopology.TriangleStrip, 32);
            return mesh;
        }
        /*
        public void InitUnitTessSphere(Device device_)
        {
            var stream = MakeSpherePatches(8,16);

            vertices_ = new SlimDX.Direct3D11.Buffer(device_, stream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)stream.Length,
                Usage = ResourceUsage.Default
            });

            stream.Dispose();
            stride_ = 32;
            offset_ = 0;
            vertCount_ = (int)stream.Length / 32;
            startVert_ = 0;

            bytecode_ = ShaderBytecode.CompileFromFile("Tessellation.fx", "fx_5_0", ShaderFlags.None, EffectFlags.None);
            effect_ = new Effect(device_, bytecode_);
            technique_ = effect_.GetTechniqueByIndex(0);
            pass_ = technique_.GetPassByIndex(0);
            layout_ = new InputLayout(device_, pass_.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0) 
            });

            topology_ = PrimitiveTopology.PatchListWith4ControlPoints;
        }*/
        public void Dispose()
        {
            vertices_.Dispose();
        }
    }
}
