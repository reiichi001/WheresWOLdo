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
using WheresWOLdo.Attributes;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game;

namespace WheresWOLdo
{
    public class WheresWOLdo : IDalamudPlugin
    {
        private const string ConstName = "WOLdo";
        public string Name => ConstName;
        private Configuration _configuration;
        public PluginCommandManager<WheresWOLdo> CommandManager;

        [PluginService] public static ClientState ClientState { get; private set; }
        [PluginService] public static DataManager Data { get; private set; }
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static DtrBar DtrBar { get; private set; }
        private DtrBarEntry dtrEntry;

        private string _location = "";
        private bool _enabled;
        private float _scale = 1f;
        private ImColor _col = new ImColor { Value = new Num.Vector4(1f, 1f, 1f, 1f) };
        private bool _noMove;
        private bool _config;
        private bool _debug;
        private int _align;
        private float _adjustX;
        private bool _first = true;
        private Lumina.Excel.ExcelSheet<TerritoryType> _terr;

        public WheresWOLdo(CommandManager command)
        {
            _configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this._configuration.Initialize(PluginInterface, this);

            _col = _configuration.Col;
            _noMove = _configuration.NoMove;
            _scale = _configuration.Scale;
            _enabled = _configuration.Enabled;
            _align = _configuration.Align;
            _terr = Data.GetExcelSheet<TerritoryType>();
            Framework.Update += OnFrameworkUpdate;
            PluginInterface.UiBuilder.Draw += DrawWindow;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

            

            this.CommandManager = new PluginCommandManager<WheresWOLdo>(this, command);
        }

        [Command("/woldo")]
        [HelpMessage("Where's WOLdo config.")]
        public void Command(string command, string arguments)
        {
            _config = true;
        }
        
        private void OpenConfigUi()
        {
            _config = true;
        }

        private void OnFrameworkUpdate(Framework framework)
        {
            if (_configuration.ShowLocationInNative)
            {
                dtrEntry ??= DtrBar.Get(ConstName);
                dtrEntry.Shown = _configuration.ShowLocationInNative;
                UpdateDtrBarEntry();
            }
            else {
                if (dtrEntry != null) dtrEntry.Shown = false;
            }
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
                ImGui.Checkbox("Enable moveable location bar", ref _enabled);
                ImGui.ColorEdit4("Colour", ref _col.Value, ImGuiColorEditFlags.NoInputs);
                ImGui.InputFloat("Size", ref _scale);
                ImGui.Checkbox("Locked", ref _noMove);
                ImGui.Checkbox("Debug", ref _debug);
                //ImGui.ListBox("Alignment", ref align, alignStr, 3);
                var showNative = _configuration.ShowLocationInNative;
                if (ImGui.Checkbox("Show current location in the \"server info\" UI element in-game", ref showNative))
                {
                    _configuration.ShowLocationInNative = showNative;
                    _configuration.Save();
                }

                if (ImGui.Button("Save and Close Config"))
                {
                    _configuration.Enabled = _enabled;
                    _configuration.Col = _col;
                    _configuration.Scale = _scale;
                    _configuration.NoMove = _noMove;

                    _configuration.Save();
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

        public void SetDtrBarEntry(bool value)
        {
            // Somehow it was set to the same value. This should not occur
            if (value == _configuration.ShowLocationInNative) return;

            if (value)
            {
                dtrEntry = DtrBar.Get(ConstName);
                dtrEntry.Text = _location;
            }
            else
                dtrEntry.Dispose();
        }

        private void UpdateDtrBarEntry()
        {
            if (!_configuration.ShowLocationInNative) return;

            dtrEntry.Text = _location;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            CommandManager.Dispose();
            dtrEntry?.Dispose();
            Framework.Update -= OnFrameworkUpdate;
            PluginInterface.UiBuilder.Draw -= DrawWindow;
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
            _terr = null;

            PluginInterface.SavePluginConfig(this._configuration);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}