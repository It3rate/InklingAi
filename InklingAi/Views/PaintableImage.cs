namespace InklingAi.Views;

using Avalonia.Input;
using SkiaSharp;
using System;
using InkPad.Adapters;
using Avalonia;
using InklingAi.ViewModels;

public class PaintableImage : BaseImage
{
	private SkPolylineAdapter _curPath;
	public DrawState CurrentDrawState { get; set; } = DrawState.Black;

	public PaintableImage()
	{
		_canDrag = false;
		this.LayoutUpdated += OnLayoutUpdated;
	}
	protected void OnLayoutUpdated(object? sender, EventArgs e)
	{
		_curPath = new SkPolylineAdapter((int)Bounds.Width, (int)Bounds.Height);
	}

	#region Mouse Events

	protected override void OnMouseDown(Point currentPosition)
	{
		_curPath.Reset();
		_curPath.AddPoint(currentPosition.X, currentPosition.Y);
	}
	protected override void OnMouseMoved(Point currentPosition, Point diff)
	{
		_curPath.AddPoint(currentPosition.X, currentPosition.Y);
		if(_curPath.Count > 1)
		{
			InvalidateVisual();
		}
	}
	protected override void OnMouseUp(Point currentPosition, Point diff)
	{
		_curPath.AddPoint(currentPosition.X, currentPosition.Y);
		if (_curPath.Count > 1)
		{
			InvalidateVisual();
		}
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
	#endregion


	protected override void OnDrop(object? sender, DragEventArgs e) { }
	protected override void RenderSkia(SKCanvas canvas)
	{
		canvas.Clear(SKColors.White);
		var path = _curPath.ToPath(false); 
		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Red,
			StrokeWidth = 4,
			IsAntialias = true,
		};
		canvas.DrawPath(path, paint);
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