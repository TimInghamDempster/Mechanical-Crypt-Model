using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IRenderableScene
    {
        void CreateCamera();
        void SetCurrentCamera(int cameraIndex);
        IRenderableItem GetNewRenderable(string effectFilename, string techniqueName, string textureFilename, bool isCameraRelative);
        IRenderableItem GetNewBlobRenderable(string effectFilename, string techniqueName, float size);
        void UpdateCameraPos(float x, float y);
        float Width { get; }
        float Height { get; }
        List<Core.RenderArrays> RenderArrays { get; set; }
        List<Core.RenderArrays3d> RenderArrays3d { get; set; }
    }
}
