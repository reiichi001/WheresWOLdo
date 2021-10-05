using Dalamud.Configuration;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using ImGuiNET;
using Num = System.Numerics;
using Lumina.Excel.GeneratedSheets;
using System;

namespace WheresWOLdo
{
    public class WheresWOLdo : IDalamudPlugin
    {
        public string Name => "WOLdo";
        private Config _configuration;
        private string _location = "";
        private bool _enabled = true;
        private Lumina.Excel.ExcelSheet<TerritoryType> _terr;
        private float _scale = 1f;
        private ImColor _col = new ImColor { Value = new Num.Vector4(1f, 1f, 1f, 1f) };
        private bool _noMove;
        private bool _config;
        private bool _debug;
        private int _align;
        private float _adjustX;
        private bool _first = true;

        [PluginService] public static ClientState ClientState { get; private set; }
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static DataManager Data { get; private set; }
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

        public WheresWOLdo()
        {
            _configuration = PluginInterface.GetPluginConfig() as Config ?? new Config();
            _col = _configuration.Col;
            _noMove = _configuration.NoMove;
            _scale = _configuration.Scale;
            _enabled = _configuration.Enabled;
            _align = _configuration.Align;
            _terr = Data.GetExcelSheet<TerritoryType>();
            PluginInterface.UiBuilder.Draw += DrawWindow;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

            CommandManager.AddHandler("/woldo", new CommandInfo(Command)
            {
                HelpMessage = "Where's WOLdo config."
            });
        }

        public void Dispose()
        {
            PluginInterface.UiBuilder.Draw -= DrawWindow;
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
            CommandManager.RemoveHandler("/woldo");
            _terr = null;
        }

        private void Command(string command, string arguments)
        {
            _config = true;
        }
        
        private void OpenConfigUi()
        {
            _config = true;
        }

        private void DrawWindow()
        {
            ImGuiWindowFlags windowFlags = 0;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoScrollbar;
            windowFlags |= ImGuiWindowFlags.NoScrollbar;
            if (_noMove)
            {
                windowFlags |= ImGuiWindowFlags.NoMove;
                windowFlags |= ImGuiWindowFlags.NoMouseInputs;
                windowFlags |= ImGuiWindowFlags.NoNav;
            }
            windowFlags |= ImGuiWindowFlags.AlwaysAutoResize;
            if (!_debug)
            {
                windowFlags |= ImGuiWindowFlags.NoBackground;
            }


            if (_config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(200, 160), ImGuiCond.FirstUseEver);
                ImGui.Begin("Where's WOLdo Config", ref _config, ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Checkbox("Enabled", ref _enabled);
                ImGui.ColorEdit4("Colour", ref _col.Value, ImGuiColorEditFlags.NoInputs);
                ImGui.InputFloat("Size", ref _scale);
                ImGui.Checkbox("Locked", ref _noMove);
                ImGui.Checkbox("Debug", ref _debug);
                //ImGui.ListBox("Alignment", ref align, alignStr, 3);

                if (ImGui.Button("Save and Close Config"))
                {
                    SaveConfig();
                    _config = false;
                }
                ImGui.End();
            }

            _location = "";
            if (ClientState.LocalPlayer != null)
            {
                _location = "Uhoh";
                try
                {
                    _location = _terr.GetRow(ClientState.TerritoryType).PlaceName.Value.Name;
                }
                catch (Exception)
                {
                    _location = "Change zone to load";
                }
            }


            if (!_enabled) return;
            ImGui.PushStyleColor(ImGuiCol.Text, _col.Value);
            ImGui.Begin("WOLdo", ref _enabled, windowFlags);
            ImGui.SetWindowFontScale(_scale);
                
            if (_debug)
            {
                ImGui.SetWindowPos(new Num.Vector2(ImGui.GetWindowPos().X + _adjustX, ImGui.GetWindowPos().Y));
                if (_align == 0)
                {
                    _adjustX = 0;
                    ImGui.Text("Left Align");
                }
                if (_align == 1)
                {
                    _adjustX = (float)Math.Floor((ImGui.CalcTextSize("Middle Align").X) / 2);
                    ImGui.SetWindowPos(new Num.Vector2(ImGui.GetWindowPos().X - _adjustX, ImGui.GetWindowPos().Y));
                    ImGui.Text("Middle Align");
                }
                if (_align == 2)
                {
                    _adjustX = ImGui.CalcTextSize("Right Align").X;
                    ImGui.SetWindowPos(new Num.Vector2(ImGui.GetWindowPos().X - _adjustX, ImGui.GetWindowPos().Y));
                    ImGui.Text("Right Align");
                }
            }
            else
            {
                ImGui.SetWindowPos(new Num.Vector2(ImGui.GetWindowPos().X + _adjustX, ImGui.GetWindowPos().Y));
                if (_align == 0)
                {
                    _adjustX = 0;
                }
                if (_align == 1)
                {
                    _adjustX = (float)Math.Floor((ImGui.CalcTextSize(_location).X) / 2);
                    ImGui.SetWindowPos(new Num.Vector2(ImGui.GetWindowPos().X - _adjustX, ImGui.GetWindowPos().Y));
                }
                if (_align == 2)
                {
                    _adjustX = ImGui.CalcTextSize(_location).X;

                    if (_first)
                    {
                        if (Math.Abs(_adjustX) > 0) { _first = false; }
                    }
                    else
                    {
                        ImGui.SetWindowPos(new Num.Vector2(ImGui.GetWindowPos().X - _adjustX, ImGui.GetWindowPos().Y));
                    }
                            
                }
                ImGui.Text(_location);

            }
            ImGui.End();
            ImGui.PopStyleColor();

        }

        private void SaveConfig()
        {
            _configuration.Enabled = _enabled;
            _configuration.Col = _col;
            _configuration.Scale = _scale;
            _configuration.NoMove = _noMove;
            _configuration.Align = _align;
            PluginInterface.SavePluginConfig(_configuration);
        }
    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public ImColor Col { get; set; } = new ImColor { Value = new Num.Vector4(1f,1f,1f,1f) };
        public float Scale { get; set; } = 1f;
        public bool NoMove { get; set; }
        public int Align { get; set; }
    }
}