using SharpDX.DXGI;
using SharpDX.Win32;
using SharpDX.Direct3D11;

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Message = System.Windows.Forms.Message;

using static Engine.Graphics;

namespace Engine
{
    public abstract class Core : Disposable
    {
        private static Core instance;

        public static IKeyboard Keyboard
        {
            get => instance.window.Keyboard;
        }
        public static IMouse Mouse
        {
            get => instance.window.Mouse;
        }

        public Core()
        {
            timer = new Timer();
            window = new CoreWindow();

            using (var factory = new Factory1())
            using (var adapter = factory.GetAdapter1(0))
            using (var output = adapter.GetOutput(0))
            {
                var modes = output.GetDisplayModeList(Format.B8G8R8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
                var mode = modes[modes.Length - 1];
                var desc = new SwapChainDescription()
                {
                    ModeDescription = mode,
                    SampleDescription = new SampleDescription(sample, 0),
                    Usage = Usage.RenderTargetOutput,
                    BufferCount = 1,
                    OutputHandle = window.Handle,
                    IsWindowed = true,
                    SwapEffect = SwapEffect.Discard,
                    Flags = SwapChainFlags.None,
                };

                swapChain = new SwapChain(factory, device, desc);

                using (var texture = swapChain.GetBackBuffer<Texture2D>(0))
                    renderView = new RenderView(texture);
            }
        }
        public Core(Window window)
        {
            timer = new Timer();
            this.window = window;

            using (var factory = new Factory1())
            using (var adapter = factory.GetAdapter1(0))
            using (var output = adapter.GetOutput(0))
            {
                var modes = output.GetDisplayModeList(Format.B8G8R8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
                var mode = modes[modes.Length - 1];
                var desc = new SwapChainDescription()
                {
                    ModeDescription = mode,
                    SampleDescription = new SampleDescription(sample, 0),
                    Usage = Usage.RenderTargetOutput,
                    BufferCount = 1,
                    OutputHandle = window.Handle,
                    IsWindowed = true,
                    SwapEffect = SwapEffect.Discard,
                    Flags = SwapChainFlags.None,
                };

                swapChain = new SwapChain(factory, device, desc);

                using (var texture = swapChain.GetBackBuffer<Texture2D>(0))
                    renderView = new RenderView(texture);
            }
        }

        private bool active;
        private Timer timer;
        private Window window;
        private SwapChain swapChain;
        private RenderView renderView;

        protected bool Cursor
        {
            set
            {
                if (value) System.Windows.Forms.Cursor.Show();
                else System.Windows.Forms.Cursor.Hide();
            }
        }

        public void Run()
        {
            Initialize();

            while (active)
                Frame();

            Quit();
        }
        public void Exit()
        {
            active = false;
        }

        protected virtual void Initialize()
        {
            instance = this;
            active = true;
            window.Show();
            timer.Start();
        }
        protected virtual void Quit()
        {
            timer.Stop();
            window?.Hide();
            instance = null;
        }
        protected virtual void Update(ITime time)
        {

        }
        protected virtual void Draw()
        {
            renderView.Clear();
            renderView.Apply();
        }

        protected override void Dispose(bool disposing)
        {
            window.Dispose();
            swapChain.Dispose();
            renderView.Dispose();

            window = null;
            swapChain = null;
            renderView = null;
            timer = null;
        }

        private void Frame()
        {
            Update(timer);
            Draw();

            NativeMessage msg;
            while (Utils.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0) != 0)
            {
                if (Utils.GetMessage(out msg, IntPtr.Zero, 0, 0) == -1)
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new Exception($"Error code {error} occured while prcessing messages.");
                }

                if (msg.msg == 0x0112) active = msg.wParam.ToInt32() != 0x0000f060;
                if (msg.msg == 0x00a1) active = msg.wParam.ToInt32() != 0x00000014;

                var message = new Message()
                {
                    HWnd = msg.handle,
                    LParam = msg.lParam,
                    Msg = (int)msg.msg,
                    WParam = msg.wParam
                };

                if (!Application.FilterMessage(ref message))
                {
                    Utils.TranslateMessage(ref msg);
                    Utils.DispatchMessage(ref msg);
                }
            }

            swapChain.Present(0, PresentFlags.None);
            window.UpdateInput();
            timer.Update();
        }
    }
}
