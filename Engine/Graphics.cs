using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.Mathematics.Interop;

using static Engine.Graphics;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceChild = SharpDX.Direct3D11.DeviceChild;

namespace Engine
{
    public interface IMesh
    {
        void Draw();
    }

    public sealed class Graphics : Disposable
    {
        public const byte sample = 4;

        public Graphics()
        {
            device = new Device(DriverType.Hardware);
            context = device.ImmediateContext;
        }

        internal static Device device;
        internal static DeviceContext context;

        protected override void Dispose(bool disposing)
        {
            device.Dispose();
            device = null;
            context = null;
        }
    }

    public sealed class RenderState : Disposable
    {
        public RenderState(bool depthEnabled, bool depthWrite, bool wireframe, bool add)
        {
            var stencilStateDesc = new DepthStencilStateDescription
            {
                IsDepthEnabled = depthEnabled,
                DepthWriteMask = depthWrite ? DepthWriteMask.All : DepthWriteMask.Zero,
                DepthComparison = Comparison.LessEqual,
                IsStencilEnabled = false,
            };
            var rasterizerStateDesc = new RasterizerStateDescription
            {
                IsAntialiasedLineEnabled = true,
                CullMode = CullMode.Front,
                DepthBias = 0,
                DepthBiasClamp = 0,
                IsDepthClipEnabled = true,
                FillMode = wireframe ? FillMode.Wireframe : FillMode.Solid,
                IsFrontCounterClockwise = true,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            };
            var renderTargetDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = add,
                SourceBlend = BlendOption.One,
                DestinationBlend = add ? BlendOption.One : BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
            };
            var blendStateDesc = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false
            };

            blendStateDesc.RenderTarget[0] = renderTargetDesc;

            depthStencil = new DepthStencilState(device, stencilStateDesc);
            rasterizer = new RasterizerState(device, rasterizerStateDesc);
            blend = new BlendState(device, blendStateDesc);
        }

        private DepthStencilState depthStencil;
        private RasterizerState rasterizer;
        private BlendState blend;

        public void Apply()
        {
            context.OutputMerger.SetDepthStencilState(depthStencil);
            context.OutputMerger.SetBlendState(blend);
            context.Rasterizer.State = rasterizer;
        }

        protected override void Dispose(bool disposing)
        {
            depthStencil.Dispose();
            rasterizer.Dispose();
            blend.Dispose();

            depthStencil = null;
            rasterizer = null;
            blend = null;
        }
    }

    public abstract class View : Disposable
    {
        public View()
        {

        }
        internal View(Texture2D texture)
        {
            Sample = texture.Description.SampleDescription.Count;
            Width = texture.Description.Width;
            Height = texture.Description.Height;
        }

        public int Sample { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
    }

    public sealed class RenderView : View
    {
        public static void Create(int width, int height, out RenderView renderView, out ShaderView shaderView)
        {
            var desc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None
            };

            var texture = new Texture2D(device, desc);

            renderView = new RenderView(texture);
            shaderView = new ShaderView(texture);
        }

        internal RenderView(Texture2D texture) : base(texture)
        {
            var desc = new Texture2DDescription
            {
                Width = Width,
                Height = Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(Sample, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None
            };

            using (var buffer = new Texture2D(device, desc))
            {
                render = new RenderTargetView(device, texture);
                stencil = new DepthStencilView(device, buffer);
            }
        }

        private RenderTargetView render;
        private DepthStencilView stencil;

        public void Apply()
        {
            context.OutputMerger.SetRenderTargets(stencil, render);
            context.Rasterizer.SetViewport(0, 0, Width, Height);
        }
        public void Clear()
        {
            context.ClearRenderTargetView(render, new RawColor4());
            context.ClearDepthStencilView(stencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
        }
        public void Clear(float r, float g, float b)
        {
            context.ClearRenderTargetView(render, new RawColor4(r, g, b, 1));
            context.ClearDepthStencilView(stencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
        }
        public void ClearDepthStencil()
        {
            context.ClearDepthStencilView(stencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
        }

        protected override void Dispose(bool disposing)
        {
            render.Dispose();
            stencil.Dispose();

            render = null;
            stencil = null;
        }
    }

    public class ShaderView : View
    {
        internal ShaderView(Texture2D texture) : base(texture)
        {
            resource = new ShaderResourceView(device, texture);
        }

        private ShaderResourceView resource;

        public void Apply()
        {
            context.PixelShader.SetShaderResource(0, resource);
        }

        protected override void Dispose(bool disposing)
        {
            resource.Dispose();
            resource = null;
        }
    }

    public sealed class Shader : Disposable
    {
        public Shader(ShaderStage stage, byte[] code)
        {
            switch (stage)
            {
                case ShaderStage.Compute:
                    target = context.ComputeShader;
                    break;
                case ShaderStage.Vertex:
                    target = context.VertexShader;
                    break;
                case ShaderStage.Hull:
                    target = context.HullShader;
                    break;
                case ShaderStage.Domain:
                    target = context.DomainShader;
                    break;
                case ShaderStage.Geometry:
                    target = context.GeometryShader;
                    break;
                case ShaderStage.Pixel:
                    target = context.PixelShader;
                    break;
            }

            if (code == null) return;

            switch (stage)
            {
                case ShaderStage.Compute:
                    shader = new ComputeShader(device, code);
                    break;
                case ShaderStage.Vertex:
                    shader = new VertexShader(device, code);
                    break;
                case ShaderStage.Hull:
                    shader = new HullShader(device, code);
                    break;
                case ShaderStage.Domain:
                    shader = new DomainShader(device, code);
                    break;
                case ShaderStage.Geometry:
                    shader = new GeometryShader(device, code);
                    break;
                case ShaderStage.Pixel:
                    shader = new PixelShader(device, code);
                    break;
            }

            using (var reflection = new ShaderReflection(code))
            {
                if (reflection.Description.ConstantBuffers == 0) return;

                using (var buffer = reflection.GetConstantBuffer(0))
                {
                    var size = buffer.Description.Size;

                    data = new byte[size];

                    var desc = new BufferDescription()
                    {
                        SizeInBytes = size,
                        BindFlags = BindFlags.ConstantBuffer,
                        Usage = ResourceUsage.Default,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None,
                    };

                    this.buffer = new Buffer(device, desc);
                }
            }
        }

        private bool changed;
        private byte[] data;
        private Buffer buffer;
        private DeviceChild shader;
        private CommonShaderStage target;

        public void SetParameter(int offset, int value)
        {
            data.Insert(value, offset);
            changed = true;
        }
        public void SetParameter(int offset, float value)
        {
            data.Insert(value, offset);
            changed = true;
        }
        public void SetParameter(int offset, IArray value)
        {
            data.Insert(value, offset);
            changed = true;
        }
        public void Apply()
        {
            target.SetShader(shader, null, 0);
        }
        public void Pass()
        {
            if (changed) context.UpdateSubresource(data, buffer);
            target.SetConstantBuffers(0, buffer);
            changed = false;
        }

        protected override void Dispose(bool disposing)
        {
            shader?.Dispose();
            buffer?.Dispose();

            buffer = null;
            data = null;
            target = null;
            shader = null;
        }
    }

    public sealed class Layout : Disposable
    {
        public Layout(byte[] code)
        {
            using (var reflection = new ShaderReflection(code))
            {
                var count = reflection.Description.InputParameters;
                var elements = new InputElement[count];

                for (int i = 0; i < count; i++)
                {
                    var input = reflection.GetInputParameterDescription(i);
                    var format = Format.Unknown;
                    var name = input.SemanticName;
                    var index = input.SemanticIndex;
                    var slot = name == "INSTANCE" ? 1 : 0;
                    var clas = slot == 1 ? InputClassification.PerInstanceData : InputClassification.PerVertexData;

                    if ((input.UsageMask & RegisterComponentMaskFlags.ComponentW) > 0) format = Format.R32G32B32A32_Float;
                    else if ((input.UsageMask & RegisterComponentMaskFlags.ComponentZ) > 0) format = Format.R32G32B32_Float;
                    else if ((input.UsageMask & RegisterComponentMaskFlags.ComponentY) > 0) format = Format.R32G32_Float;
                    else if ((input.UsageMask & RegisterComponentMaskFlags.ComponentX) > 0) format = Format.R32_Float;

                    elements[i] = new InputElement(name, index, format, -1, slot, clas, slot);
                }

                layout = new InputLayout(device, code, elements);
            }
        }

        private InputLayout layout;

        public void Apply()
        {
            context.InputAssembler.InputLayout = layout;
        }

        protected override void Dispose(bool disposing)
        {
            layout.Dispose();
            layout = null;
        }
    }

    public abstract class SimpleMesh : Disposable
    {
        public static SimpleMesh Quad
        {
            get
            {
                var quad = new Vertex[]
                {
                    new Vertex( 1,  1,  0,  0,  0,  1,  1,  0),
                    new Vertex( 1, -1,  0,  0,  0,  1,  1,  1),
                    new Vertex(-1,  1,  0,  0,  0,  1,  0,  0),
                    new Vertex(-1, -1,  0,  0,  0,  1,  0,  1),
                };

                return new SimpleMesh<Vertex>(quad, Topology.Strip);
            }
        }

        public SimpleMesh(Topology topology)
        {
            this.topology = (PrimitiveTopology)topology;
        }

        private Buffer vertices;
        private VertexBufferBinding binding;
        private PrimitiveTopology topology;

        public int VertexCount { get; private set; }

        public virtual void Draw()
        {
            Pass();
            context.Draw(VertexCount, 0);
        }

        protected void Initialize<T>(T[] vertices) where T : struct
        {
            var stride = Utilities.SizeOf<T>();
            var buffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
            var binding = new VertexBufferBinding(buffer, stride, 0);

            this.vertices = buffer;
            this.binding = binding;

            VertexCount = vertices.Length;
        }
        protected virtual void Pass()
        {
            context.InputAssembler.PrimitiveTopology = topology;
            context.InputAssembler.SetVertexBuffers(0, binding);
        }
        protected override void Dispose(bool disposing)
        {
            vertices.Dispose();

            vertices = null;
            binding.Buffer = null;
        }
    }

    public abstract class Mesh : SimpleMesh
    {
        public Mesh(Topology topology) : base(topology)
        {

        }

        private Buffer indices;

        public int IndexCount { get; private set; }

        public override void Draw()
        {
            Pass();
            context.DrawIndexed(IndexCount, 0, 0);
        }

        protected void Initialize<T>(int[] indices)
        {
            this.indices = Buffer.Create(device, BindFlags.IndexBuffer, indices);

            IndexCount = indices.Length;
        }
        protected override void Pass()
        {
            base.Pass();
            context.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            indices.Dispose();
            indices = null;
        }
    }

    public sealed class SimpleMesh<T> : SimpleMesh where T : struct
    {
        public SimpleMesh(T[] vertices, Topology topology) : base(topology)
        {
            Initialize(vertices);
        }
    }

    public sealed class Mesh<T> : Mesh where T : struct
    {
        public Mesh(int[] indices, T[] vertices, Topology topology) : base(topology)
        {
            Initialize(vertices);
            Initialize(indices);
        }
    }

    public sealed class ParticleSystem : Disposable
    {
        public static ParticleSystem Create()
        {
            var quad = new Vertex[]
            {
                new Vertex( 1,  1,  0,  0,  0,  1,  1,  0),
                new Vertex( 1, -1,  0,  0,  0,  1,  1,  1),
                new Vertex(-1,  1,  0,  0,  0,  1,  0,  0),
                new Vertex(-1, -1,  0,  0,  0,  1,  0,  1),
            };

            return Create(quad, Topology.Strip);
        }
        public static ParticleSystem Create<T>(params T[] instances) where T : struct, IArray
        {
            var result = Create();

            result.Stride = instances[0].ByteSize;
            result.Size = instances.Length;

            result.Add(instances);

            return result;
        }
        public static ParticleSystem Create<T>(T[] vertex, Topology topology) where T : struct
        {
            var stride = Utilities.SizeOf<T>();
            var vertices = Buffer.Create(device, BindFlags.VertexBuffer, vertex);

            return new ParticleSystem()
            {
                vertexCount = vertex.Length,
                instanceCount = 0,
                vertices = vertices,
                vBinding = new VertexBufferBinding(vertices, stride, 0),
                iBinding = new VertexBufferBinding(),
                topology = (PrimitiveTopology)topology
            };
        }
        public static ParticleSystem Create<T, V>(T[] vertex, Topology topology, params V[] instances) where T : struct where V : struct, IArray
        {
            var result = Create(vertex, topology);

            result.Stride = instances[0].ByteSize;
            result.Size = instances.Length;

            result.Add(instances);

            return result;
        }

        private bool changed;
        private int vertexCount;
        private int instanceCount;
        private byte[] data;
        private Buffer vertices;
        private Buffer instances;
        private VertexBufferBinding vBinding;
        private VertexBufferBinding iBinding;
        private PrimitiveTopology topology;

        public int Size
        {
            get => data.Length;
            set
            {
                instances?.Dispose();

                var data = new byte[value * Stride];

                if (instanceCount > 0) data.Insert(this.data, instanceCount, 0);

                this.data = data;

                instances = Buffer.Create(device, BindFlags.VertexBuffer, data);
                iBinding.Buffer = instances;

                changed = false;
            }
        }
        public int Stride
        {
            get => iBinding.Stride;
            set => iBinding.Stride = value;
        }
        public Topology Topology
        {
            get => (Topology)topology;
            set => topology = (PrimitiveTopology)value;
        }

        public void Add<T>(T instanceData) where T : IArray
        {
            data.Insert(instanceData, instanceCount * Stride);
            instanceCount += 1;
            changed = true;
        }
        public void Add<T>(T[] instanceData) where T : IArray
        {
            foreach (var data in instanceData)
                Add(data);
        }
        public void Clear()
        {
            data.Clear();
            instanceCount = 0;
            changed = true;
        }
        public void Draw()
        {
            Update();

            context.InputAssembler.PrimitiveTopology = topology;
            context.InputAssembler.SetVertexBuffers(0, vBinding, iBinding);
            context.DrawInstanced(vertexCount, instanceCount, 0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            vertices?.Dispose();
            instances?.Dispose();

            data = null;
            vertices = null;
            instances = null;
            vBinding.Buffer = null;
            iBinding.Buffer = null;
        }

        private void Update()
        {
            if (!changed) return;

            context.UpdateSubresource(data, instances);
            changed = false;
        }
    }

    public enum ShaderStage : byte
    {
        Compute,
        Vertex,
        Domain,
        Hull,
        Geometry,
        Pixel
    }

    public enum Topology : byte
    {
        Points = 1,
        List = 4,
        Strip = 5,
        ListAdj = 12,
        StripAdj = 13,
        Tris = 35,
        Quads = 36,
        Cube = 40,
    }
}
