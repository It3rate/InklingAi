namespace AIEnv.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public struct IntSize
    {
        public IntSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public float Ratio => Height != 0 ? Width / (float)Height : 0;

        public override bool Equals(object? obj)
        {
            if (obj is Size other)
            {
                return Width == other.Width && Height == other.Height;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }

        public static bool operator ==(IntSize left, IntSize right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntSize left, IntSize right)
        {
            return !(left == right);
        }

        public override string ToString() => $"Width: {Width}, Height: {Height}";

    }




}
