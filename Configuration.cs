using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using Num = System.Numerics;

namespace WheresWOLdo
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public ImColor Col { get; set; } = new ImColor { Value = new Num.Vector4(1f, 1f, 1f, 1f) };
        public float Scale { get; set; } = 1f;
        public bool NoMove { get; set; }
        public int Align { get; set; }

        public bool showLocationInNative { get; set; } = true;
        [JsonIgnore]
        public bool ShowLocationInNative
        {
            get => showLocationInNative;
            set
            {
                plugin.SetDtrBarEntry(value);
                showLocationInNative = value;
            }
        }
        [NonSerialized] private WheresWOLdo plugin;

        // Add any other properties or methods here.
        [JsonIgnore] private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface, WheresWOLdo plugin)
        {
            this.pluginInterface = pluginInterface;
            this.plugin = plugin;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
