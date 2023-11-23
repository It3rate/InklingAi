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
	public IBrushState BrushState { get; }
	public float BrushSize { get; private set; }
	public float Feather { get; private set; }
	public uint ColorValue { get; private set; }

	private float _prevBrushSize { get; set; }
	private float _prevFeather { get; set; }
	private uint _prevSelColor { get; set; }

	public override bool IsValid => true;
	bool _firstRunSkip = false;

	public ChangeBrushParamsTask(IBrushState brushState, float brushSize, float feather, uint selColor, bool isPreviousValues)
	{
		BrushState = brushState;
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
	public ChangeBrushParamsTask(IBrushState brushState, uint oldSelColor)
	{
		_firstRunSkip = true;
		BrushState = brushState;
		_prevBrushSize = brushState.BrushSize;
		_prevFeather = brushState.Feather;
		_prevSelColor = oldSelColor;
		BrushSize = brushState.BrushSize;
		Feather = brushState.Feather;
        ColorValue = brushState.ColorValue;
	}
	public ChangeBrushParamsTask(IBrushState brushState, float oldBrushSize)
	{
		_firstRunSkip = true;
		BrushState = brushState;
		_prevBrushSize = oldBrushSize;
		_prevFeather = brushState.Feather;
		_prevSelColor = brushState.ColorValue;
		BrushSize = brushState.BrushSize;
		Feather = brushState.Feather;
        ColorValue = brushState.ColorValue;
	}

	public override void RunTask()
	{
		if (!_firstRunSkip)
		{
			BrushState.BrushSize = BrushSize;
			BrushState.Feather = Feather;
			BrushState.ColorValue = ColorValue;
            BrushState.Transparency = ColorValue.Alpha();
        }
		_firstRunSkip = false;
	}

	public override void UnRunTask()
	{
		BrushState.BrushSize = _prevBrushSize;
		BrushState.Feather = _prevFeather;
		BrushState.ColorValue = _prevSelColor;
        BrushState.Transparency = _prevSelColor.Alpha();
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

