using InklingAi.ViewModels;
using InkPad.CommandEngine;
using InkPad.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InklingAi.Tasks;

public class ChangeBrushParamsTask : TaskBase, ICreateTask
{
	public IDrawingData DrawingData { get; }
	public float BrushSize { get; private set; }
	public float Feather { get; private set; }
	public uint ColorValue { get; private set; }

	private float _prevBrushSize { get; set; }
	private float _prevFeather { get; set; }
	private uint _prevSelColor { get; set; }

	public override bool IsValid => true;
	bool _firstRunSkip = false;

	public ChangeBrushParamsTask(IDrawingData drawingData, float brushSize, float feather, uint selColor, bool isPreviousValues)
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
            ColorValue = selColor;
		}
	}
	public ChangeBrushParamsTask(IDrawingData drawingData, uint oldSelColor)
	{
		_firstRunSkip = true;
		DrawingData = drawingData;
		_prevBrushSize = drawingData.BrushSize;
		_prevFeather = drawingData.Feather;
		_prevSelColor = oldSelColor;
		BrushSize = drawingData.BrushSize;
		Feather = drawingData.Feather;
        ColorValue = drawingData.ColorValue;
	}
	public ChangeBrushParamsTask(IDrawingData drawingData, float oldBrushSize)
	{
		_firstRunSkip = true;
		DrawingData = drawingData;
		_prevBrushSize = oldBrushSize;
		_prevFeather = drawingData.Feather;
		_prevSelColor = drawingData.ColorValue;
		BrushSize = drawingData.BrushSize;
		Feather = drawingData.Feather;
        ColorValue = drawingData.ColorValue;
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
			DrawingData.ColorValue = ColorValue;
            DrawingData.Transparency = ColorValue.Alpha();
        }
		_firstRunSkip = false;
	}

	public override void UnRunTask()
	{
		DrawingData.BrushSize = _prevBrushSize;
		DrawingData.Feather = _prevFeather;
		DrawingData.ColorValue = _prevSelColor;
        DrawingData.Transparency = _prevSelColor.Alpha();
	}

    public override bool MergeDataFrom(TaskBase newTask)
    {
        var result = false;
        if(newTask is ChangeBrushParamsTask newCbp)
        {
            BrushSize = newCbp.BrushSize;
            Feather = newCbp.Feather;
            ColorValue = newCbp.ColorValue;
            result = true;
        }
        return result;
    }
}

