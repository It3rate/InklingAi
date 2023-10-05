namespace InklingAi.Elements;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Flags]
public enum ElementKind
{
    None = 0,
    Terminal = 0x1,
    RefPoint = 0x2,
    FocalPoint = 0x4,
    BondPoint = 0x8,
    Trait = 0x10,
    Focal = 0x20,
    SingleBond = 0x40,
    DoubleBond = 0x80,
    Entity = 0x100,
    Unit = 0x200,
    Group = 0x400,

    Grid = 0x800,
    SelectionRect = 0x1000,
    Ruler = 0x2000,
    Value = 0x4000,

    Any = 0xFFFF,

    PointKind = RefPoint | Terminal | FocalPoint | BondPoint,
    SegmentKind = Trait | Focal | SingleBond | Unit,
    TraitPart = Trait | Terminal | RefPoint,
    FocalPart = Focal | Unit | FocalPoint,
    BondPart = SingleBond | BondPoint,
    UIPart = Grid | SelectionRect | Ruler | Value,
    TraitPointSource = Terminal | RefPoint | Entity,
    FocalPointSource = FocalPoint | Terminal | RefPoint,
    BondPointSource = Focal | BondPoint | FocalPoint,
}
public static class ElementKindExtensions
{
    public static bool IsNone(this ElementKind kind) => kind == ElementKind.None;
    public static bool IsPoint(this ElementKind kind) => (int)(kind & ElementKind.PointKind) > 0;// ElementKind.PointKind.HasFlag(kind);
    public static bool IsTerminal(this ElementKind kind) => kind == ElementKind.Terminal;
    public static bool IsCompatible(this ElementKind self, ElementKind other) => (int)(self & other) != 0;

    public static ElementKind AttachableElements(this ElementKind self)
    {
        var result = ElementKind.None;
        switch (self)
        {
            case ElementKind.Terminal:
                result = ElementKind.Any; // or any?
                break;
            case ElementKind.RefPoint:
                result = ElementKind.PointKind;
                break;
            case ElementKind.FocalPoint:
                result = ElementKind.FocalPointSource;
                break;
            case ElementKind.BondPoint:
                result = ElementKind.BondPointSource;
                break;
        }
        return result;
    }

    public static bool CanCreateWith(this ElementKind self, ElementKind other)
    {
        var result = false;
        switch (self)
        {
            case ElementKind.Terminal:
                result = (other == ElementKind.None);
                break;
            case ElementKind.RefPoint:
                result = other.IsCompatible(ElementKind.PointKind); // maybe only terminal and ref points?
                break;
            case ElementKind.Trait:
                result = other.IsCompatible(ElementKind.TraitPointSource);
                break;
            case ElementKind.Focal:
                result = other.IsCompatible(ElementKind.FocalPointSource);
                break;
            case ElementKind.SingleBond:
                result = other.IsCompatible(ElementKind.BondPointSource);
                break;
        }
        return result;
    }
}
