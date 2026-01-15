using System;

namespace HealthBar.Config;

public interface IConfigProvider
{
    ModConfigSnapshot Snapshot { get; }
    event Action Changed;
}
