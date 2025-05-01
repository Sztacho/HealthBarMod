using Vintagestory.API.Client;

namespace HealthBar.Gui;

public static class ElementBoundsExtensions
{
    /// <summary>
    /// Tworzy kopię ElementBounds z opcjonalnym kopiowaniem dzieci.
    /// </summary>
    /// <param name="bounds">Oryginalne ElementBounds</param>
    /// <param name="deepCopy">Jeśli true, skopiuje także dzieci rekurencyjnie</param>
    /// <returns>Nowa instancja ElementBounds</returns>
    public static ElementBounds Copy(this ElementBounds bounds, bool deepCopy = false)
    {
        var copy = new ElementBounds
        {
            Alignment = bounds.Alignment,
            verticalSizing = bounds.verticalSizing,
            horizontalSizing = bounds.horizontalSizing,
            percentX = bounds.percentX,
            percentY = bounds.percentY,
            percentWidth = bounds.percentWidth,
            percentHeight = bounds.percentHeight,
            percentPaddingX = bounds.percentPaddingX,
            percentPaddingY = bounds.percentPaddingY,
            fixedMarginX = bounds.fixedMarginX,
            fixedMarginY = bounds.fixedMarginY,
            fixedPaddingX = bounds.fixedPaddingX,
            fixedPaddingY = bounds.fixedPaddingY,
            fixedX = bounds.fixedX,
            fixedY = bounds.fixedY,
            fixedOffsetX = bounds.fixedOffsetX,
            fixedOffsetY = bounds.fixedOffsetY,
            fixedWidth = bounds.fixedWidth,
            fixedHeight = bounds.fixedHeight,
            IsDrawingSurface = bounds.IsDrawingSurface,
            Code = bounds.Code,
            Name = bounds.Name,
            AllowNoChildren = bounds.AllowNoChildren,
            ParentBounds = bounds.ParentBounds // zachowujemy referencję
        };

        if (deepCopy)
        {
            foreach (var child in bounds.ChildBounds)
            {
                var childCopy = child.Copy(true);
                copy.WithChild(childCopy);
            }
        }

        return copy;
    }
}
