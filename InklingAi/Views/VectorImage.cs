namespace InklingAi.Views;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Skia;
using InklingAi.CommandEngine;
using InklingAi.Commands;
using InklingAi.ViewModels;
using InkPad.Adapters;
using InkPad.Utils;
using SkiaSharp;

public class VectorImage : BaseImage, INotifyPropertyChanged
{
	private InklingModel _model => InklingModel.Instance;
	private Engine _engine => _model.Engine;
    private KeyboardData _keyboardData => _model.KeyboardData;

    // todo: Move vector data to it's own model class, add bitmap rendering on demand.
    public override ImageData? Data { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override SKBitmap? SourceImage { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    public BrushPanelMode BrushPanelMode { get; set; } = BrushPanelMode.None;
    public DrawPanelMode DrawPanelMode { get; set; } = DrawPanelMode.Paint;
    public DrawState CurrentDrawState { get; set; } = DrawState.Black;

    public BrushState BrushState { get; private set; }
    public BrushSettings BrushSettings { get; private set; }
    private BrushState _unSelBrushState { get; set; }

    private IBrushState _prevBrushStateSource;

    private List<ISelectableElement> _paths { get; } = new List<ISelectableElement>();

	public VectorImage(BrushSettings brushSettings)
    {
        BrushSettings = brushSettings;
        _unSelBrushState = new BrushState(this);
        BrushState  = new BrushState(this);
        Selection = new Selection(this);
        _selectionHandles = new SelectionHandles(this);

        _prevBrushStateSource = BrushState;
        _canDrag = false;
		_curPath = CreateNewPath();
        Selection.ContentChanged += Selection_ContentChanged;
        BrushState.BrushSizeChanged += OnBrushSizeChanged;
        BrushState.FeatherChanged += OnFeatherChanged;
        BrushState.TransparencyChanged += OnTransparencyChanged;
        BrushState.ColorValueChanged += OnColorValueChanged;
    }

    #region Elements
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
    #endregion

    #region Mouse Events
    private bool _hasDragged = false;
    private bool _madeSelection = false;
    private bool _choseExistingSelection = false;
    private bool _isDraggingHandle = false;
    private SKPoint _startDragPoint = SKPoint.Empty;
    private SKPoint _curDragPoint = SKPoint.Empty;
    private SKPoint _endDragPoint = SKPoint.Empty;
    protected override void OnMouseDown(Point currentPosition)
	{
        _startDragPoint = currentPosition.ToSKPoint();
        _curPath.ColorValue = BrushState.SelColor.ToUInt32();
        _curPath.BrushSize = BrushState.BrushSize;
        _curPath.Feather = BrushState.Feather;
        _curPath.Reset();

        switch (DrawPanelMode)
        {
            case DrawPanelMode.Arrow:
                if (CheckHandlesHit(currentPosition))
                {
                    _isDraggingHandle = true;
                }
                else
                {
                    if (CheckSelectionHit(currentPosition))
                    {
                    }
                    else
                    {
                        Selection.Clear();
                    }
                }
                break;
            case DrawPanelMode.Rect:
            case DrawPanelMode.Oval:
            case DrawPanelMode.Star:
                break;
            case DrawPanelMode.Paint:
                _curPath.AddPoint(currentPosition.X, currentPosition.Y);
                break;
        }
    }
    protected override void OnMouseMove(Point currentPosition)
    {
        BrushState.CurrentPosition = currentPosition.ToSKPoint();

        switch (DrawPanelMode)
        {
            case DrawPanelMode.Arrow:
                break;
            default:
                InvalidateVisual();
                break;
        }
	}
    protected override void OnMouseDownDrag(Point currentPosition, Point diff)
	{
		_hasDragged = true;
        _curDragPoint = currentPosition.ToSKPoint();

        switch (DrawPanelMode)
        {
            case DrawPanelMode.Arrow:
                OnDownDrag_Arrow(currentPosition, diff);
                break;
            case DrawPanelMode.Rect:
                _curPath.SetRect(_startDragPoint, _curDragPoint);
                break;
            case DrawPanelMode.Oval:
                _curPath.SetOval(_startDragPoint, _curDragPoint);
                break;
            case DrawPanelMode.Star:
                _curPath.SetStar(_startDragPoint, _curDragPoint);
                break;
            case DrawPanelMode.Paint:
                _curPath.AddPoint(currentPosition.X, currentPosition.Y);
                break;
        }

        InvalidateVisual();
    }
    protected override void OnMouseUp(Point currentPosition, Point diff)
    {
        _endDragPoint = currentPosition.ToSKPoint();
        switch (DrawPanelMode)
        {
            case DrawPanelMode.Arrow:
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
                break;
            case DrawPanelMode.Rect:
                _curPath.SetRect(_startDragPoint, _endDragPoint);
                break;
            case DrawPanelMode.Oval:
                _curPath.SetOval(_startDragPoint, _endDragPoint);
                break;
            case DrawPanelMode.Star:
                _curPath.SetStar(_startDragPoint, _endDragPoint);
                break;
            case DrawPanelMode.Paint:
                _curPath.AddPoint(currentPosition.X, currentPosition.Y);
                break;
        }

        if (DrawPanelMode.IsDrawMode())
        {
            if (_curPath.PointCount > 2 && _hasDragged)
            {
                var cmd = new CreateSKPolylineCommand(_curPath, this);
                _engine.Stack.Do(cmd);
            }
            else
            {
                Selection.Clear();
            }
            _curPath = CreateNewPath();
        }
        _startDragPoint = SKPoint.Empty;
        _curDragPoint = SKPoint.Empty;
        _endDragPoint = SKPoint.Empty;

        _hasDragged = false;
        _madeSelection = false;
        _choseExistingSelection = false;
        _isDraggingHandle = false;
        _selectionHandles.EndDragHandle();
        InvalidateVisual();
    }
    private void OnDownDrag_Arrow(Point currentPosition, Point diff)
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
    private SkPolyline CreateNewPath()
    {
        return new SkPolyline((int)Bounds.Width, (int)Bounds.Height,
            BrushState.BrushSize, BrushState.SelColor.ToUInt32(), BrushState.Feather);
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

    public override void OnDragOver(object? sender, DragEventArgs e)
    {
        var underlay = GetUnderlayImage();
        if(underlay != null)
        {
            underlay.OnDragOver(sender, e);
        }
    }
    public override void OnDrop(object? sender, DragEventArgs e)
    {
        var underlay = GetUnderlayImage();
        if (underlay != null)
        {
            underlay.OnDrop(sender, e);
        }
    }
    #endregion

    private SingleImage? GetUnderlayImage()
    {
        SingleImage? result = null;

        var sibs = Parent?.GetLogicalSiblings();
        if (sibs != null)
        {
            foreach (var ctrl in sibs)
            {
                if (ctrl is SingleImage si && si.Name == "_editImageControl")
                {
                    result = si;
                }
            }
        }
        return result;
    }

    #region Keyboard Events
    private bool IsShiftDown() => _keyboardData.IsShiftDown;
	private bool IsControlDown() => _keyboardData.IsControlDown;
	private bool IsKeyDown(Key key) => _keyboardData.IsDown(key);
    #endregion

    #region Commands
    public void DeleteSelected()
    {
        if(Selection.HasSelection)
        {
            var cmd = new DeleteSelectedElementsCommand(this);
            _engine.Stack.Do(cmd);
            InvalidateVisual();
        }
	}

    private void OnBrushSizeChanged(object? sender, EventArgs e)
    {
        if(Selection.Count > 0)
        {
            var bd = GetBrushDataClone();
            bd.BrushSize = BrushState.BrushSize;
            var cmd = new SetSelectionAttributesCommand(this, bd);
            _engine.Stack.Do(cmd);
        }
        else
        {
            _unSelBrushState.BrushSize = BrushState.BrushSize;
        }
        InvalidateVisual();
    }
    private void OnFeatherChanged(object? sender, EventArgs e)
    {
        if (Selection.Count > 0)
        {
            var bd = GetBrushDataClone();
            bd.Feather = BrushState.Feather;
            var cmd = new SetSelectionAttributesCommand(this, bd);
            _engine.Stack.Do(cmd);
        }
        else
        {
            _unSelBrushState.Feather = BrushState.Feather;
        }
        InvalidateVisual();
    }
    private void OnTransparencyChanged(object? sender, EventArgs e)
    {
        if (Selection.Count > 0)
        {
            var bd = GetBrushDataClone();
            bd.Transparency = BrushState.Transparency;
            var cmd = new SetSelectionAttributesCommand(this, bd);
            _engine.Stack.Do(cmd);
        }
        else
        {
            _unSelBrushState.Transparency = BrushState.Transparency;
        }
        InvalidateVisual();
    }
    private void OnColorValueChanged(object? sender, EventArgs e)
    {
        if (Selection.Count > 0)
        {
            var bd = GetBrushDataClone();
            bd.ColorValue = BrushState.ColorValue;
            var cmd = new SetSelectionAttributesCommand(this, bd);
            _engine.Stack.Do(cmd);
        }
        else
        {
            _unSelBrushState.ColorValue = BrushState.ColorValue;
        }
        InvalidateVisual();
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
    #endregion

    #region Selection
    public Selection Selection { get; }
    private SkPolyline _curPath;
    private SelectionHandles _selectionHandles;
    private void Selection_ContentChanged(object? sender, EventArgs e)
    {
        IBrushState src = BrushStateSource;
        if (_prevBrushStateSource != src)
        {
            BrushState.SetValuesNoEvents(src);

            _prevBrushStateSource = src;
        }
    }
    #endregion

    #region BrushState
    public IBrushState? SingleSelection => Selection.Count == 1 ? Selection[0] : null;
    public IBrushState BrushStateSource => SingleSelection ?? _unSelBrushState;

    public BrushData GetBrushDataClone() => BrushState.GetBrushDataClone();
    public void SetWithBrushData(BrushData brushData)
    {
        BrushState.SetWithBrushData(brushData);
    }

    #endregion

    #region Render

    private UIRenderUtils _renderUtil = new UIRenderUtils();
    protected override void RenderSkia(SKCanvas canvas)
    {
        for (int i = 0; i < _paths.Count; i++) // Note: foreach can have collection modified issues
        {
            _renderUtil.DrawShape(canvas, _paths[i]);
        }

        if (DrawPanelMode == DrawPanelMode.Arrow)
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
                _renderUtil.RenderHoverBrush(canvas, BrushState); // draw pen nib
		    }
        }
        _selectionHandles.RenderSkia(canvas);
    }

    #endregion
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