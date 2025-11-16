#nullable enable
namespace HealthBar.Config;
public class ModConfig {
	public static string ConfigName = "HealthBar.json";
	public static ModConfig Instance = new();

	public bool Enabled = true;
	public bool TargetOnlyHealthBar = false;
	public int DisplayRange = 14;
	public bool ShowOnPlayer = true;
	public bool ShowHpText = true;
	public int BarWidth = 66;
	public int BarHeight = 7;
	public int VerticalOffset = 22;

	public float MinScale = 1.0f;
	public float MaxScale = 8.0f;

	public float FadeInSpeed = 0.3f;
	public float FadeOutSpeed = 0.5f;

	public string FullHealthColor = "#44FF44";
	public string MidHealthColor = "#FFCC00";
	public string LowHealthColor = "#FF4444";
	public string FrameColor = "#cccccc";

	public int MidHealthThreshold = 60;
	public int LowHealthThreshold = 25;
}
