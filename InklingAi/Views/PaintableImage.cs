namespace InklingAi.Views;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Skia;
using InklingAi.CommandEngine;
using InklingAi.Commands;
using InklingAi.ViewModels;
using InkPad.Adapters;
using InkPad.Utils;
using NumSharp.Utilities;
using SkiaSharp;

public class PaintableImage : BaseImage
{
	private InklingModel _model => InklingModel.Instance;
	private Engine _engine => _model.Engine;
	private KeyboardData _keyboardData => _model.KeyboardData;

	public DrawingData DrawingData { get; set; } = new DrawingData();

	public DrawState CurrentDrawState { get; set; } = DrawState.Black;

    public Selection Selection { get; }
    private List<ISelectableElement> _paths { get; } = new List<ISelectableElement>();
    private SkPolyline _curPath;
    private UIRenderUtils _renderUtil = new UIRenderUtils();
    private SelectionHandles _selectionHandles;

	public PaintableImage()
    {
        _engine.FocusedCommandTarget = this; // todo: there will be multiple instances of this eventually

        Selection = new Selection(this);
		_canDrag = false;
		_curPath = CreateNewPath();
        _selectionHandles = new SelectionHandles(this);
    }

    public int IndexOfElement(ISelectableElement element)
    {
        return _paths.IndexOf(element);
    }
    public void AddElement(ISelectableElement path)
    {
        _paths.Add(path);
        Selection.Set(path);

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
    public int RemoveElement(ISelectableElement element)
    {
        int result = -1;
        if (_paths.Contains(element))
        {
            result = _paths.IndexOf(element);
            _paths.RemoveAt(result);
            InvalidateVisual();
        }
        return result;
    }
    public void AddElementAt(int index, ISelectableElement element)
    {
        if (index <= _paths.Count)
        {
            _paths.Insert(index, element);
            InvalidateVisual();
        }
    }



	#region Mouse Events
	private bool _hasDragged = false;
	private bool _choseExistingSelection = false;
	protected override void OnMouseDown(Point currentPosition)
	{
        if (DrawingData.DrawMode == DrawMode.Arrow)
        {
            if (CheckHandlesHit(currentPosition)){}
            else if(CheckSelectionHit(currentPosition)){}
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
		_hasDragged = true;
		if (DrawingData.DrawMode == DrawMode.Arrow)
        {
			if (_choseExistingSelection)
			{
				// move selection
				Move((float)diff.X, (float)diff.Y);
			}
			else if (_selectionHandles.IsHandlePressed)
			{
				// scale or rotate
			}
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
			if (_choseExistingSelection && !_hasDragged)
			{
				var tk = Selection.TransformKind;
				Selection.TransformKind = (tk == TransformKind.Scale) ?
					TransformKind.Rotate : TransformKind.Scale;
				InvalidateVisual();
			}
		}
        else
        {
            _curPath.AddPoint(currentPosition.X, currentPosition.Y);
            if (_curPath.Count > 2)
            {
                var cmd = new CreateSKPolylineCommand(_curPath, this);
                _engine.Stack.Do(cmd);
                _curPath = CreateNewPath();
            }
            else
            {
                Selection.Clear();
            }
        }

		_hasDragged = false;
		_choseExistingSelection = false;
		_selectionHandles.EndDragHandle();
	}
    private SkPolyline CreateNewPath()
    {
        return new SkPolyline((int)Bounds.Width, (int)Bounds.Height,
            DrawingData.BrushSize, DrawingData.SelColor.ToUInt32(), DrawingData.Feather);
	}
	private bool CheckHandlesHit(Point currentPosition)
	{
		bool result = false;
		if (Selection.HasSelection)
		{
			_selectionHandles.HitTest(currentPosition.ToSKPoint());
			if (_selectionHandles.IsHandlePressed)
			{
				result = true;
			}
		}
		return result;
	}
	private bool CheckSelectionHit(Point currentPosition)
	{
		var result = false;
		var skpt = new SKPoint((float)currentPosition.X, (float)currentPosition.Y);
		foreach (var path in _paths)
		{
			if (path.IsPointInside(skpt))
			{
				result = true;
				if (Selection.GetIds().Contains(path.Id))
				{
					_choseExistingSelection = true;
				}
				else
				{
					SelectElementCommand cmd = new SelectElementCommand(this, path);
					_engine.Stack.Do(cmd);
				}
				InvalidateVisual();
				break;
			}
		}

		return result;
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

    public void DeleteSelected()
    {
        if(Selection.HasSelection)
        {
            var cmd = new DeleteSelectedElementsCommand(this);
            _engine.Stack.Do(cmd);
            InvalidateVisual();
        }
	}
	public void Move(SelectionTransformKind transformKind, bool isControlDown, bool isShiftDown)
	{
		var amount = isControlDown ? 1f : isShiftDown ? 32f : 4f;
		var dx = transformKind == SelectionTransformKind.MoveLeft ? -amount : transformKind == SelectionTransformKind.MoveRight ? amount : 0;
		var dy = transformKind == SelectionTransformKind.MoveUp ? -amount : transformKind == SelectionTransformKind.MoveDown ? amount : 0;
		var cmd = TransformElementCommand.CreateTranslationCommand(this, dx, dy);
		_engine.Stack.Do(cmd);
		InvalidateVisual();
	}
	public void Move(float dx, float dy)
	{
		var cmd = TransformElementCommand.CreateTranslationCommand(this, dx, dy, false);
		_engine.Stack.Do(cmd);
		InvalidateVisual();
	}
	public void Rotate(SelectionTransformKind transformKind, bool isControlDown, bool isShiftDown)
	{
		var amount = isControlDown ? 1f : isShiftDown ? 15f : 45f;
		var rot = transformKind == SelectionTransformKind.RotateLeft ? -amount : transformKind == SelectionTransformKind.RotateRight ? amount : 0;
		var cmd = TransformElementCommand.CreateRotationCommand(this, rot);
		_engine.Stack.Do(cmd);
		InvalidateVisual();
	}
	public void Scale(SelectionTransformKind transformKind, bool isControlDown, bool isShiftDown)
	{
		var amount = isControlDown ? 1.05f : isShiftDown ? 1.2f : 1.1f;
		var scale = 
            transformKind == SelectionTransformKind.ScaleDown ? 1f/amount : 
            transformKind == SelectionTransformKind.ScaleUp ? amount : 0;
		var cmd = TransformElementCommand.CreateScaleCommand(this, scale, scale);
		_engine.Stack.Do(cmd);
		InvalidateVisual();
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