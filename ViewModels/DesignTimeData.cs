namespace InklingAi.ViewModels
{
    using Avalonia.Controls;
    using Avalonia.Svg.Skia;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DesignTimeInputItem : InputItem
    {
        public DesignTimeInputItem() : base("scratch0", "Scratch 1", @"C:\Users\Robin\InklingAI\SD\working\")
        {
        }
    }
}
