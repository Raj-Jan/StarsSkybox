using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

using System;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Drawing.Imaging;
using System.Collections.Generic;

using static Engine.Graphics;

namespace Engine
{
    public static class PNG
    {
        public static ShaderView FromFile(string path)
        {
            var texture = CreateTexture(path);
            return new ShaderView(texture);
        }

        private static Texture2D CreateTexture(string path)
        {
            using (var image = Image.FromFile(path))
            using (var bitmap = new Bitmap(image))
            {
                var rectangle = new Rectangle(System.Drawing.Point.Empty, bitmap.Size);
                var data = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                var desc = new Texture2DDescription()
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                };
                var bytes = new DataRectangle(data.Scan0, data.Stride);

                var ret = new Texture2D(device, desc, bytes);
                bitmap.UnlockBits(data);
                return ret;
            }
        }
    }

    public static class HLSL
    {
        public static Layout CompileLayout(ShaderStage stage, string path)
        {
            using (var code = GetCode(stage, path))
                return new Layout(code);
        }
        public static Shader CompileShader(ShaderStage stage, string path)
        {
            using (var code = GetCode(stage, path))
                return new Shader(stage, code);
        }

        private static ShaderBytecode GetCode(ShaderStage stage, string path)
        {
            switch (stage)
            {
                case ShaderStage.Compute: return ShaderBytecode.CompileFromFile(path, "MainCS", "cs_5_0");
                case ShaderStage.Vertex: return ShaderBytecode.CompileFromFile(path, "MainVS", "vs_5_0");
                case ShaderStage.Hull: return ShaderBytecode.CompileFromFile(path, "MainHS", "hs_5_0");
                case ShaderStage.Domain: return ShaderBytecode.CompileFromFile(path, "MainDS", "ds_5_0");
                case ShaderStage.Geometry: return ShaderBytecode.CompileFromFile(path, "MainGS", "gs_5_0");
                case ShaderStage.Pixel: return ShaderBytecode.CompileFromFile(path, "MainPS", "ps_5_0");
                default: return null;
            }

        }
    }

    public static class OBJ
    {
        public static Mesh FromFile(string file)
        {
            var triangles = new GeometryData();

            using (var stream = File.OpenRead(file))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(' ');

                    switch (line[0])
                    {
                        case "v":
                            ReadVector(triangles.Pos, 1, 3, line);
                            break;
                        case "vn":
                            ReadVector(triangles.Nor, 1, 3, line);
                            break;
                        case "vt":
                            ReadVector(triangles.Tex, 1, 2, line);
                            break;
                        case "f":
                            ReadTriangle(triangles, line);
                            break;
                    }
                }
            }

            var vertices = triangles.GetTriangleList(out var indices);

            return new Mesh<Vertex>(indices.ToArray(), vertices.ToArray(), Topology.List);
        }

        private static void ReadVector(List<Vector3> vectors, int offset, int size, string[] line)
        {
            var vec = new float[3];

            for (int i = 0; i < size; i++)
                vec[i] = float.Parse(line[offset++], CultureInfo.InvariantCulture);

            var vector = new Vector3(vec[0], vec[1], vec[2]);
            vectors.Add(vector);
        }
        private static void ReadTriangle(GeometryData triangles, string[] line)
        {
            var p0 = ReadVertex(line[1].Split('/'));
            var p2 = ReadVertex(line[2].Split('/'));
            var p4 = ReadVertex(line[3].Split('/'));

            triangles.AddTriangle(p0, p2, p4);
        }

        private static VertexOBJ ReadVertex(string[] xyz)
        {
            var pos = int.Parse(xyz[0], CultureInfo.InvariantCulture) - 1;
            var nor = int.Parse(xyz[2], CultureInfo.InvariantCulture) - 1;
            var tex = int.Parse(xyz[1], CultureInfo.InvariantCulture) - 1;

            return new VertexOBJ(pos, nor, tex);
        }
    }

    public class GeometryData
    {
        private List<Triangle> triangles = new List<Triangle>();

        public List<Vector3> Pos { get; } = new List<Vector3>();
        public List<Vector3> Nor { get; } = new List<Vector3>();
        public List<Vector3> Tex { get; } = new List<Vector3>();

        public void AddTriangle(Triangle triangle)
        {
            triangles.Add(triangle);

            foreach (var tri in triangles)
                tri.Adj(triangle);
        }
        public void AddTriangle(VertexOBJ p0, VertexOBJ p2, VertexOBJ p4)
        {
            var triangle = new Triangle(p0, p2, p4);
            AddTriangle(triangle);
        }
        public void RemoveTriangle(Triangle triangle)
        {
            triangles.Remove(triangle);
            foreach (var tri in triangles)
                tri.RemoveAdj(triangle);
        }

        public List<Vertex> GetTriangleList(out List<int> indices)
        {
            var result = new List<Vertex>();
            indices = new List<int>();

            for (int i = 0; i < triangles.Count; i++)
            {
                if (triangles[i] == null) continue;

                var array = triangles[i].ToArray();

                for (int j = 0; j < 6; j++)
                {
                    var vert = array[j].ToVertex(Pos, Nor, Tex);

                    if (result.Contains(vert)) indices.Add(result.IndexOf(vert));
                    else
                    {
                        indices.Add(result.Count);
                        result.Add(vert);
                    }
                }

            }

            return result;
        }
    }

    public class Triangle : IComparable<Triangle>
    {
        public Triangle(VertexOBJ p0, VertexOBJ p2, VertexOBJ p4)
        {
            vertices = new VertexOBJ[] { p0, p0, p2, p2, p4, p4 };
            adj = new Triangle[3];
        }

        private int connect;
        private VertexOBJ[] vertices;
        private Triangle[] adj;

        public void Adj(Triangle other)
        {
            if (connect == 3) return;

            for (int i = 0, j = 4, k = 2; i < 6; k = j, j = i, i += 2)
            {
                for (int l = 0, m = 4, n = 2; l < 6; n = m, m = l, l += 2)
                {
                    if (vertices[i].IsContinuos(other.vertices[m]) && vertices[j].IsContinuos(other.vertices[l]))
                    {
                        if (vertices[i].IsFullyContinuos(other.vertices[m]) && vertices[j].IsFullyContinuos(other.vertices[l]))
                        {
                            adj[j / 2] = other;
                            other.adj[m / 2] = this;

                            connect++;
                            other.connect++;
                        }

                        vertices[j + 1] = other.vertices[n];
                        other.vertices[m + 1] = vertices[k];
                    }
                }
            }
        }
        public void RemoveAdj(Triangle other)
        {
            if (other == null) return;

            for (int i = 0; i < 3; i++)
            {
                if (adj[i] == other)
                {
                    adj[i] = null;
                    connect--;
                }
            }
        }
        public void RemoveOtherAdj(Triangle other)
        {
            for (int i = 0; i < 3; i++)
                if (adj[i] != other)
                    RemoveAdj(adj[i]);
        }
        public void RemoveFromAdj()
        {
            if (adj[0] != null) adj[0].RemoveAdj(this);
            if (adj[1] != null) adj[1].RemoveAdj(this);
            if (adj[2] != null) adj[2].RemoveAdj(this);
        }
        public int CompareTo(Triangle other)
        {
            if (other == null) return 1;
            return other.connect - connect;
        }
        public Triangle Next()
        {
            var result = adj[0];
            var max = adj[0] != null ? adj[0].connect : 0;

            for (int i = 1; i < 3; i++)
                if (adj[i] != null)
                    if (adj[i].connect > max)
                        result = adj[i];

            return result;
        }

        public VertexOBJ[] ToArray()
        {
            return vertices;
        }
    }

    public struct VertexOBJ
    {
        public VertexOBJ(int pos, int nor, int tex)
        {
            this.pos = pos;
            this.tex = tex;
            this.nor = nor;
        }

        public int pos;
        public int nor;
        public int tex;

        public bool IsContinuos(VertexOBJ other)
        {
            return pos == other.pos;
        }
        public bool IsFullyContinuos(VertexOBJ other)
        {
            return IsContinuos(other) && nor == other.nor && tex == other.tex;
        }

        public Vertex ToVertex(List<Vector3> pos, List<Vector3> nor, List<Vector3> tex)
        {
            var p = pos[this.pos];
            var n = nor[this.nor];
            var t = tex[this.tex];

            return new Vertex(p, n, t);
        }
    }
}
