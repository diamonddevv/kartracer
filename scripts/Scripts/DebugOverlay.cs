using Godot;
using System;
using System.Collections.Generic;

public partial class DebugOverlay : CanvasLayer
{
    public static DebugOverlay Instance { get; private set; }
    public Dictionary<string, string> DebugLines { get; set; }

    private Label _label;

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");

        DebugLines = new Dictionary<string, string>();

        DebugLines["gd"] = "Engine: Godot " + Engine.GetVersionInfo()["string"];
        DebugLines["cpu"] = "CPU: " + OS.GetProcessorName();
        DebugLines["gpu"] = "GPU: " + RenderingServer.GetVideoAdapterName();
        DebugLines["arch"] = "System Architecture: " + Engine.GetArchitectureName();
        DebugLines["os"] = "Operating System: " + OS.GetName();
        DebugLines["fps"] = "";

        Visible = false;
        Instance = this;
    }


    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("debug"))
        {
            Visible = !Visible;
        }

        DebugLines["fps"] = "FPS: " + Engine.GetFramesPerSecond();


        string s = "";
        foreach (var line in DebugLines.Values)
        {
            s += line + '\n';
        }

        _label.Text = s;
    }
}
