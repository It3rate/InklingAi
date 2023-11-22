namespace InklingAi.Views;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
//using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Skia;
using InklingAi.CommandEngine;
using InklingAi.Commands;
using InklingAi.ViewModels;
using InkPad.Adapters;
using InkPad.Utils;
using SkiaSharp;
using Tmds.DBus.SourceGenerator;

public class PaintableImage : BaseImage, INotifyPropertyChanged
{
	private InklingModel _model => InklingModel.Instance;
	private Engine _engine => _model.Engine;
	private KeyboardData _keyboardData => _model.KeyboardData;


    public BrushMode BrushMode { get; set; } = BrushMode.None;
    public DrawMode DrawMode { get; set; } = DrawMode.Paint;

    private DrawingData _unSelDrawingData { get; set; } = new DrawingData();
    public DrawingData DrawingData { get; set; } = new DrawingData();
    private IDrawingData _prevDrawingDataSource;
    public IDrawingData? SingleSelection => Selection.Count == 1 ? Selection[0] : null;
    //public IDrawingData DrawingDataSource => SingleSelection ?? DrawingData;

    //public static readonly StyledProperty<IDrawingData> DrawingDataSourceProperty = AvaloniaProperty.Register<PaintableImage, IDrawingData>(nameof(DrawingDataSource));
    //public IDrawingData DrawingDataSource
    //{
    //    get => GetValue(DrawingDataSourceProperty);
    //    set
    //    {
    //        SetValue(DrawingDataSourceProperty, value);
    //        InvalidateVisual();
    //    }
    //}
    public IDrawingData DrawingDataSource => SingleSelection ?? _unSelDrawingData;


    public static readonly StyledProperty<byte> TransparencyProperty = AvaloniaProperty.Register<PaintableImage, byte>(nameof(Transparency), 255);
    public byte Transparency
    {
        get => GetValue(TransparencyProperty);
        set
        {
            SetValue(TransparencyProperty, value);
            DrawingDataSource.Transparency = value;
            InvalidateVisual();
        }
    }

    public static readonly StyledProperty<float> FeatherProperty = AvaloniaProperty.Register<PaintableImage, float>(nameof(Feather), 0);
    public float Feather
    {
        get => GetValue(FeatherProperty);
        set
        {
            SetValue(FeatherProperty, value);
            DrawingDataSource.Feather = value;
            InvalidateVisual();
        }
    }
    public Color SelColor
    {
        get => Color.FromUInt32(DrawingDataSource.ColorValue);
        set
        {
            DrawingDataSource.ColorValue = value.ToUInt32();
            InvalidateVisual();
        }
    }
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
        _prevDrawingDataSource = DrawingData;
        _canDrag = false;
		_curPath = CreateNewPath();
        _selectionHandles = new SelectionHandles(this);
        Selection.ContentChanged += Selection_ContentChanged;
        DrawingData.BrushSizeChanged += DrawingData_BrushSizeChanged;
        DrawingData.FeatherChanged += DrawingData_FeatherChanged;
        DrawingData.TransparencyChanged += DrawingData_TransparencyChanged;
        DrawingData.ColorValueChanged += DrawingData_ColorValueChanged;
    }

    private void DrawingData_ColorValueChanged(object? sender, EventArgs e)
    {
        DrawingDataSource.ColorValue = DrawingData.ColorValue;
        InvalidateVisual();
    }

    private void DrawingData_TransparencyChanged(object? sender, EventArgs e)
    {
        DrawingDataSource.Transparency = DrawingData.Transparency;
        InvalidateVisual();
    }

    private void DrawingData_FeatherChanged(object? sender, EventArgs e)
    {
        DrawingDataSource.Feather = DrawingData.Feather;
        InvalidateVisual();
    }

    private void DrawingData_BrushSizeChanged(object? sender, EventArgs e)
    {
        DrawingDataSource.BrushSize = DrawingData.BrushSize;
        InvalidateVisual();
    }

    private void Selection_ContentChanged(object? sender, EventArgs e)
    {
        IDrawingData src = SingleSelection ?? DrawingData;
        if (_prevDrawingDataSource != src)
        {
            DrawingData.SetValuesNoEvents(src);

            _prevDrawingDataSource = src;
        }
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
    private bool _madeSelection = false;
    private bool _choseExistingSelection = false;
    private bool _isDraggingHandle = false;
    protected override void OnMouseDown(Point currentPosition)
	{
        if (DrawMode == DrawMode.Arrow)
        {
            if (CheckHandlesHit(currentPosition))
            {
                _isDraggingHandle = true;
            }
            else
            {
                if (CheckSelectionHit(currentPosition))
                {
                    if (SingleSelection != null)
                    {
                        //DrawingData.ColorValue = SingleSelection.ColorValue;
                        //DrawingData.BrushSize = SingleSelection.BrushSize;
                        //DrawingData.Feather = SingleSelection.Feather;
                        //DrawingData.Transparency = SingleSelection.Transparency;
                    }
                    else
                    {
                        //DrawingData.ColorValue = DrawingData.ColorValue;
                        //DrawingData.BrushSize = DrawingData.BrushSize;
                        //DrawingData.Feather = DrawingData.Feather;
                        //DrawingData.Transparency = DrawingData.Transparency;
                    }
                }
                else
                {
                    Selection.Clear();
                }
            }
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

		if (DrawMode == DrawMode.Arrow)
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
		if (DrawMode == DrawMode.Arrow)
        {
			if (_madeSelection)
			{
                _selectionHandles.Matrix = Move(diff.X, diff.Y, _selectionHandles.Center);
            }
            else if (_isDraggingHandle && Selection.TransformKind == TransformKind.Scale)
            {
                var ta = _selectionHandles.TransformAnchor;
                var startDiff = _startPosition.Value.ToSKPoint() - ta;
                var curDiff = currentPosition.ToSKPoint() - ta;
                var anchor = _selectionHandles.TransformAnchor;
                _selectionHandles.Matrix = Scale(curDiff.X / startDiff.X, curDiff.Y / startDiff.Y, anchor);
            }
            else if (_isDraggingHandle && Selection.TransformKind == TransformKind.Rotate)
            {
                var ta = _selectionHandles.TransformAnchor;
                var startDiff = _startPosition.Value.ToSKPoint() - ta;
                var curDiff = currentPosition.ToSKPoint() - ta;

                var startRadians = Math.Atan2(startDiff.Y, startDiff.X);
                var curRadians = Math.Atan2(curDiff.Y, curDiff.X);

                var deltaRadians = curRadians - startRadians;
                if (deltaRadians > Math.PI)
                {
                    deltaRadians -= 2 * Math.PI;
                }
                else if (deltaRadians < -Math.PI)
                {
                    deltaRadians += 2 * Math.PI;
                }
                _selectionHandles.Matrix = Rotate(deltaRadians * 180f / MathF.PI, ta);
            }
        }
        else
        {
            _curPath.AddPoint(currentPosition.X, currentPosition.Y);
            if (_curPath.PointCount > 1)
            {
                InvalidateVisual();
            }
        }
    }
    protected override void OnMouseUp(Point currentPosition, Point diff)
    {
        if (DrawMode == DrawMode.Arrow)
		{
			if (_choseExistingSelection && !_hasDragged)
			{
				var tk = Selection.TransformKind;
				Selection.TransformKind = (tk == TransformKind.Scale) ?
					TransformKind.Rotate : TransformKind.Scale;
				InvalidateVisual();
			}
            else if (_hasDragged && _engine.Stack.PreviousCommand() is TransformElementCommand tec)
            {
               tec.IsSealed = true;
            }
		}
        else
        {
            _curPath.AddPoint(currentPosition.X, currentPosition.Y);
            if (_curPath.PointCount > 2)
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
        _madeSelection = false;
        _choseExistingSelection = false;
        _isDraggingHandle = false;
        _selectionHandles.EndDragHandle();
        InvalidateVisual();
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
			var pos = _selectionHandles.HitTest(currentPosition.ToSKPoint());
			if (pos != RectPosition.None)
			{
				result = true;
				_selectionHandles.StartDragHandle(pos);
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
                _madeSelection = true;

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
	public SKMatrix Move(double dx, double dy, SKPoint anchor)
	{
		var cmd = TransformElementCommand.CreateTranslationCommand(this, (float)dx, (float)dy, anchor, true);
		_engine.Stack.Do(cmd);
		InvalidateVisual();
        return cmd.NewMatrix;
	}
	public void Rotate(SelectionTransformKind transformKind, bool isControlDown, bool isShiftDown)
	{
		var amount = isControlDown ? 1f : isShiftDown ? 15f : 45f;
		var rot = transformKind == SelectionTransformKind.RotateLeft ? -amount : transformKind == SelectionTransformKind.RotateRight ? amount : 0;
		var cmd = TransformElementCommand.CreateRotationCommand(this, rot);
		_engine.Stack.Do(cmd);
		InvalidateVisual();
    }
	public SKMatrix Rotate(double degrees, SKPoint anchor)
	{
		var cmd = TransformElementCommand.CreateRotationCommand(this, (float)degrees, anchor, _isDraggingHandle);
		_engine.Stack.Do(cmd);
		InvalidateVisual();
        return cmd.NewMatrix;
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
    public SKMatrix Scale(float scaleX, float scaleY, SKPoint anchor)
    {
        var cmd = TransformElementCommand.CreateScaleCommand(this, scaleX, scaleY, anchor, _isDraggingHandle);
        _engine.Stack.Do(cmd);
        InvalidateVisual();
        return cmd.NewMatrix;
    }


    protected override void RenderSkia(SKCanvas canvas)
    {
        for (int i = 0; i < _paths.Count; i++) // Note: foreach can have collection modified issues
        {
            _renderUtil.DrawShape(canvas, _paths[i]);
        }

        if (DrawMode == DrawMode.Arrow)
        {
            if (_selectionHandles.IsHandlePressed)
            {
                _renderUtil.GhostShape(canvas, Selection[0]); // todo: draw original position while transforming. Requires 'live' command stack.
            }
        }
        else
        {
            if (_curPath != null && _curPath.HasContent)
            {
                _renderUtil.DrawShape(canvas, _curPath, true); // in progress drawing
            }

            if (IsMouseInside && !IsMouseDown)
            {
                _renderUtil.RenderHoverBrush(canvas, DrawingData); // draw pen nib
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