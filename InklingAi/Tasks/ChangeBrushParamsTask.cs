using Avalonia.Media;
using InklingAi.ViewModels;
using InklingAi.Views;
using InkPad.Adapters;
using InkPad.CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InklingAi.Tasks;

public class ChangeBrushParamsTask : TaskBase, ICreateTask
{
	public DrawingData DrawingData { get; }
	public float BrushSize { get; private set; }
	public float Feather { get; private set; }
	public Color SelColor { get; private set; }

	private float _prevBrushSize { get; set; }
	private float _prevFeather { get; set; }
	private Color _prevSelColor { get; set; }

	public override bool IsValid => true;

	public ChangeBrushParamsTask(DrawingData drawingData, float brushSize, float feather, Color selColor)
	{
		DrawingData = drawingData;
		BrushSize = brushSize;
		Feather = feather;
		SelColor = selColor;
	}

	public override void RunTask()
	{
		_prevBrushSize = DrawingData.BrushSize;
		_prevFeather = DrawingData.Feather;
		_prevSelColor = DrawingData.SelColor;

		DrawingData.BrushSize = BrushSize;
		DrawingData.Feather = Feather;
		DrawingData.SelColor = SelColor;
	}

	public override void UnRunTask()
	{
		DrawingData.BrushSize = _prevBrushSize;
		DrawingData.Feather = _prevFeather;
		DrawingData.SelColor = _prevSelColor;
	}
}

