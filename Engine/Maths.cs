using System;

namespace Engine
{
    public struct Vertex
    {
        public Vertex(float px, float py, float pz, float nx, float ny, float nz, float u, float v)
        {
            this.px = px;
            this.py = py;
            this.pz = pz;
            this.nx = nx;
            this.ny = ny;
            this.nz = nz;
            this.u = u;
            this.v = v;
        }
        public Vertex(Vector3 p, Vector3 n, Vector3 t) : this(p.X, p.Y, p.Z, n.X, n.Y, n.Z, t.X, t.Y)
        {

        }

        private float px, py, pz;
        private float nx, ny, nz;
        private float u, v;
    }

    public struct Vertex1
    {
        public static Vertex1[] Quad =
        {
            new Vertex1( 1,  1,  1,  0),
            new Vertex1( 1, -1,  1,  1),
            new Vertex1(-1,  1,  0,  0),
            new Vertex1(-1, -1,  0,  1),
        };

        public Vertex1(float x, float y, float z, float w, float u, float v)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
            this.u = u;
            this.v = v;
        }

        public Vertex1(float x, float y, float u, float v) : this(x, y, 1, 1, u, v)
        {
            this.x = x;
            this.y = y;
            this.u = u;
            this.v = v;
        }

        private float x, y, z, w;
        private float u, v;
    }

    public struct Color : IArray
    {
        public static readonly Color Black = new Color(0, 0, 0, 0);

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        public Color(float r, float g, float b) : this(r, g, b, 1)
        {

        }

        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public static Color operator +(Color p, Color q)
        {
            return new Color(p.R + q.R, p.G + q.G, p.B + q.B, p.A + q.A);
        }
        public static Color operator *(float p, Color q)
        {
            return new Color(p * q.R, p * q.G, p * q.B, p * q.A);
        }
        public static Color operator -(Color p)
        {
            return -1 * p;
        }
        public static Color operator -(Color p, Color q)
        {
            return p + -q;
        }
        public static Color operator /(Color p, float q)
        {
            return 1 / q * p;
        }

        int IArray.ByteSize
        {
            get => 16;
        }
        Array IArray.Data
        {
            get => new float[] { R, G, B, A };
        }

        public override string ToString()
        {
            return $"rgba: {R}, {G}, {B}, {A}";
        }
    }

    public struct Vector3 : IArray
    {
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3(float thera, float phi)
        {
            X = (float)(Math.Sin(thera) * Math.Cos(phi));
            Y = (float)(Math.Sin(thera) * Math.Sin(phi));
            Z = (float)Math.Cos(thera);
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float Max => Math.Max(Math.Max(X, Y), Z);
        public float Length => (float)Math.Sqrt(this | this);
        public Vector3 Normalize() => this / Length;

        public static float operator |(Vector3 p, Vector3 q)
        {
            return p.X * q.X + p.Y * q.Y + p.Z * q.Z;
        }
        public static Vector3 operator ^(Vector3 p, Vector3 q)
        {
            return new Vector3(p.Y * q.Z - p.Z * q.Y, p.Z * q.X - p.X * q.Z, p.X * q.Y - p.Y * q.X);
        }

        public static Vector3 operator -(Vector3 p)
        {
            return new Vector3(-p.X, -p.Y, -p.Z);
        }
        public static Vector3 operator +(Vector3 p, Vector3 q)
        {
            return new Vector3(p.X + q.X, p.Y + q.Y, p.Z + q.Z);
        }
        public static Vector3 operator -(Vector3 p, Vector3 q)
        {
            return new Vector3(p.X - q.X, p.Y - q.Y, p.Z - q.Z);
        }
        public static Vector3 operator *(float p, Vector3 q)
        {
            return new Vector3(p * q.X, p * q.Y, p * q.Z);
        }
        public static Vector3 operator /(Vector3 p, float q)
        {
            return new Vector3(p.X / q, p.Y / q, p.Z / q);
        }

        int IArray.ByteSize
        {
            get => 12;
        }
        Array IArray.Data
        {
            get => new float[] { X, Y, Z };
        }

        public override string ToString() => $"{X}, {Y}, {Z}";
    }

    public struct Vector2 : IArray
    {
        public static readonly Vector2 Zero = new Vector2(0, 0);

        public Vector2(float x, float y, float z)
        {
            V = (float)Math.Acos(z);
            U = (float)Math.Atan2(y, x);
        }
        public Vector2(float u, float v)
        {
            U = u;
            V = v;
        }

        public float U { get; set; }
        public float V { get; set; }

        public static Vector2 operator -(Vector2 p)
        {
            return new Vector2(-p.U, -p.V);
        }
        public static Vector2 operator +(Vector2 p, Vector2 q)
        {
            return new Vector2(p.U + q.U, p.V + q.V);
        }
        public static Vector2 operator -(Vector2 p, Vector2 q)
        {
            return new Vector2(p.U - q.U, p.V - q.V);
        }
        public static Vector2 operator *(float p, Vector2 q)
        {
            return new Vector2(p * q.U, p * q.V);
        }
        public static Vector2 operator /(Vector2 p, float q)
        {
            return new Vector2(p.U / q, p.V / q);
        }

        int IArray.ByteSize
        {
            get => 8;
        }
        Array IArray.Data
        {
            get => new float[] { U, V };
        }

        public override string ToString() => $"{U}, {V}";
    }

    public struct Point : IArray
    {
        public static readonly Point Zero = new Point(0, 0, 0);

        public const byte length = 24; // 16 777 216
        public const long size = 1L << length;
        public const float sizeInv = 1f / size;

        public Point(long x, long y, long z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Point(float x, float y, float z)
        {
            X = (long)(x * size);
            Y = (long)(y * size);
            Z = (long)(z * size);
        }

        public long X { get; set; }
        public long Y { get; set; }
        public long Z { get; set; }

        public static Vector3 operator -(Point p, Point q)
        {
            return new Vector3((p.X - q.X) * sizeInv, (p.Y - q.Y) * sizeInv, (p.Z - q.Z) * sizeInv);
        }
        public static Point operator +(Point p, Vector3 q)
        {
            var x = p.X + (long)(q.X * size);
            var y = p.Y + (long)(q.Y * size);
            var z = p.Z + (long)(q.Z * size);

            return new Point(x, y, z);
        }
        public static Point operator -(Point p, Vector3 q)
        {
            var x = p.X - (long)(q.X * size);
            var y = p.Y - (long)(q.Y * size);
            var z = p.Z - (long)(q.Z * size);

            return new Point(x, y, z);
        }

        int IArray.ByteSize
        {
            get => 24;
        }
        Array IArray.Data
        {
            get => new long[] { X, Y, Z };
        }

        public override string ToString()
        {
            var result = string.Empty;

            result += $"{X >> length} + {(X & (size - 1)) * sizeInv}, ";
            result += $"{Y >> length} + {(Y & (size - 1)) * sizeInv}, ";
            result += $"{Z >> length} + {(Z & (size - 1)) * sizeInv}";

            return result;
        }
    }

    public struct Matrix : IArray
    {
        public static readonly Matrix Identity = new Matrix() { m00 = 1, m11 = 1, m22 = 1, m33 = 1 };
        public static readonly Matrix Zero = new Matrix();

        private float m00, m01, m02, m03;
        private float m10, m11, m12, m13;
        private float m20, m21, m22, m23;
        private float m30, m31, m32, m33;

        public Vector3 X
        {
            get => new Vector3(m00, m10, m20);
            set
            {
                m00 = value.X;
                m10 = value.Y;
                m20 = value.Z;
            }
        }
        public Vector3 Y
        {
            get => new Vector3(m01, m11, m21);
            set
            {
                m01 = value.X;
                m11 = value.Y;
                m21 = value.Z;
            }
        }
        public Vector3 Z
        {
            get => new Vector3(m02, m12, m22);
            set
            {
                m02 = value.X;
                m12 = value.Y;
                m22 = value.Z;
            }
        }
        public Vector3 T
        {
            get => new Vector3(m03, m13, m23);
            set
            {
                m03 = value.X;
                m13 = value.Y;
                m23 = value.Z;
            }
        }
        public Vector3 P
        {
            get => new Vector3(m30, m31, m32);
            set
            {
                m30 = value.X;
                m31 = value.Y;
                m32 = value.Z;
            }
        }
        public float W
        {
            get => m33;
            set => m33 = value;
        }

        public static Matrix CreateView(Point position, Vector3 offset, Point target, Vector3 up)
        {
            return CreateView(position + offset, target, up);
        }
        public static Matrix CreateView(Point position, Point target, Vector3 up)
        {
            return CreateView((position - target).Normalize(), up.Normalize());
        }
        public static Matrix CreateView(Vector3 direction, Vector3 up)
        {
            return CreateView(Vector3.Zero, direction, up);
        }
        public static Matrix CreateView(Vector3 offset, Vector3 direction, Vector3 up)
        {
            var z = direction;
            var x = up ^ z;
            var y = z ^ x;

            var result = new Matrix
            {
                m00 = x.X,
                m01 = x.Y,
                m02 = x.Z,
                m03 = -(x | offset),
                m10 = y.X,
                m11 = y.Y,
                m12 = y.Z,
                m13 = -(y | offset),
                m20 = z.X,
                m21 = z.Y,
                m22 = z.Z,
                m23 = -(z | offset),
                m33 = 1
            };

            return result;
        }

        public static Matrix CreateProjection(float fov, float aspectInv, float near, float far)
        {
            fov *= 0.5f;

            var x = 1 / (float)Math.Tan(fov);
            var y = x * aspectInv;

            var result = new Matrix
            {
                m00 = y,
                m11 = x,
                m22 = (far + near) / (far - near),
                m32 = 1,
                m23 = 2 * near * far / (near - far)
            };

            return result;
        }
        public static Matrix CreateProjection(float fov, float aspectInv)
        {
            fov *= 0.5f;

            var x = 1 / (float)Math.Tan(fov);
            var y = x * aspectInv;

            var result = new Matrix
            {
                m00 = y,
                m11 = x,
                m22 = 1,
                m32 = 1,
            };

            return result;
        }

        public static Matrix CreateRotation(Vector3 velocity)
        {
            var angle = velocity.Length;

            if (angle == 0) return Matrix.Identity;

            return CreateRotation(velocity / angle, angle);
        }
        public static Matrix CreateRotation(Vector3 axis, float angle)
        {
            var x = axis.X;
            var y = axis.Y;
            var z = axis.Z;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);
            var x2 = x * x;
            var y2 = y * y;
            var z2 = z * z;
            var xy = x * y;
            var xz = x * z;
            var yz = y * z;

            var result = new Matrix
            {
                m00 = x2 + (cos * (1 - x2)),
                m01 = xy - (cos * xy) + (sin * z),
                m02 = xz - (cos * xz) - (sin * y),
                m10 = xy - (cos * xy) - (sin * z),
                m11 = y2 + (cos * (1 - y2)),
                m12 = yz - (cos * yz) + (sin * x),
                m20 = xz - (cos * xz) + (sin * y),
                m21 = yz - (cos * yz) - (sin * x),
                m22 = z2 + (cos * (1 - z2)),

                m33 = 1
            };

            return result;
        }
        public static Matrix CreateRotationX(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);

            var result = new Matrix
            {
                m11 = cos,
                m12 = sin,
                m21 = -sin,
                m22 = cos,
                m00 = 1,
                m33 = 1
            };

            return result;
        }
        public static Matrix CreateRotationY(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);

            var result = new Matrix
            {
                m00 = cos,
                m02 = sin,
                m20 = -sin,
                m22 = cos,
                m11 = 1,
                m33 = 1
            };

            return result;
        }
        public static Matrix CreateRotationZ(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);

            var result = new Matrix
            {
                m00 = cos,
                m01 = sin,
                m10 = -sin,
                m11 = cos,
                m22 = 1,
                m33 = 1
            };

            return result;
        }

        public static Matrix CreateScale(Vector3 s)
        {
            var result = new Matrix
            {
                m00 = s.X,
                m11 = s.Y,
                m22 = s.Z,
                m33 = 1
            };

            return result;
        }
        public static Matrix CreateScale(float x, float y, float z)
        {
            var result = new Matrix
            {
                m00 = x,
                m11 = y,
                m22 = z,
                m33 = 1
            };

            return result;
        }

        public static Matrix operator ~(Matrix p)
        {
            var result = new Matrix
            {
                m00 = p.m00,
                m01 = p.m10,
                m02 = p.m20,
                m03 = p.m30,
                m10 = p.m01,
                m11 = p.m11,
                m12 = p.m21,
                m13 = p.m31,
                m20 = p.m02,
                m21 = p.m12,
                m22 = p.m22,
                m23 = p.m32,
                m30 = p.m03,
                m31 = p.m13,
                m32 = p.m23,
                m33 = p.m33,
            };

            return result;
        }
        public static Matrix operator -(Matrix p)
        {
            return new Matrix()
            {
                m00 = -p.m00,
                m01 = -p.m01,
                m02 = -p.m02,
                m03 = -p.m03,
                m10 = -p.m10,
                m11 = -p.m11,
                m12 = -p.m12,
                m13 = -p.m13,
                m20 = -p.m20,
                m21 = -p.m21,
                m22 = -p.m22,
                m23 = -p.m23,
                m30 = -p.m30,
                m31 = -p.m31,
                m32 = -p.m32,
                m33 = -p.m33,
            };
        }
        public static Matrix operator +(Matrix p, Matrix q)
        {
            return new Matrix()
            {
                m00 = p.m00 + q.m00,
                m01 = p.m01 + q.m01,
                m02 = p.m02 + q.m02,
                m03 = p.m03 + q.m03,
                m10 = p.m10 + q.m10,
                m11 = p.m11 + q.m11,
                m12 = p.m12 + q.m12,
                m13 = p.m13 + q.m13,
                m20 = p.m20 + q.m20,
                m21 = p.m21 + q.m21,
                m22 = p.m22 + q.m22,
                m23 = p.m23 + q.m23,
                m30 = p.m30 + q.m30,
                m31 = p.m31 + q.m31,
                m32 = p.m32 + q.m32,
                m33 = p.m33 + q.m33,
            };
        }
        public static Matrix operator -(Matrix p, Matrix q)
        {
            return new Matrix()
            {
                m00 = p.m00 - q.m00,
                m01 = p.m01 - q.m01,
                m02 = p.m02 - q.m02,
                m03 = p.m03 - q.m03,
                m10 = p.m10 - q.m10,
                m11 = p.m11 - q.m11,
                m12 = p.m12 - q.m12,
                m13 = p.m13 - q.m13,
                m20 = p.m20 - q.m20,
                m21 = p.m21 - q.m21,
                m22 = p.m22 - q.m22,
                m23 = p.m23 - q.m23,
                m30 = p.m30 - q.m30,
                m31 = p.m31 - q.m31,
                m32 = p.m32 - q.m32,
                m33 = p.m33 - q.m33,
            };
        }
        public static Matrix operator *(float p, Matrix q)
        {
            return new Matrix()
            {
                m00 = p * q.m00,
                m01 = p * q.m01,
                m02 = p * q.m02,
                m03 = p * q.m03,
                m10 = p * q.m10,
                m11 = p * q.m11,
                m12 = p * q.m12,
                m13 = p * q.m13,
                m20 = p * q.m20,
                m21 = p * q.m21,
                m22 = p * q.m22,
                m23 = p * q.m23,
                m30 = p * q.m30,
                m31 = p * q.m31,
                m32 = p * q.m32,
                m33 = p * q.m33,
            };
        }
        public static Matrix operator /(Matrix p, float q)
        {
            return 1 / q * p;
        }
        public static Matrix operator *(Matrix p, Matrix q)
        {
            return new Matrix()
            {
                m00 = p.m00 * q.m00 + p.m01 * q.m10 + p.m02 * q.m20 + p.m03 * q.m30,
                m01 = p.m00 * q.m01 + p.m01 * q.m11 + p.m02 * q.m21 + p.m03 * q.m31,
                m02 = p.m00 * q.m02 + p.m01 * q.m12 + p.m02 * q.m22 + p.m03 * q.m32,
                m03 = p.m00 * q.m03 + p.m01 * q.m13 + p.m02 * q.m23 + p.m03 * q.m33,

                m10 = p.m10 * q.m00 + p.m11 * q.m10 + p.m12 * q.m20 + p.m13 * q.m30,
                m11 = p.m10 * q.m01 + p.m11 * q.m11 + p.m12 * q.m21 + p.m13 * q.m31,
                m12 = p.m10 * q.m02 + p.m11 * q.m12 + p.m12 * q.m22 + p.m13 * q.m32,
                m13 = p.m10 * q.m03 + p.m11 * q.m13 + p.m12 * q.m23 + p.m13 * q.m33,

                m20 = p.m20 * q.m00 + p.m21 * q.m10 + p.m22 * q.m20 + p.m23 * q.m30,
                m21 = p.m20 * q.m01 + p.m21 * q.m11 + p.m22 * q.m21 + p.m23 * q.m31,
                m22 = p.m20 * q.m02 + p.m21 * q.m12 + p.m22 * q.m22 + p.m23 * q.m32,
                m23 = p.m20 * q.m03 + p.m21 * q.m13 + p.m22 * q.m23 + p.m23 * q.m33,

                m30 = p.m30 * q.m00 + p.m31 * q.m10 + p.m32 * q.m20 + p.m33 * q.m30,
                m31 = p.m30 * q.m01 + p.m31 * q.m11 + p.m32 * q.m21 + p.m33 * q.m31,
                m32 = p.m30 * q.m02 + p.m31 * q.m12 + p.m32 * q.m22 + p.m33 * q.m32,
                m33 = p.m30 * q.m03 + p.m31 * q.m13 + p.m32 * q.m23 + p.m33 * q.m33,
            };
        }

        public static Vector3 operator *(Matrix p, Vector3 q)
        {
            var x = (p.X | q) + p.m03;
            var y = (p.Y | q) + p.m13;
            var z = (p.Z | q) + p.m23;

            return new Vector3(x, y, z);
        }
        public static Vector3 operator &(Matrix p, Vector3 q)
        {
            var x = (p.X | q);
            var y = (p.Y | q);
            var z = (p.Z | q);

            return new Vector3(x, y, z);
        }

        int IArray.ByteSize
        {
            get => 64;
        }
        Array IArray.Data
        {
            get => new float[]
            {
                m00, m10, m20, m30,
                m01, m11, m21, m31,
                m02, m12, m22, m32,
                m03, m13, m23, m33,
            };
        }

        public override string ToString()
        {
            return $"{m00}, {m01}, {m02}, {m03} | {m10}, {m11}, {m12}, {m13} | {m20}, {m21}, {m22}, {m23} | {m30}, {m31}, {m32}, {m33}";
        }
    }

    public struct Matrix3x3 : IArray
    {
        public static readonly Matrix3x3 Identity = new Matrix3x3() { m00 = 1, m11 = 1, m22 = 1 };
        public static readonly Matrix3x3 Zero = new Matrix3x3();

        private float m00, m01, m02;
        private float m10, m11, m12;
        private float m20, m21, m22;

        public Vector3 X
        {
            get => new Vector3(m00, m10, m20);
            set
            {
                m00 = value.X;
                m10 = value.Y;
                m20 = value.Z;
            }
        }
        public Vector3 Y
        {
            get => new Vector3(m01, m11, m21);
            set
            {
                m01 = value.X;
                m11 = value.Y;
                m21 = value.Z;
            }
        }
        public Vector3 Z
        {
            get => new Vector3(m02, m12, m22);
            set
            {
                m02 = value.X;
                m12 = value.Y;
                m22 = value.Z;
            }
        }

        public static Matrix3x3 CreateRotation(Vector3 velocity)
        {
            var angle = velocity.Length;

            if (angle == 0) return Identity;

            return CreateRotation(velocity / angle, angle);
        }
        public static Matrix3x3 CreateRotation(Vector3 axis, float angle)
        {
            var x = axis.X;
            var y = axis.Y;
            var z = axis.Z;
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);
            var x2 = x * x;
            var y2 = y * y;
            var z2 = z * z;
            var xy = x * y;
            var xz = x * z;
            var yz = y * z;

            var result = new Matrix3x3
            {
                m00 = x2 + (cos * (1 - x2)),
                m01 = xy - (cos * xy) + (sin * z),
                m02 = xz - (cos * xz) - (sin * y),
                m10 = xy - (cos * xy) - (sin * z),
                m11 = y2 + (cos * (1 - y2)),
                m12 = yz - (cos * yz) + (sin * x),
                m20 = xz - (cos * xz) + (sin * y),
                m21 = yz - (cos * yz) - (sin * x),
                m22 = z2 + (cos * (1 - z2)),
            };

            return result;
        }
        public static Matrix3x3 CreateRotationX(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);

            var result = new Matrix3x3
            {
                m11 = cos,
                m12 = sin,
                m21 = -sin,
                m22 = cos,
                m00 = 1,
            };

            return result;
        }
        public static Matrix3x3 CreateRotationY(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);

            var result = new Matrix3x3
            {
                m00 = cos,
                m02 = sin,
                m20 = -sin,
                m22 = cos,
                m11 = 1,
            };

            return result;
        }
        public static Matrix3x3 CreateRotationZ(float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);

            var result = new Matrix3x3
            {
                m00 = cos,
                m01 = sin,
                m10 = -sin,
                m11 = cos,
                m22 = 1,
            };

            return result;
        }

        public static Matrix3x3 CreateScale(Vector3 s)
        {
            var result = new Matrix3x3
            {
                m00 = s.X,
                m11 = s.Y,
                m22 = s.Z,
            };

            return result;
        }
        public static Matrix3x3 CreateScale(float x, float y, float z)
        {
            var result = new Matrix3x3
            {
                m00 = x,
                m11 = y,
                m22 = z,
            };

            return result;
        }

        public static Matrix3x3 operator ~(Matrix3x3 p)
        {
            var result = new Matrix3x3
            {
                m00 = p.m00,
                m01 = p.m10,
                m02 = p.m20,
                m10 = p.m01,
                m11 = p.m11,
                m12 = p.m21,
                m20 = p.m02,
                m21 = p.m12,
                m22 = p.m22,
            };

            return result;
        }
        public static Matrix3x3 operator -(Matrix3x3 p)
        {
            return new Matrix3x3()
            {
                m00 = -p.m00,
                m01 = -p.m01,
                m02 = -p.m02,
                m10 = -p.m10,
                m11 = -p.m11,
                m12 = -p.m12,
                m20 = -p.m20,
                m21 = -p.m21,
                m22 = -p.m22,
            };
        }
        public static Matrix3x3 operator +(Matrix3x3 p, Matrix3x3 q)
        {
            return new Matrix3x3()
            {
                m00 = p.m00 + q.m00,
                m01 = p.m01 + q.m01,
                m02 = p.m02 + q.m02,
                m10 = p.m10 + q.m10,
                m11 = p.m11 + q.m11,
                m12 = p.m12 + q.m12,
                m20 = p.m20 + q.m20,
                m21 = p.m21 + q.m21,
                m22 = p.m22 + q.m22,
            };
        }
        public static Matrix3x3 operator -(Matrix3x3 p, Matrix3x3 q)
        {
            return new Matrix3x3()
            {
                m00 = p.m00 - q.m00,
                m01 = p.m01 - q.m01,
                m02 = p.m02 - q.m02,
                m10 = p.m10 - q.m10,
                m11 = p.m11 - q.m11,
                m12 = p.m12 - q.m12,
                m20 = p.m20 - q.m20,
                m21 = p.m21 - q.m21,
                m22 = p.m22 - q.m22,
            };
        }
        public static Matrix3x3 operator *(float p, Matrix3x3 q)
        {
            return new Matrix3x3()
            {
                m00 = p * q.m00,
                m01 = p * q.m01,
                m02 = p * q.m02,
                m10 = p * q.m10,
                m11 = p * q.m11,
                m12 = p * q.m12,
                m20 = p * q.m20,
                m21 = p * q.m21,
                m22 = p * q.m22,
            };
        }
        public static Matrix3x3 operator /(Matrix3x3 p, float q)
        {
            return 1 / q * p;
        }
        public static Matrix3x3 operator *(Matrix3x3 p, Matrix3x3 q)
        {
            return new Matrix3x3()
            {
                m00 = p.m00 * q.m00 + p.m01 * q.m10 + p.m02 * q.m20,
                m01 = p.m00 * q.m01 + p.m01 * q.m11 + p.m02 * q.m21,
                m02 = p.m00 * q.m02 + p.m01 * q.m12 + p.m02 * q.m22,

                m10 = p.m10 * q.m00 + p.m11 * q.m10 + p.m12 * q.m20,
                m11 = p.m10 * q.m01 + p.m11 * q.m11 + p.m12 * q.m21,
                m12 = p.m10 * q.m02 + p.m11 * q.m12 + p.m12 * q.m22,

                m20 = p.m20 * q.m00 + p.m21 * q.m10 + p.m22 * q.m20,
                m21 = p.m20 * q.m01 + p.m21 * q.m11 + p.m22 * q.m21,
                m22 = p.m20 * q.m02 + p.m21 * q.m12 + p.m22 * q.m22,
            };
        }

        public static Vector3 operator *(Matrix3x3 p, Vector3 q)
        {
            var x = (p.X | q);
            var y = (p.Y | q);
            var z = (p.Z | q);

            return new Vector3(x, y, z);
        }

        int IArray.ByteSize
        {
            get => 36;
        }
        Array IArray.Data
        {
            get => new float[]
            {
                m00, m10, m20,
                m01, m11, m21,
                m02, m12, m22,
            };
        }

        public static implicit operator Matrix(Matrix3x3 matrix)
        {
            return new Matrix
            {
                X = matrix.X,
                Y = matrix.Y,
                Z = matrix.Z,
                W = 1
            };
        }
    }
}
