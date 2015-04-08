using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace GameWorld
{
    public class CellVisualiser
    {
        IRenderer m_renderer;
        IRenderableScene m_scene;

        RenderArrays3d m_renderArrays;

        Random m_random;

        Colour[] m_baseColours;

        List<List<Vector3d>> m_positions;

        const float Scale = 1000.0f;

        int countdown = 0;
        int activeList = 0;

        public CellVisualiser(IRenderer renderer)
        {
            m_renderer = renderer;
            m_scene = m_renderer.GetNewScene();

            m_positions = new List<List<Vector3d>>();

            m_random = new Random();

            m_renderArrays = new RenderArrays3d();
            m_renderArrays.Positions = new List<Vector3d>();
            m_renderArrays.Colours = new List<Colour>();

            m_scene.CreateCamera();
            m_scene.SetCurrentCamera(0);

            m_scene.RenderArrays3d.Add(m_renderArrays);

            m_baseColours = new Colour[11];

            m_baseColours[00] = new Colour() { R = 1.0f, G = 0.0f, B = 0.0f, A = 0.0f };
            m_baseColours[01] = new Colour() { R = 0.0f, G = 1.0f, B = 0.0f, A = 0.0f };
            m_baseColours[02] = new Colour() { R = 0.0f, G = 0.0f, B = 1.0f, A = 0.0f };
            m_baseColours[03] = new Colour() { R = 1.0f, G = 1.0f, B = 0.0f, A = 0.0f };
            m_baseColours[04] = new Colour() { R = 0.0f, G = 1.0f, B = 1.0f, A = 0.0f };
            m_baseColours[05] = new Colour() { R = 1.0f, G = 0.5f, B = 0.5f, A = 0.0f };
            m_baseColours[06] = new Colour() { R = 0.5f, G = 1.0f, B = 0.5f, A = 0.0f };
            m_baseColours[07] = new Colour() { R = 0.5f, G = 0.5f, B = 1.0f, A = 0.0f };
            m_baseColours[08] = new Colour() { R = 1.0f, G = 1.0f, B = 0.5f, A = 0.0f };
            m_baseColours[09] = new Colour() { R = 0.5f, G = 1.0f, B = 1.0f, A = 0.0f };
            m_baseColours[10] = new Colour() { R = 1.0f, G = 0.5f, B = 1.0f, A = 0.0f };

            LoadFile("C:/Users/Tim/Desktop/UbuntuScratch/chasteOutput/OverlappingSpheres/results_from_time_0/results.viznodes");
        }

        public void LoadFile(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path, Encoding.UTF8);
            string line = file.ReadLine();

            while (line != null)
            {
                string[] entries = line.Split('\t');
                string[] components = entries[1].Split(' ');
                List<Vector3d> vectorList = new List<Vector3d>();

                int numVectors = (components.Count() - 1) / 3;

                for (int vectorId = 0; vectorId < numVectors; vectorId++)
                {
                    int componentId = vectorId * 3;

                    float x, y, z;
                    bool success = float.TryParse(components[componentId], out x);
                    success = float.TryParse(components[componentId + 1], out y);
                    success = float.TryParse(components[componentId + 2], out z);

                    if (success)
                    {
                        Vector3d newVec = new Vector3d(x * Scale, y * Scale, z * Scale);
                        vectorList.Add(newVec);
                    }
                }

                m_positions.Add(vectorList);
                line = file.ReadLine();
            }

            file.Close();
        }

        void SwitchActiveList(int newActive)
        {
            m_renderArrays.Positions = m_positions[newActive];
            while (m_renderArrays.Colours.Count < m_positions[newActive].Count)
            {
                m_renderArrays.Colours.Add(m_baseColours[0]);
            }
            while (m_renderArrays.Colours.Count > m_positions[newActive].Count)
            {
                m_renderArrays.Colours.RemoveAt(m_renderArrays.Colours.Count - 1);
            }
        }

        public void Tick()
        {
            countdown--;

            if (countdown < 0)
            {
                countdown = 1;
                activeList++;

                if (activeList >= m_positions.Count)
                {
                    activeList = 0;
                }

                SwitchActiveList(activeList);
            }
        }
    }
}
