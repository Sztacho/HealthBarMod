using Vintagestory.API.Common.Entities;

namespace HealthBar.Client.Visibility;

public interface ILineOfSightService
{
    bool HasLineOfSight(Entity target);
}
