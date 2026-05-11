using System.Net;
using System.Net.Sockets;
using FastOSC;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using SDL2;


namespace osc_controller_relay.buny {
    static class Program
    {
        static void Main()
        {

            Init();
            RunLoop();
            Cleanup();
        }

        static void Init()
        {
            int result = SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_JOYSTICK);
            if (result < 0)
            {
                Console.WriteLine($"SDL init failed: {SDL.SDL_GetError()}");
            }
            _ = ControllerInput.ControllerHandling();
            _ = Osc.OscHandling();
            Raylib.InitWindow(Config.WindowWidth, Config.WindowHeight, Config.WindowTitle);
            Raylib.SetTargetFPS(Config.TargetFps);
            rlImGui.Setup(true);
        }

        static void RunLoop()
        {
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.LightGray);
                Raylib.DrawFPS(5, 5);
                rlImGui.Begin();
                DrawUI();
                rlImGui.End();
                Raylib.EndDrawing();
            }
        }

        static void DrawUI()
        {
            ImGui.Begin("Config Window");
            ImGui.Text("made by benjibuny007 :D and other, check lisences");
            ImGui.Separator();
            if (showDebug)
                DrawDebug();
            ImGui.End();
        }
        static bool showDebug = Config.ShowDebug;

        static void DrawDebug()
        { 
            int x = 10;
            int y = 10;
            int lh = 20;

            for (int i = 0; i < 4; i++)
            {
            var g = ControllerState.Gamepads[i];

        Raylib.DrawText($"--- Controller {i + 1} ---", x, y, 18, g.Connected ? Color.Green : Color.Red);
        y += lh;

        if (!g.Connected)
        {
            Raylib.DrawText("Disconnected", x, y, 16, Color.Gray);
            y += lh * 2;
            continue;
        }

        Raylib.DrawText($"A:{g.A} B:{g.B} X:{g.X} Y:{g.Y}", x, y, 16, Color.White); y += lh;
        Raylib.DrawText($"Start:{g.Start} Select:{g.Select} Guide:{g.Guide}", x, y, 16, Color.White); y += lh;
        Raylib.DrawText($"LB:{g.LeftBumper} RB:{g.RightBumper}", x, y, 16, Color.White); y += lh;
        Raylib.DrawText($"LS:{g.LeftStickClick} RS:{g.RightStickClick}", x, y, 16, Color.White); y += lh;
        Raylib.DrawText($"Dpad U:{g.DpadUp} D:{g.DpadDown} L:{g.DpadLeft} R:{g.DpadRight}", x, y, 16, Color.White); y += lh;
        Raylib.DrawText($"LeftStick  X:{g.LeftX:F2} Y:{g.LeftY:F2}", x, y, 16, Color.Yellow); y += lh;
        Raylib.DrawText($"RightStick X:{g.RightX:F2} Y:{g.RightY:F2}", x, y, 16, Color.Yellow); y += lh;
        Raylib.DrawText($"LT:{g.LeftTrigger:F2} RT:{g.RightTrigger:F2}", x, y, 16, Color.Orange); y += lh;
        y += lh;
        }
        }
        static void Cleanup()
        {
            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}
