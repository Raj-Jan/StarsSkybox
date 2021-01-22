using Engine;

using System;
using System.IO;
using System.Collections.Generic;

using static Engine.Core;

namespace Game
{
    internal static class Program
    {
        private static void Main()
        {
            using (var graphics = new Graphics())
            using (var platform = new Platform())
                platform.Run();
        }
    }

    public sealed class Platform : Core
    {
        private Skybox skybox = new Skybox();
        private Camera camera = new Camera(new Vector3(0, 0, 1), new Vector3(0, 1, 0), Utils._120D , Utils._4D, 9f / 16, 1, 2)
        {
            Sensivity = 1f,
            Inertia = 10
        };
        private Vector3 sun = new Vector3(0, 0, 1);

        protected override void Initialize()
        {
            skybox.Load();

            Cursor = false;
            base.Initialize();
        }
        protected override void Quit()
        {
            base.Quit();
        }
        protected override void Update(ITime time)
        {
            if (Keyboard.IsKey(Key.Escape, KeyState.JustRelesed)) Exit();

            camera.Update(time);

            //sun = Matrix.CreateRotationX(time.Elapsed) * sun; 
        }
        protected override void Draw()
        {
            base.Draw();
            skybox.Draw(camera, sun);
        }
        protected override void Dispose(bool disposing)
        {
            skybox.Dispose();
            skybox = null;

            base.Dispose(disposing);
        }
    }

    public class Camera : DynamicBody
    {
        public Camera(Vector3 direction, Vector3 up, float fov0, float fov1, float aspectInv, float near, float far)
        {
            projection0 = Matrix.CreateProjection(fov0, aspectInv);
            projection1 = Matrix.CreateProjection(fov1, aspectInv);
            World = Matrix.CreateView(Vector3.Zero, direction.Normalize(), up);

            this.fov0 = new Vector2(fov0, aspectInv * fov0) / 2;
            this.fov1 = new Vector2(fov1, aspectInv * fov1) / 2;
        }

        private float mix;
        private Vector2 fov0;
        private Vector2 fov1;
        private Matrix projection0;
        private Matrix projection1;

        private Matrix cproj => (1 - Zoom) * projection0 + Zoom * projection1;

        public float Zoom => (float)Math.Sqrt(mix);
        public Matrix Projection => cproj * World;
        public Vector2 Aspect => new Vector2(cproj.X.X, cproj.Y.Y);
        public Vector2 FOV => (1 - Zoom) * fov0 + Zoom * fov1;

        public float Sensivity { get; set; }
        public float Inertia { get; set; }

        public override void Update(ITime time)
        {
            var mouse = new Vector3
            {
                X = Mouse.Velocity.X,
                Y = Mouse.Velocity.Y
            };

            VelocityA += time.Elapsed * (Sensivity * World * mouse - Inertia * VelocityA);

            if (Keyboard.IsKey(Key.LeftShift, KeyState.Pressed)) mix += time.Elapsed / 4;
            if (Keyboard.IsKey(Key.LeftControl, KeyState.Pressed)) mix -= time.Elapsed / 4;

            if (mix < 0) mix = 0;
            if (mix > 1) mix = 1;

            base.Update(time);
        }
    }

    public class Skybox : Disposable
    {
        private Starfield starfield = new Starfield();
        private Sun sun = new Sun();
        private Flare flare = new Flare();

        public void Load()
        {
            starfield.Load();
            sun.Load();
            flare.Load();
        }
        public void Draw(Camera camera, Vector3 sunPos)
        {
            starfield.Draw(camera);
            sun.Draw(camera, sunPos);
            flare.Draw(camera, sunPos);
        }

        protected override void Dispose(bool disposing)
        {
            starfield.Dispose();
            sun.Dispose();
            flare.Dispose();
        }
    }

    public class Starfield : Disposable
    {
        private RenderState state;
        private Layout layout;
        private Shader vs;
        private Shader ps;

        private ShaderView texture;
        private ParticleSystem stars;

        public void Load()
        {
            state = new RenderState(false, false, false, true);
            layout = HLSL.CompileLayout(ShaderStage.Vertex, "Resources/starfield.hlsl");
            vs = HLSL.CompileShader(ShaderStage.Vertex, "Resources/starfield.hlsl");
            ps = HLSL.CompileShader(ShaderStage.Pixel, "Resources/starfield.hlsl");

            texture = PNG.FromFile("Resources/star.png");

            List<Star> stars = new List<Star>();

            using (var stream = File.OpenRead("Resources/starfield"))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length)
                {
                    var star = new Star
                    {
                        Pos = new Vector3
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle(),
                        },
                        Color = new Color
                        {
                            R = reader.ReadSingle(),
                            G = reader.ReadSingle(),
                            B = reader.ReadSingle(),
                            A = reader.ReadSingle(),
                        }
                    };

                    var col = star.Color;
                    col.R = 1;
                    col.G = 1;
                    col.B = 1;

                    star.Color = col;

                    stars.Add(star);
                }
            }

            this.stars = ParticleSystem.Create(stars.ToArray());
        }
        public void Draw(Camera camera)
        {
            state.Apply();
            layout.Apply();
            vs.Apply();
            ps.Apply();

            var fov = camera.FOV;

            vs.SetParameter(0, camera.World);
            vs.SetParameter(64, fov);
            vs.Pass();

            texture.Apply();
            stars.Draw();
        }

        protected override void Dispose(bool disposing)
        {
            state.Dispose();
            layout.Dispose();
            vs.Dispose();
            ps.Dispose();

            texture.Dispose();
            stars.Dispose();
        }
    }

    public class Sun : Disposable
    {
        private RenderState state;
        private Layout layout;
        private Shader vs;
        private Shader ps;

        private ShaderView texture;
        private SimpleMesh quad;

        public void Load()
        {
            state = new RenderState(false, false, false, false);
            layout = HLSL.CompileLayout(ShaderStage.Vertex, "Resources/sun.hlsl");
            vs = HLSL.CompileShader(ShaderStage.Vertex, "Resources/sun.hlsl");
            ps = HLSL.CompileShader(ShaderStage.Pixel, "Resources/sun.hlsl");

            texture = PNG.FromFile("Resources/sun.png");
            quad = SimpleMesh.Quad;
        }
        public void Draw(Camera camera, Vector3 sunPos)
        {
            state.Apply();
            layout.Apply();
            vs.Apply();
            ps.Apply();

            vs.SetParameter(0, ~camera.World & sunPos);
            vs.SetParameter(16, camera.FOV);
            vs.SetParameter(24, 0.7f);
            vs.Pass();

            texture.Apply();
            quad.Draw();
        }

        protected override void Dispose(bool disposing)
        {
            state.Dispose();
            layout.Dispose();
            vs.Dispose();
            ps.Dispose();

            texture.Dispose();
            quad.Dispose();
        }
    }

    public class Flare : Disposable
    {
        private RenderState state;
        private Layout layout;
        private Shader vs;
        private Shader ps;

        private ShaderView texture;
        private ParticleSystem ghosts;

        public void Load()
        {
            state = new RenderState(false, false, false, true);
            layout = HLSL.CompileLayout(ShaderStage.Vertex, "Resources/flare.hlsl");
            vs = HLSL.CompileShader(ShaderStage.Vertex, "Resources/flare.hlsl");
            ps = HLSL.CompileShader(ShaderStage.Pixel, "Resources/flare.hlsl");

            texture = PNG.FromFile("Resources/flare.png");

            var particles = new Color[]
            {
                new Color(0.04f, -0.3f, 0.08f, 0),
                new Color(0.04f, -0.8f, 0.12f, 0),
                new Color(0.05f, -1,    0.1f,  0),

                new Color(0.11f,   0.2f, 0.07f, 0.5f),
                new Color(0.12f, -0.5f, 0.07f, 0.5f),
            };

            var quad = new Vertex[]
            {
                new Vertex( 1,  1,  0,  0,  0,  1,  0.5f,  0),
                new Vertex( 1, -1,  0,  0,  0,  1,  0.5f,  1),
                new Vertex(-1,  1,  0,  0,  0,  1,  0,     0),
                new Vertex(-1, -1,  0,  0,  0,  1,  0,     1),
            };

            ghosts = ParticleSystem.Create(quad, Topology.Strip, particles);
        }
        public void Draw(Camera camera, Vector3 lightDir)
        {
            state.Apply();
            layout.Apply();
            vs.Apply();
            ps.Apply();

            vs.SetParameter(0, ~camera.World & lightDir);
            vs.SetParameter(16, camera.FOV);
            vs.Pass();

            texture.Apply();
            ghosts.Draw();
        }

        protected override void Dispose(bool disposing)
        {
            state.Dispose();
            layout.Dispose();
            vs.Dispose();
            ps.Dispose();

            texture.Dispose();
            ghosts.Dispose();
        }
    }

    public struct Star : IArray
    {
        public Star(float theta, float phi, float bv, float mag)
        {
            var temp = GetTemp(bv);

            Pos = new Vector3(theta, phi);
            Color = GetColor(temp, mag);
        }

        public Vector3 Pos { get; set; }
        public Color Color { get; set; }

        private static float GetTemp(float bv)
        {
            bv = 0.92f * bv;
            return 4600 * (1 / (bv + 1.7f) + 1 / (bv + 0.62f));
        }
        private static Color GetColor(float temp, float mag)
        {
            temp /= 100;

            var red = 255d;
            var green = 0d;
            var blue = 255d;
            var alpha = 0d;

            if (temp > 66)
            {
                red = temp - 66;
                red = 329.698727446 * Math.Pow(red, -0.1332047592);
                Clamp(ref red);
            }

            if (temp < 66)
            {
                green = temp;
                green = 99.4708025861 * Math.Log(green) - 161.1195681661;
                Clamp(ref green);
            }
            else
            {
                green = temp - 60;
                green = 288.1221695283 * Math.Pow(green, -0.0755148492);
                Clamp(ref green);
            }

            if (temp < 66)
            {
                if (temp < 19)
                {
                    blue = 0;
                }
                else
                {
                    blue = temp - 10;
                    blue = 138.5177312231 * Math.Log(blue) - 305.0447927307;
                    Clamp(ref blue);
                }
            }

            mag = -14.18f - mag;
            alpha = Math.Pow(10, mag / 2.5);
            alpha = Math.Pow(alpha, 1d / 3);

            return new Color((float)red / 255, (float)green / 255, (float)blue / 255, (float)alpha);
        }
        private static void Clamp(ref double x)
        {
            if (x < 0) x = 0;
            if (x > 255) x = 255;
        }

        int IArray.ByteSize
        {
            get => 32;
        }
        Array IArray.Data
        {
            get => new float[] { Pos.X, Pos.Y, Pos.Z, 0, Color.R, Color.G, Color.B, Color.A };
        }
    }
}
