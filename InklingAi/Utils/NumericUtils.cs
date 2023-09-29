namespace InklingAi.Utils
{
    using System;
    using System.Collections.Generic;
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

        public override string ToString() => $"Width: {Width}, Height: {Height}";

        public static explicit operator IntSize(Avalonia.Size size)
        {
            return new IntSize((int)size.Width, (int)size.Height);
        }

        public static explicit operator Avalonia.Size(IntSize intSize)
        {
            return new Avalonia.Size(intSize.Width, intSize.Height);
        }
    }




}
