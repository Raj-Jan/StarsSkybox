using SharpDX.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Engine
{
    public interface IArray
    {
        int ByteSize { get; }
        Array Data { get; }
    }

    public interface ITime
    {
        uint Frame { get; }
        float Elapsed { get; }
        float Total { get; }
    }

    public interface ITarget
    {
        void Apply();
        void Clear();
    }

    public class Timer : ITime
    {
        public Timer()
        {
            stopwatch = new Stopwatch();
        }

        private Stopwatch stopwatch;

        public uint Frame { get; set; }
        public float Elapsed { get; set; }
        public float Total { get; set; }

        public void Start()
        {
            stopwatch.Start();
        }
        public void Stop()
        {
            stopwatch.Stop();
        }
        public void Reset()
        {
            stopwatch.Reset();
        }

        public void Update()
        {
            var current = (float)stopwatch.Elapsed.TotalSeconds;
            Elapsed = current - Total;
            Total = current;
            Frame++;
        }
    }

    public abstract class Disposable : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed) return;
            Dispose(true);
            disposed = true;
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }

    public static class Utils
    {
        public const float PI = (float)Math.PI;
        public const float _120D = (float)(Math.PI / 1.5);
        public const float _90D = (float)(Math.PI / 2);
        public const float _60D = (float)(Math.PI / 3);
        public const float _4D = (float)(Math.PI / 45);

        public static void Dispose(this IDisposable[] disposables)
        {
            foreach (var disposable in disposables)
                disposable?.Dispose();
        }
        public static void Dispose<T>(this List<T> disposables) where T : IDisposable
        {
            foreach (var disposable in disposables)
                disposable?.Dispose();
        }

        public static void Clear<T>(this T[] array)
        {
            if (array.Length < 77)
            {
                for (int i = 0; i < array.Length; i++)
                    array[i] = default;
            }
            else Array.Clear(array, 0, array.Length);
        }

        public static void Insert(this byte[] dst, int value, int offset)
        {
            var src = new int[] { value };
            Buffer.BlockCopy(src, 0, dst, offset, 4);
        }
        public static void Insert(this byte[] dst, float value, int offset)
        {
            var src = new float[] { value };
            Buffer.BlockCopy(src, 0, dst, offset, 4);
        }
        public static void Insert(this byte[] dst, IArray data, int offset)
        {
            var src = data.Data;
            var size = data.ByteSize;

            Buffer.BlockCopy(src, 0, dst, offset, size);
        }
        public static void Insert(this byte[] dst, Array src, int size, int offset)
        {
            Buffer.BlockCopy(src, 0, dst, offset, size);
        }

        [DllImport("user32.dll")]
        internal static extern int PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgMin, int wMsgMax, int wRemoveMsg);
        [DllImport("user32.dll")]
        internal static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgMin, int wMsgMax);
        [DllImport("user32.dll")]
        internal static extern void TranslateMessage(ref NativeMessage msg);
        [DllImport("user32.dll")]
        internal static extern void DispatchMessage(ref NativeMessage msg);
    }
}
