using System;

namespace HealthBar.Config;

public sealed class NullConfigProvider : IConfigProvider
{
    public static NullConfigProvider Instance { get; } = new();

    private static readonly ModConfigSnapshot DefaultSnapshot = new(new ModConfig());

    public ModConfigSnapshot Snapshot => DefaultSnapshot;

    public event Action Changed
    {
        add { }
        remove { }
    }

    private NullConfigProvider()
    {
    }
}
