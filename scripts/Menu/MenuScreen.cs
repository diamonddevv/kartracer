using Godot;
using System;
using System.Collections.Generic;

public partial class MenuScreen : Control
{
    private const string SHADER_TEXTURE_RECT = "shadertexture";


    public override void _Process(double delta)
    {
        if (HasNode(SHADER_TEXTURE_RECT))
        {
            GetNode<TextureRect>(SHADER_TEXTURE_RECT).Visible = MenuManager.Instance.MenuShaderEnabled;
        }
    }
}
