namespace InklingAi.Views;

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Skia;
using InklingAi.CommandEngine;
using InklingAi.Commands;
using InklingAi.ViewModels;
using InkPad.Adapters;
using SkiaSharp;

public class PaintableImage : BaseImage
{
    public SkPolyline? SelectedPath { get; set; } 
    private List<SkPolyline> _paths { get; } = new List<SkPolyline>();
    private SkPolyline _curPath;
    private UIRenderUtils _renderUtil = new UIRenderUtils();

    public DrawState CurrentDrawState { get; set; } = DrawState.Black;
    public DrawingData DrawingData => DrawingData.Instance;

    public PaintableImage()
    {
        _canDrag = false;
		_curPath = CreateNewPath();
    }
    public void AddPath(SkPolyline path)
    {
        _paths.Add(path);
        SelectedPath = path;

		InvalidateVisual();
	}
    public void RemoveLastPath()
    {
        if (_paths.Count > 0)
        {
            _paths.RemoveAt(_paths.Count - 1);
            _curPath.Reset();
			InvalidateVisual();
		}
    }

    #region Mouse Events

    protected override void OnMouseMove(Point currentPosition)
    {
        DrawingData.CurrentPosition = currentPosition.ToSKPoint();
		InvalidateVisual();
	}
	protected override void OnMouseDown(Point currentPosition)
	{
		_curPath.ColorValue = DrawingData.SelColor.ToUInt32();
		_curPath.BrushSize = DrawingData.BrushSize;
		_curPath.Feather = DrawingData.Feather;
		_curPath.Reset();
        _curPath.AddPoint(currentPosition.X, currentPosition.Y);
    }
    protected override void OnMouseDownDrag(Point currentPosition, Point diff)
    {
        _curPath.AddPoint(currentPosition.X, currentPosition.Y);
        if (_curPath.Count > 1)
        {
            InvalidateVisual();
        }
    }
    protected override void OnMouseUp(Point currentPosition, Point diff)
    {
        _curPath.AddPoint(currentPosition.X, currentPosition.Y);
        if (_curPath.Count > 1)
        {
            //_paths.Add(_curPath); // addpath command
            var cmd = new CreateSKPolylineCommand(_curPath, this);
            Engine.Instance.Stack.Do(cmd);

            _curPath = CreateNewPath();
            //InvalidateVisual();
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
    //public void OnMouseEnter(EventArgs e)
    //{
    //	_isMouseInside = true;
    //}
    //public void OnDragOver(DragEventArgs drgevent)
    //{
    //	_lastPoint = InputPb.PointToClient(new System.Drawing.Point(drgevent.X, drgevent.Y)).CvPoint();
    //	Invalidate(true);
    //}
    //public void OnMouseLeave(EventArgs e)
    //{
    //	_isMouseInside = false;
    //}
    //public void OnDragEnter(DragEventArgs e)
    //{
    //	_isMouseInside = true;
    //}
    //public void OnDragLeave(EventArgs e)
    //{
    //	_isMouseInside = false;
    //}
    //public void OnDragDrop(DragEventArgs e)
    //{
    //	IsPanning = false;
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
    private bool IsShiftDown() => KeyboardData.Instance.IsShiftDown;
    private bool IsControlDown() => KeyboardData.Instance.IsControlDown;

    //private System.Timers.Timer _reprocessTimer = new System.Timers.Timer(500);
    //private void _reprocessTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    //{
    //	ReprocessRequested?.Invoke();
    //}
    protected override void OnDrop(object? sender, DragEventArgs e) { }
    #endregion


    private SKPaint _highlight = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0, 0, 0, 150),
        StrokeWidth = 3,
        StrokeCap = SKStrokeCap.Round,
        StrokeJoin = SKStrokeJoin.Round,
        StrokeMiter = 3.0f,
        IsAntialias = true,
    };

    protected override void RenderSkia(SKCanvas canvas)
    {
        for (int i = 0; i < _paths.Count; i++) // Note: foreach can have collection modified issues
        {
            DrawShape(canvas, _paths[i]);
        }

        if (_curPath != null)
        {
            DrawShape(canvas, _curPath, true);
        }

        if (IsMouseInside && !IsMouseDown)
        {
            _renderUtil.RenderHoverBrush(canvas, DrawingData);
		}

        if(SelectedPath != null)
        {
            _renderUtil.RenderSelectionFrame(canvas, SelectedPath);
        }
    }
    private void DrawShape(SKCanvas canvas, SkPolyline line, bool strokePaint = false)
    {
        if(strokePaint)
        {
            _highlight.StrokeWidth = line.BrushSize + 2;
            canvas.DrawPath(line.PathOutline, _highlight);
        }
        canvas.DrawPath(line.PathOutline, line.Paint);
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