using Avalonia.Controls;
using InklingAi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InklingAi.Views
{
	public class InklingColorPickerWindow : AvaloniaColorPicker.ColorPickerWindow
	{  
		public InklingColorPickerWindow() : base() => SetProperties();
		public InklingColorPickerWindow(Avalonia.Media.Color? previousColour) : base(previousColour) => SetProperties();

		private void SetProperties()
		{
			IsAlphaVisible = false;
			IsColourBlindnessSelectorVisible = false;
			IsHexVisible = false;
			IsPaletteVisible = false;
			IsColourSpaceSelectorVisible = true;

			IsCIELABVisible = false;
			IsCIELABSelectable = false;

			Icon = new WindowIcon(new Avalonia.Media.Imaging.Bitmap("Assets/colorPicker.ico"));
			//IsHSBVisible = true;
			//IsHSBSelectable = true;
			//IsRGBVisible = true;
			//IsRGBSelectable = true;
			//ColorSpace = AvaloniaColorPicker.ColorPicker.ColorSpaces.RGB;
			//Color = DrawingData.Instance.SelColor;
		}


	}
}
