using System.Collections.Generic;
using Vintagestory.API.Common.Entities;

namespace HealthBar.Client.Selection;

public interface ISelectionStrategy
{
    void Select(in SelectionContext ctx, List<Entity> output);
}
