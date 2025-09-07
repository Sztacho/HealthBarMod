#nullable enable
namespace HealthBar;
public class ModConfig {
	public static string ConfigName = "HealthbarKnewOne.json";
	public static ModConfig Instance = new ModConfig();

	public bool Enabled = true;
	public bool ShowOnPlayer = true;
	public bool ShowHpText = true;
	public float BarWidth = 66f;
	public float BarHeight = 6.6f;
	public float VerticalOffset = 22f;

	public float FadeInSpeed = 0.3f;
	public float FadeOutSpeed = 0.5f;

	public string FullHealthColor = "#44FF44";
	public string MidHealthColor = "#FFCC00";
	public string LowHealthColor = "#FF4444";
	public string FrameColor = "#cccccc";

	public float MidHealthThreshold = 0.6f;
	public float LowHealthThreshold = 0.25f;
}