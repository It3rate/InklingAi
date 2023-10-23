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
	bool _firstRunSkip = false;

	public ChangeBrushParamsTask(DrawingData drawingData, float brushSize, float feather, Color selColor, bool isPreviousValues)
	{
		DrawingData = drawingData;
		if (isPreviousValues)
		{
			_firstRunSkip = true;
			_prevBrushSize = brushSize;
			_prevFeather = feather;
			_prevSelColor = selColor;
		}
		else
		{
			_firstRunSkip = false;
			BrushSize = brushSize;
			Feather = feather;
			SelColor = selColor;
		}
	}
	public ChangeBrushParamsTask(DrawingData drawingData, Color oldSelColor)
	{
		_firstRunSkip = true;
		DrawingData = drawingData;
		_prevBrushSize = drawingData.BrushSize;
		_prevFeather = drawingData.Feather;
		_prevSelColor = oldSelColor;
		BrushSize = drawingData.BrushSize;
		Feather = drawingData.Feather;
		SelColor = drawingData.SelColor;
	}
	public ChangeBrushParamsTask(DrawingData drawingData, float oldBrushSize)
	{
		_firstRunSkip = true;
		DrawingData = drawingData;
		_prevBrushSize = oldBrushSize;
		_prevFeather = drawingData.Feather;
		_prevSelColor = drawingData.SelColor;
		BrushSize = drawingData.BrushSize;
		Feather = drawingData.Feather;
		SelColor = drawingData.SelColor;
	}

	public override void RunTask()
	{
		if (!_firstRunSkip)
		{
			//_prevBrushSize = DrawingData.BrushSize;
			//_prevFeather = DrawingData.Feather;
			//_prevSelColor = DrawingData.SelColor;

			DrawingData.BrushSize = BrushSize;
			DrawingData.Feather = Feather;
			DrawingData.SelColor = SelColor;
            DrawingData.Transparency = SelColor.A;
        }
		_firstRunSkip = false;
	}

	public override void UnRunTask()
	{
		DrawingData.BrushSize = _prevBrushSize;
		DrawingData.Feather = _prevFeather;
		DrawingData.SelColor = _prevSelColor;
        DrawingData.Transparency = _prevSelColor.A;
	}

    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        if(newTask is ChangeBrushParamsTask newCbp)
        {
            BrushSize = newCbp.BrushSize;
            Feather = newCbp.Feather;
            SelColor = newCbp.SelColor;
            result = true;
        }
        return result;
    }
}

