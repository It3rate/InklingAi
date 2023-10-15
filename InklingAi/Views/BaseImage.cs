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
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using InklingAi.ViewModels;
using Avalonia.Data;
using Avalonia.VisualTree;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using InklingAi.Utils;
using Avalonia.Threading;

public abstract class BaseImage : UserControl
{
    protected RenderingLogic renderingLogic;

    protected List<ImageData> _imageDataList = new();
    public float _widthExtra = 2f;
    private Point? _startPoint;
    private int _minDragDistance = 8;

    #region Avalonia Properties

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

    #endregion

    public BaseImage()
    {
        renderingLogic = new RenderingLogic();
        renderingLogic.RenderCall += RenderSkia;

        SetValue(DragDrop.AllowDropProperty, true);
        PointerPressed += OnImagePointerPressed;
        PointerMoved += OnImagePointerMoved;
        PointerReleased += OnImagePointerReleased;

        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);

        AttachedToVisualTree += OnLoaded;
    }

    protected virtual void OnLoaded(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var mvm = MainViewModel.GetRootDataContext(this);
        if (mvm != null)
        {
            var binding = new Binding("SDConfig.AspectSize")
            {
                Mode = BindingMode.OneWay,
                Source = mvm
            };
            this.Bind(AspectSizeProperty, binding);
        }
    }

    Stretch _stretch = Stretch.UniformToFill;
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
        }
    }

    #region Mouse Events
    private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _startPoint = e.GetPosition(this);
        OnImagePointerMoved(sender, e);
    }

    private async Task DoDrag(object? sender, PointerEventArgs e)
    {
        var dragData = new DataObject();
        dragData.Set(DataFormats.Files, _imageDataList);
        var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
        _startPoint = null;
    }
    protected virtual void OnDragOver(object? sender, DragEventArgs e)
    {
    }

    protected abstract void OnDrop(object? sender, DragEventArgs e);

    private void OnImagePointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_startPoint.HasValue)
            return;

        var currentPosition = e.GetPosition(this);
        var diff = currentPosition - _startPoint.Value;

        if (Math.Abs(diff.X) > _minDragDistance || Math.Abs(diff.Y) > _minDragDistance)
        {
            _ = DoDrag(sender, e);
        }
    }

    private void OnImagePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _startPoint = null;
    }
    #endregion

    protected abstract void RenderSkia(SKCanvas canvas);
    public override void Render(DrawingContext context)
    {
        renderingLogic.Bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
        context.Custom((ICustomDrawOperation)renderingLogic);
        base.Render(context);
    }
    protected override Size MeasureOverride(Size availableSize) => Stretch.CalculateSize(availableSize, (Size)AspectSize, StretchDirection.Both);
    protected override Size ArrangeOverride(Size finalSize) => Stretch.CalculateSize(finalSize, (Size)AspectSize);

    protected void DrawImage(SKCanvas canvas, SKBitmap? img, double alpha = 1.0)
    {
        if (img != null && Bounds.Width > 0 && Bounds.Height > 0)
        {
            Rect viewPort = new Rect(Bounds.Size);
            Size sourceSize = new Size(img.Width, img.Height);

                Vector scale = _stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection.Both);
                Size scaledSize = sourceSize * scale;

                Rect destRect = viewPort
                    .CenterRect(new Rect(scaledSize))
                    .Intersect(viewPort);
                Rect sourceRect = new Rect(sourceSize)
                    .CenterRect(new Rect(destRect.Size / scale));

                var skSource = sourceRect.ToSKRect();
                var skDest = destRect.ToSKRect();

            using (SKPaint paint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High })
            {
                if (alpha != 1.0)
                {
                    paint.BlendMode = SKBlendMode.ColorDodge;
                }
                canvas.DrawBitmap(img, skSource, skDest, paint);
            }
        }
    }

}








public class RenderingLogic : ICustomDrawOperation
{
    public delegate void RenderEventHandler(SKCanvas canvas);
    public event RenderEventHandler RenderCall;

    public Rect Bounds { get; set; }
    public void Dispose() { }
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

}
