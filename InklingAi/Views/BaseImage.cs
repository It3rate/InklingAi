namespace InklingAi.Views;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using InklingAi.ViewModels;
using Avalonia.Data;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using InklingAi.Utils;
using AIEnv.Utils;
using Avalonia.Interactivity;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MathNet.Numerics;
using Avalonia.Controls.Shapes;

public abstract class BaseImage : UserControl
{
    protected InklingModel _model => InklingModel.Instance;

    protected RenderingLogic renderingLogic;

    //public SKBitmap? SourceImage { get; protected set; }
    public float _widthExtra = 2f;
    private Stretch _stretch = Stretch.UniformToFill;

    #region Avalonia Properties

    public static readonly StyledProperty<ImageData> DataProperty = AvaloniaProperty.Register<BaseImage, ImageData>(nameof(Data));
    public ImageData Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly StyledProperty<SKBitmap?> CurrentImageProperty = AvaloniaProperty.Register<BaseImage, SKBitmap?>(nameof(CurrentImage));
    //protected SKBitmap? _currentImage;
    public SKBitmap? CurrentImage
    {
        get => GetValue(CurrentImageProperty);
        set => SetValue(CurrentImageProperty, value);
    }

    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<BaseImage, Stretch>(nameof(Stretch), Stretch.UniformToFill);
    public Stretch Stretch
    {
        get { return GetValue(StretchProperty); }
        set { SetValue(StretchProperty, value); }
    }

    public static readonly StyledProperty<IntSize> AspectSizeProperty = AvaloniaProperty.Register<BaseImage, IntSize>(nameof(AspectSize));
    public IntSize AspectSize
    {
        get { return GetValue(AspectSizeProperty); }
        set { SetValue(AspectSizeProperty, value); }
    }
    public static readonly StyledProperty<ImageOrigin> OriginProperty = AvaloniaProperty.Register<BaseImage, ImageOrigin>(nameof(Origin));
    public ImageOrigin Origin
    {
        get { return GetValue(OriginProperty); }
        set { SetValue(OriginProperty, value); }
    }

    public SKMatrix Matrix { get; set; } = SKMatrix.Identity;

    #endregion

    public BaseImage()
    {
		renderingLogic = new RenderingLogic();
        renderingLogic.RenderCall += RenderSkia;

        SetValue(DragDrop.AllowDropProperty, true);
        PointerPressed += OnImagePointerPressed;
        PointerMoved += OnImagePointerMoved;
        PointerReleased += OnImagePointerReleased;
		PointerEntered += OnPointerEntered;
		PointerExited += OnPointerExited;

        AttachedToVisualTree += OnLoaded;
	}

	protected virtual void OnLoaded(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var mvm = InklingModel.GetRootDataContext(this);
        if (mvm != null)
        {
            var binding = new Binding("SDConfig.AspectSize")
            {
                Mode = BindingMode.OneWay,
                Source = mvm
            };
            this.Bind(AspectSizeProperty, binding);
        }
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        RemoveHandler(DragDrop.DropEvent, OnDrop);
        RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
    }
    protected virtual void SetWith(ImageData imageData)
    {
        SetWith(imageData.SourcePath, imageData.ProcessedPath);
    }
    protected virtual void SetWith(string path, string processedPath = "")
    {
        if (Data.ImageKind.HasFixedPath())
        {
            IOUtils.CopyFile(path, Data.SourcePath);
            if(processedPath != "")
            {
                IOUtils.CopyFile(processedPath, Data.ProcessedPath);
            }
            else
            {
                IOUtils.CopyFile(path, Data.ProcessedPath);
            }
            Data.SourcePath = Data.SourcePath;
            Data.ProcessedPath = Data.ProcessedPath;
        }
        else
        {
            Data.SourcePath = path;
            Data.ProcessedPath = processedPath == "" ? path : processedPath;
        }
        Data.Origin = ImageOrigin.Input;
        InvalidateVisual();
    }
    private SKBitmap? _currentImage;
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        var prop = change.Property;
        switch (prop.Name)
        {
            case nameof(AspectSize):
                InvalidateMeasure();
                InvalidateVisual();
                break;

            case nameof(Stretch):
                _stretch = Stretch;
                break;

            case nameof(CurrentImage):
                //if (change.NewValue is SKBitmap currentImage)
                //{
                //    Data.CurrentImage = currentImage;
                //}
                //else
                //{
                //    Data.CurrentImage = null;
                //}
                _currentImage = CurrentImage;
                InvalidateVisual();
                break;
        }
    }

    #region Mouse Events
    protected Point? _startPosition;
	protected Point _currentPosition;
	public bool IsMouseInside { get; protected set; }
	protected int _minDragDistance = 10;
	protected bool _canDrag = true;



    protected virtual void OnMouseDown(Point currentPosition) { }
	protected virtual void OnMouseMove(Point currentPosition) { }
	protected virtual void OnMouseDownDrag(Point currentPosition, Point diff) { }
	protected virtual void OnMouseUp(Point currentPosition, Point diff) { }
    public virtual void OnDragOver(object? sender, DragEventArgs e)
    {
        if (Data != null)
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        e.Handled = true;
    }
    public virtual void OnDrop(object? sender, DragEventArgs e)
    {
        if (Data != null)
        {
            var result = e.DragEffects;
            if ((result & DragDropEffects.Copy) != DragDropEffects.None)
            {
                AcceptImageDrop(e.Data);
                if (Name == "_editImageControl")
                {
                    if (_model.InfluenceData.FocusedItem != null)
                    {
                        //_model.InfluenceData.FocusedItem.DoResetLinked();
                    }
                }
            }
        }
    }
    protected bool IsMouseDown => _startPosition.HasValue;

	protected void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
	{
		_currentPosition = e.GetPosition(this);
		_startPosition = _currentPosition;

		OnMouseDown((Point)_startPosition);
    }
	protected void OnImagePointerMoved(object? sender, PointerEventArgs e)
	{
        IsMouseInside = true;
		_currentPosition = e.GetPosition(this);
        if (_startPosition.HasValue)
        {
            var diff = _currentPosition - _startPosition.Value;

            if (_canDrag && (Math.Abs(diff.X) > _minDragDistance || Math.Abs(diff.Y) > _minDragDistance))
            {
                _ = DoDrag(sender, e);
            }
            else
            {
                OnMouseDownDrag(_currentPosition, diff);
            }
        }
        else
        {
            OnMouseMove(_currentPosition);
        }
    }
	protected void OnImagePointerReleased(object? sender, PointerReleasedEventArgs e)
	{
        if (_startPosition.HasValue)
        {
            _currentPosition = e.GetPosition(this);
            var diff = _currentPosition - _startPosition.Value;
            OnMouseUp(_currentPosition, diff);
        }
        _startPosition = null;
	}
	private void OnPointerEntered(object? sender, PointerEventArgs e)
	{
		IsMouseInside = true;
		InvalidateVisual();
	}
	private void OnPointerExited(object? sender, PointerEventArgs e)
	{
		IsMouseInside = false;
		InvalidateVisual();
	}

	private async Task DoDrag(object? sender, PointerEventArgs e)
    {
        var dragData = new DataObject();
        SetDragContents(dragData);
        var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
        _startPosition = null;
    }

    protected virtual void SetDragContents(DataObject dataObject)
    {
        if (Data != null)
        {
            dataObject.Set(DataFormats.Files, new List<ImageData>() { Data });
        }
    }
    protected virtual void AcceptImageDrop(IDataObject dataObject)
    {
        if (dataObject.Contains(DataFormats.Files))
        {
            var data = dataObject.Get(DataFormats.Files);
            string path = "";
            string processedPath = "";
            if (data is IEnumerable<IStorageItem> dataList)
            {
                var firstFile = dataList.FirstOrDefault();
                path = firstFile?.Path.AbsolutePath ?? "";
            }
            else if (data is IEnumerable<ImageData> imageDataList)
            {
                var first = imageDataList.First();
                if (first != null)
                {
                    path = first.SourcePath;
                    processedPath = first.ProcessedPath;
                }
            }
            else if (data is string sData)
            {
                path = sData;
            }

            if (path != "" && File.Exists(path))
            {
                SetWith(path, processedPath);
            }
        }
    }
    #endregion

    protected virtual void RenderSkia(SKCanvas canvas)
    {
        //DrawImage(canvas, SourceImage);
        DrawImage(canvas, _currentImage);
    }
    public override void Render(DrawingContext context)
    {
        renderingLogic.Bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
        context.Custom((ICustomDrawOperation)renderingLogic);
        base.Render(context);
    }
    protected override Size MeasureOverride(Size availableSize) => Stretch.CalculateSize(availableSize, AspectSize.ToAvaloniaSize(), StretchDirection.Both);
    protected override Size ArrangeOverride(Size finalSize) => Stretch.CalculateSize(finalSize, AspectSize.ToAvaloniaSize());

    private SKMatrix _matrix = SKMatrix.CreateIdentity();// CreateScale(1.2f, 1.2f);
    protected void DrawImage(SKCanvas canvas, SKBitmap? img, double alpha = 1.0)
    {
        if (img != null && Bounds.Width > 0 && Bounds.Height > 0)
        {
            var skDest = SKRect.Create(Bounds.Size.ToSKSize()); // may be non zero at top left
            var skSource = img.Rect().AspectFit(skDest.Size);

            SKMatrix currentMatrix = canvas.TotalMatrix;
            SKMatrix combinedMatrix = SKMatrix.Concat(currentMatrix, _matrix);
            canvas.SetMatrix(combinedMatrix);

            using (SKPaint paint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High })
            {
                if (alpha != 1.0)
                {
                    paint.BlendMode = SKBlendMode.ColorDodge;
                }
                canvas.DrawBitmap(img, skSource, skDest, paint);
            }
            canvas.SetMatrix(currentMatrix);
        }
    }
}



public class RenderingLogic : ICustomDrawOperation
{
    public delegate void RenderEventHandler(SKCanvas canvas);
    public event RenderEventHandler RenderCall;

    public Rect Bounds { get; set; }
    public bool Equals(ICustomDrawOperation? other) => other == this;
    public bool HitTest(Point p) { return true; }

    public void Render(ImmediateDrawingContext context)
    {
        if (context.PlatformImpl is IDrawingContextImpl drawingContext)
        {
            var leaseFeature = drawingContext.GetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            RenderCall?.Invoke(canvas);
        }
    }

    public void Dispose() { }
}

//private IImage? _latentImage;
// transform matrix
// source rect
// mask/alpha channel (can be vector)
// latent data
// process config (eg canny settings, or config that created the image)
// application settings (skip last steps, etc - can be in config)

// Composite settings:
// Images[]
// Blend stack
// Blend percents

// Text:
// letters, styles, font, size, spans etc
// path, matrix etc
// ocr
// gen vs overlay