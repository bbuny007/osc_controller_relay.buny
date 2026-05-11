using System.Net;
using System.Net.Sockets;
using FastOSC;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using SDL2;

namespace osc_controller_relay.buny
{
    public static class ControllerState
    {
        public struct GamepadState
        {
            public bool Connected;
            public IntPtr Controller;
            public string Guid;

            // Buttons
            public bool A, B, X, Y;
            public bool Start, Select;
            public bool LeftBumper, RightBumper;
            public bool LeftStickClick, RightStickClick;
            public bool DpadUp, DpadDown, DpadLeft, DpadRight;
            public bool Guide;

            // Axes
            public float LeftX, LeftY;
            public float RightX, RightY;
            public float LeftTrigger, RightTrigger;

            // Raw joystick
            public float JoyAxis0, JoyAxis1, JoyAxis2, JoyAxis3;
        }

        public static GamepadState[] Gamepads = new GamepadState[4];
    }

    public static class ControllerInput
    {
        public static async Task ControllerHandling()
        {
            // Open already connected controllers on startup
            int numJoysticks = SDL.SDL_NumJoysticks();
            for (int i = 0; i < numJoysticks && i < 4; i++)
            {
                if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
                {
                    IntPtr controller = SDL.SDL_GameControllerOpen(i);
                    if (controller != IntPtr.Zero)
                    {
                        IntPtr joystick = SDL.SDL_GameControllerGetJoystick(controller);
                        var rawGuid = SDL.SDL_JoystickGetGUID(joystick);
                        byte[] pszGUID = new byte[33];
                        SDL.SDL_JoystickGetGUIDString(rawGuid, pszGUID, 33);
                        string guid = System.Text.Encoding.UTF8.GetString(pszGUID).TrimEnd('\0');
                        int slot = FindSlotByGuid(guid);
                        if (slot == -1) slot = FindFreeSlot();
                        if (slot == -1) continue;

                        ControllerState.Gamepads[slot].Connected = true;
                        ControllerState.Gamepads[slot].Controller = controller;
                        ControllerState.Gamepads[slot].Guid = guid;
                        Console.WriteLine($"Controller {i} opened in slot {slot}");
                    }
                }
            }

            while (true)
            {
                SDL.SDL_Event e;
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                        {
                            int joystickIndex = e.cdevice.which;
                            IntPtr controller = SDL.SDL_GameControllerOpen(joystickIndex);
                            if (controller == IntPtr.Zero) break;

                            IntPtr joystick = SDL.SDL_GameControllerGetJoystick(controller);
                            var rawGuid = SDL.SDL_JoystickGetGUID(joystick);
                            byte[] pszGUID = new byte[33];
                            SDL.SDL_JoystickGetGUIDString(rawGuid, pszGUID, 33);
                            string guid = System.Text.Encoding.UTF8.GetString(pszGUID).TrimEnd('\0');

                            int slot = FindSlotByGuid(guid);
                            if (slot == -1) slot = FindFreeSlot();
                            if (slot == -1) { SDL.SDL_GameControllerClose(controller); break; }

                            ControllerState.Gamepads[slot] = new ControllerState.GamepadState();
                            ControllerState.Gamepads[slot].Connected = true;
                            ControllerState.Gamepads[slot].Controller = controller;
                            ControllerState.Gamepads[slot].Guid = guid;
                            Console.WriteLine($"Controller connected -> slot {slot}");
                            break;
                        }

                        case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                        {
                            IntPtr removed = SDL.SDL_GameControllerFromInstanceID(e.cdevice.which);
                            for (int i = 0; i < 4; i++)
                            {
                                if (ControllerState.Gamepads[i].Controller == removed)
                                {
                                    string guid = ControllerState.Gamepads[i].Guid;
                                    SDL.SDL_GameControllerClose(removed);
                                    ControllerState.Gamepads[i] = new ControllerState.GamepadState();
                                    ControllerState.Gamepads[i].Guid = guid; // keep guid for replug
                                    Console.WriteLine($"Controller disconnected from slot {i}");
                                    break;
                                }
                            }
                            break;
                        }

                        case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
                        {
                            int slot = GetSlotFromInstance(e.caxis.which);
                            if (slot == -1) break;
                                float val = e.caxis.axisValue / 32767f;
                                val = MathF.Abs(val) < Config.Deadzone ? 0f : val;
                                switch ((SDL.SDL_GameControllerAxis)e.caxis.axis)
                            {
                                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX:        ControllerState.Gamepads[slot].LeftX        = val; break;
                                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY:        ControllerState.Gamepads[slot].LeftY        = val; break;
                                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX:       ControllerState.Gamepads[slot].RightX       = val; break;
                                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY:       ControllerState.Gamepads[slot].RightY       = val; break;
                                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT:  ControllerState.Gamepads[slot].LeftTrigger  = val; break;
                                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT: ControllerState.Gamepads[slot].RightTrigger = val; break;
                            }
                            break;
                        }

                        case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                        {
                            int slot = GetSlotFromInstance(e.cbutton.which);
                            if (slot == -1) break;
                            SetButton(slot, (SDL.SDL_GameControllerButton)e.cbutton.button, true);
                            break;
                        }

                        case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
                        {
                            int slot = GetSlotFromInstance(e.cbutton.which);
                            if (slot == -1) break;
                            SetButton(slot, (SDL.SDL_GameControllerButton)e.cbutton.button, false);
                            break;
                        }

                        case SDL.SDL_EventType.SDL_JOYAXISMOTION:
                        {
                            int slot = GetSlotFromInstance(e.jaxis.which);
                            if (slot == -1) break;
                            float val = e.jaxis.axisValue / 32767f;
                            val = MathF.Abs(val) < Config.Deadzone ? 0f : val;
                            switch (e.jaxis.axis)
                            {
                                case 0: ControllerState.Gamepads[slot].JoyAxis0 = val; break;
                                case 1: ControllerState.Gamepads[slot].JoyAxis1 = val; break;
                                case 2: ControllerState.Gamepads[slot].JoyAxis2 = val; break;
                                case 3: ControllerState.Gamepads[slot].JoyAxis3 = val; break;
                            }
                            break;
                        }
                    }
                }

                await Task.Delay(1);
            }
        }

        static int FindSlotByGuid(string guid)
        {
            for (int i = 0; i < 4; i++)
                if (ControllerState.Gamepads[i].Guid == guid)
                    return i;
            return -1;
        }

        static int FindFreeSlot()
        {
            for (int i = 0; i < 4; i++)
                if (!ControllerState.Gamepads[i].Connected)
                    return i;
            return -1;
        }

        static int GetSlotFromInstance(int instanceId)
        {
            IntPtr controller = SDL.SDL_GameControllerFromInstanceID(instanceId);
            for (int i = 0; i < 4; i++)
                if (ControllerState.Gamepads[i].Controller == controller)
                    return i;
            return -1;
        }

        static void SetButton(int id, SDL.SDL_GameControllerButton button, bool state)
        {
            switch (button)
            {
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A:             ControllerState.Gamepads[id].A               = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B:             ControllerState.Gamepads[id].B               = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X:             ControllerState.Gamepads[id].X               = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y:             ControllerState.Gamepads[id].Y               = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START:         ControllerState.Gamepads[id].Start           = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK:          ControllerState.Gamepads[id].Select          = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER:  ControllerState.Gamepads[id].LeftBumper      = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER: ControllerState.Gamepads[id].RightBumper     = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK:     ControllerState.Gamepads[id].LeftStickClick  = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK:    ControllerState.Gamepads[id].RightStickClick = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP:       ControllerState.Gamepads[id].DpadUp          = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN:     ControllerState.Gamepads[id].DpadDown        = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT:     ControllerState.Gamepads[id].DpadLeft        = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT:    ControllerState.Gamepads[id].DpadRight       = state; break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE:         ControllerState.Gamepads[id].Guide           = state; break;
            }
        }
    }

public static class Osc
{
    private static OSCSender _sender = new OSCSender();
    private static bool _connected = false;

    public static async Task OscHandling()
    {
        while (true)
        {
            if (!_connected)
            {
                try
                {
                    _sender = new OSCSender(); // fresh instance
                    await _sender.ConnectAsync(new IPEndPoint(IPAddress.Parse(Config.TargetIp), Config.TargetPort));
                    _connected = true;
                    Console.WriteLine($"OSC connected to {Config.TargetIp}:{Config.TargetPort}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OSC connect failed, retrying in 1s: {e.Message}");
                    await Task.Delay(1000);
                    continue;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                var g = ControllerState.Gamepads[i];
                if (!g.Connected) continue;

                string prefix = $"{Config.AddressPrefix}{i}";

                // Buttons
                Send($"{prefix}/a",               g.A               ? 1f : 0f);
                Send($"{prefix}/b",               g.B               ? 1f : 0f);
                Send($"{prefix}/x",               g.X               ? 1f : 0f);
                Send($"{prefix}/y",               g.Y               ? 1f : 0f);
                Send($"{prefix}/start",           g.Start           ? 1f : 0f);
                Send($"{prefix}/select",          g.Select          ? 1f : 0f);
                Send($"{prefix}/guide",           g.Guide           ? 1f : 0f);
                Send($"{prefix}/leftBumper",      g.LeftBumper      ? 1f : 0f);
                Send($"{prefix}/rightBumper",     g.RightBumper     ? 1f : 0f);
                Send($"{prefix}/leftStickClick",  g.LeftStickClick  ? 1f : 0f);
                Send($"{prefix}/rightStickClick", g.RightStickClick ? 1f : 0f);
                Send($"{prefix}/dpadUp",          g.DpadUp          ? 1f : 0f);
                Send($"{prefix}/dpadDown",        g.DpadDown        ? 1f : 0f);
                Send($"{prefix}/dpadLeft",        g.DpadLeft        ? 1f : 0f);
                Send($"{prefix}/dpadRight",       g.DpadRight       ? 1f : 0f);

                // Axes
                Send($"{prefix}/leftX",        g.LeftX);
                Send($"{prefix}/leftY",        g.LeftY);
                Send($"{prefix}/rightX",       g.RightX);
                Send($"{prefix}/rightY",       g.RightY);
                Send($"{prefix}/leftTrigger",  g.LeftTrigger);
                Send($"{prefix}/rightTrigger", g.RightTrigger);

                // Raw joystick
                Send($"{prefix}/joyAxis0", g.JoyAxis0);
                Send($"{prefix}/joyAxis1", g.JoyAxis1);
                Send($"{prefix}/joyAxis2", g.JoyAxis2);
                Send($"{prefix}/joyAxis3", g.JoyAxis3);
            }

            await Task.Delay(Config.SendRateMs);
        }
    }

    static async void Send(string address, float value)
    {
        try
        {
            await _sender.Send(new OSCMessage(address, value));
        }
        catch (Exception e)
        {
            Console.WriteLine($"OSC send failed, reconnecting: {e.Message}");
            _connected = false; // triggers reconnect on next loop
        }
    }
}
}
