using System.Text.Json;
namespace osc_controller_relay.buny
{
    public static class Config
{
    // Network
    public static string TargetIp      = "127.0.0.1";
    public static int    TargetPort    = 9067;
    public static int    ListenPort    = 9001;

    // OSC
    public static int    SendRateMs    = 10;
    public static string AddressPrefix = "/controller/";

    // Controllers
    public static float  Deadzone      = 0.05f;
    public static bool   InvertLeftY   = false;
    public static bool   InvertRightY  = false;

    // UI
    public static int    WindowWidth   = 500;
    public static int    WindowHeight  = 1000;
    public static int    TargetFps     = 60;
    public static bool   ShowDebug     = true;
    public static string WindowTitle   = "benjibuny controller to osc !!testing!!";
}

}