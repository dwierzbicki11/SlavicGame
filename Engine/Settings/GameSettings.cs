namespace SlavicGame.Engine.Settings;

public enum CameraPreference
{
    FirstPerson,
    ThirdPerson
}

public sealed class GameSettings
{
    public bool Fullscreen { get; set; } = true;
    public bool VSync { get; set; }
    public bool ShowFps { get; set; } = true;

    public float FieldOfViewDegrees { get; set; } = 60f;
    public float MouseSensitivity { get; set; } = 1.0f;
    public CameraPreference Camera { get; set; } = CameraPreference.FirstPerson;

    public bool Sky { get; set; } = true;
    public bool Sun { get; set; } = true;
    public bool Moon { get; set; } = true;
    public bool Stars { get; set; } = true;
    public bool VolumetricClouds { get; set; } = true;
    public bool CloudShadows { get; set; } = true;
    public bool SunShadows { get; set; } = true;
    public bool Fog { get; set; } = true;
    public bool TerrainPbr { get; set; } = true;
    public bool ModelPbr { get; set; } = true;

    public void Normalize()
    {
        FieldOfViewDegrees = Math.Clamp(FieldOfViewDegrees, 50f, 100f);
        MouseSensitivity = Math.Clamp(MouseSensitivity, 0.25f, 3.0f);
    }
}
