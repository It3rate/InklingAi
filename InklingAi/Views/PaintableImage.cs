namespace InklingAi.Views;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Skia;
using InklingAi.CommandEngine;
using InklingAi.Commands;
using InklingAi.ViewModels;
using InkPad.Adapters;
using SkiaSharp;

public class PaintableImage : BaseImage
{
	private InklingModel _model => InklingModel.Instance;
	private Engine _engine => _model.Engine;
	private KeyboardData _keyboardData => _model.KeyboardData;

	public DrawingData DrawingData { get; set; } = new DrawingData();

	public DrawState CurrentDrawState { get; set; } = DrawState.Black;

    ISelectableEntity? _selectedEntity;
    public ISelectableEntity? SelectedEntity
    {
        get => _selectedEntity;
        set
        {
            if (_selectedEntity != value)
            {
                _selectedEntity = value;
                InvalidateVisual();
            }
        }
    }
    private List<SkPolyline> _paths { get; } = new List<SkPolyline>();
    private SkPolyline _curPath;
    private UIRenderUtils _renderUtil = new UIRenderUtils();
    private SelectionHandles _selectionHandles;

	public PaintableImage()
    {
		_canDrag = false;
		_curPath = CreateNewPath();
        _selectionHandles = new SelectionHandles(this);
    }

	public void AddElement(SkPolyline path)
    {
        _paths.Add(path);
        SelectedEntity = path;

		InvalidateVisual();
	}
    public void RemoveLastElement()
    {
        if (_paths.Count > 0)
        {
            _paths.RemoveAt(_paths.Count - 1);
            _curPath.Reset();
			InvalidateVisual();
		}
    }
    public int RemoveElement(ISelectableEntity element)
    {
        int result = -1;
        if (element is SkPolyline path && _paths.Contains(path))
        {
            result = _paths.IndexOf(path);
            _paths.RemoveAt(result);
            InvalidateVisual();
        }
        return result;
    }
    public void AddElementAt(int index, ISelectableEntity element)
    {
        if (index <= _paths.Count && element is SkPolyline path)
        {
            _paths.Insert(index, path);
            InvalidateVisual();
        }
    }

    #region Mouse Events
    protected override void OnMouseDown(Point currentPosition)
	{
        if (DrawingData.DrawMode == DrawMode.Arrow)
        {
            CheckForSelection(currentPosition);
        }
        else
        {
            _curPath.ColorValue = DrawingData.SelColor.ToUInt32();
            _curPath.BrushSize = DrawingData.BrushSize;
            _curPath.Feather = DrawingData.Feather;
            _curPath.Reset();
            _curPath.AddPoint(currentPosition.X, currentPosition.Y);
        }
    }
    protected override void OnMouseMove(Point currentPosition)
    {
        DrawingData.CurrentPosition = currentPosition.ToSKPoint();
        if (DrawingData.DrawMode == DrawMode.Arrow)
        {
        }
        else
        {
            InvalidateVisual();
        }
	}
    protected override void OnMouseDownDrag(Point currentPosition, Point diff)
    {
        if (DrawingData.DrawMode == DrawMode.Arrow)
        {
        }
        else
        {
            _curPath.AddPoint(currentPosition.X, currentPosition.Y);
            if (_curPath.Count > 1)
            {
                InvalidateVisual();
            }
        }
    }
    protected override void OnMouseUp(Point currentPosition, Point diff)
    {
        if (DrawingData.DrawMode == DrawMode.Arrow)
        {
        }
        else
        {
            _curPath.AddPoint(currentPosition.X, currentPosition.Y);
            if (_curPath.Count > 2)
            {
                //_paths.Add(_curPath); // addpath command
                var cmd = new CreateSKPolylineCommand(_curPath, this);
                _engine.Stack.Do(cmd);

                _curPath = CreateNewPath();
                //InvalidateVisual();
            }
            else
            {
                SelectedEntity = null;
            }
        }
    }
    private SkPolyline CreateNewPath()
    {
        return new SkPolyline((int)Bounds.Width, (int)Bounds.Height,
            DrawingData.BrushSize, DrawingData.SelColor.ToUInt32(), DrawingData.Feather);
    }
    //public void OnMouseMove(Panel? sender, MouseEventArgs e)
    //{
    //	_isMouseInside = true;
    //	bool isTransformingPaint = (_isResizeCursor || _isResizeTransparent);
    //	if (isTransformingPaint && InputPb != null)
    //	{
    //		_lastPoint = e.Location.CvPoint();
    //		var dist = _startDownLocation.Y - _lastPoint.Y;
    //		if (_isResizeTransparent)
    //		{
    //			var val = (int)Math.Max(0, Math.Min(255, _startTransparent + dist));
    //			Transparent = val;
    //			OnNumericChanged?.Invoke(PaintState.Transparent, val);
    //		}
    //		else if (_isResizeCursor)
    //		{
    //			NibRadius = (int)Math.Max(_nibMin, Math.Min(_nibMax, _startNibRadius + dist));
    //			OnNumericChanged?.Invoke(PaintState.BrushSize, NibRadius);
    //			_lastPoint = _startDownLocation - (_startDownLocation - _lastPoint);
    //		}
    //	}
    //	else if (_paintState != PaintState.None)
    //	{
    //		switch (_paintState)
    //		{
    //			case PaintState.Dropper:
    //				var color = GetPixelColorAt(sender, e.Location);
    //				SetColor(color);
    //				break;
    //		}
    //	}
    //	else if (CurrentDrawState.CanDraw())
    //	{
    //		ContinueDraw(e.Location.CvPoint());
    //	}
    //	else if (CurrentDrawState == DrawState.Resize && IsPanning)
    //	{
    //		IsPanning = true;
    //		var img = InputPb.InputImage;
    //		if (img != null)
    //		{
    //			var scaledOffset = (_startDownLocation - e.Location.CvPoint()) * (1f / ZoomLevel);
    //			_panOffset = scaledOffset;
    //			//OnZoomPanChanged?.Invoke(this);
    //			InputPb.SizeAndSetImage(img, true);
    //		}
    //	}

    //	if (!IsShiftDown())
    //	{
    //		_isResizeCursor = false;
    //		if (!IsControlDown())
    //		{
    //			_isResizeTransparent = false;
    //		}
    //	}
    //	Invalidate(true); // optimize rect eventually, probably will switch to skia anyway
    //}
    //public void OnMouseUp(MouseEventArgs e)
    //{
    //	if (CurrentDrawState.CanDraw())
    //	{
    //		EndDraw(e.Location.CvPoint());
    //	}
    //	if ((CurrentDrawState == DrawState.Resize) && IsPanning)
    //	{
    //		Pan = _pan + _panOffset;
    //		_panOffset = new Point(0, 0);
    //		ReprocessRequested?.Invoke();
    //	}
    //	IsPanning = false;
    //	_isResizeCursor = false;
    //	_isResizeTransparent = false;
    //}
    //public void OnMouseWheel(MouseEventArgs e)
    //{
    //	if (CurrentDrawState == DrawState.Resize && InputPb != null)
    //	{
    //		var img = InputPb.InputImage;
    //		if (img != null)
    //		{
    //			const int WHEEL_DELTA = 120;
    //			ZoomLevel += (e.Delta / WHEEL_DELTA) * 0.01f;
    //			InputPb.SizeAndSetImage(img, true);

    //			_reprocessTimer.Stop();
    //			_reprocessTimer.Start();
    //		}
    //	}
    //}
    protected override void OnDrop(object? sender, DragEventArgs e) { }
	#endregion

	#region Keyboard Events
	private bool IsShiftDown() => _keyboardData.IsShiftDown;
	private bool IsControlDown() => _keyboardData.IsControlDown;
	private bool IsKeyDown(Key key) => _keyboardData.IsDown(key);
	#endregion

	private void CheckForSelection(Point point)
    {
        var skpt = new SKPoint((float)point.X, (float)point.Y);
        SkPolyline? sel = null;
        foreach (var path in _paths) 
        { 
            if(path.IsPointInside(skpt))
            {
                sel = path;
                break;
            }
        }
        if(sel != SelectedEntity)
        {
            SelectedEntity = sel;
            InvalidateVisual();
        }
    }


    protected override void RenderSkia(SKCanvas canvas)
    {
        for (int i = 0; i < _paths.Count; i++) // Note: foreach can have collection modified issues
        {
            _renderUtil.DrawShape(canvas, _paths[i]);
        }

        if (DrawingData.DrawMode == DrawMode.Arrow)
        {
        }
        else
        {
            if (_curPath != null)
            {
                _renderUtil.DrawShape(canvas, _curPath, true);
            }

            if (IsMouseInside && !IsMouseDown)
            {
                _renderUtil.RenderHoverBrush(canvas, DrawingData);
		    }
        }
        _selectionHandles.RenderSkia(canvas);
    }

}

public enum DrawState
{
    Cursor,
    Resize,
    Black,
    White,
    Erase,
    Filter,
}
public static class DrawStateExtensions
{
    public static bool CanDraw(this DrawState state)
    {
        return (state == DrawState.Black) || (state == DrawState.White) || (state == DrawState.Erase) || (state == DrawState.Filter);
    }
    public static bool IsMask(this DrawState state)
    {
        return (state == DrawState.Black) || (state == DrawState.White);
    }
}