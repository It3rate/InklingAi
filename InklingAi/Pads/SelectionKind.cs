namespace InklingAi.Pads;

public enum SelectionKind
{
    None,
    BeginPoint,
    BeginElement,
    CurrentPoint,
    CurrentElement,
    SelectedPoint,
    SelectedElement,
    HighlightPoint,
    HighlightElement,
    ClipboardPoint,
    ClipboardElement,

    LastCommand,
    ToolKind,
}
