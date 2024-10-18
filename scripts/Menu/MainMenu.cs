using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : MenuScreen
{
	private Label _title;

	private CheckButton _shaderToggle;
	private Button _host;
	private Button _join;


	private string[] _prefixes = new[] 
	{ 
		"new", "super", "mini", "power", "tony hawks", "ultra"
	};
	private string[] _suffixes = new[] 
	{ 
		"deluxe", "and knuckles", "OLED", "director's cut", "(new funky mode)", 
		"(featuring dante from the devil may cry series)", "pro", "all-stars", "lite",
		"xl", "u", "pocket", "advance", "sp", "color", "64", "at the olympic games",
		"& watch", "(intel inside)", "i", "bros", "arcade version", "[beta]", "[alpha]", "& kazooie", "battle royale", "for the nintendo ds",
		"GOTY edition", "welcome amiibo", "ultimate"
	};

	public override void _Ready()
	{
		_title = GetNode<Label>("title");

		_shaderToggle = GetNode<CheckButton>("shadertoggle");
		_host = GetNode<Button>("CenterContainer/HBoxContainer/host");
		_join = GetNode<Button>("CenterContainer/HBoxContainer/join");


		string pref = BuildTitlePart(ref _prefixes);
		string suff = BuildTitlePart(ref _suffixes);
		int number = Random.Shared.Next(0, 20);

		string text = pref + "\n**KART RACER**\n " + suff + number;
		_title.Text = Linebreak(text, 40);

		_shaderToggle.Toggled += v => MenuManager.Instance.MenuShaderEnabled = v;
        _host.Pressed += _host_Pressed;
        _join.Pressed += _join_Pressed;
	}

    private string BuildTitlePart(ref string[] parts)
	{
		Random r = new Random();
        string s = "";
		List<string> list = new List<string>(parts);
        for (int i = 0; i < Math.Min(r.Next(1, parts.Length), 12); i++)
        {
			var part = list[r.Next(0, list.Count)];
			list.Remove(part);
			s += part + " ";
        }

		return s;
    }

    private string Linebreak(string s, int linesize)
    {
		string n = "";
		bool breakAsap = false;
		int linebreakCounter = 0;
		for (int i = 0; i < s.Length; i++)
		{
			n += s[i];
			if (i > 0 && linebreakCounter == linesize)
			{
				breakAsap = true;
			}
			if (breakAsap && s[i] == ' ')
			{
				n += '\n';
				breakAsap = false;
                linebreakCounter = 0;
            }
			if (s[i] == '\n')
			{
				linebreakCounter = 0;
			}
            linebreakCounter += 1;
        }

		return n;
    }


    private void _join_Pressed()
    {
        GetTree().ChangeSceneToPacked(MenuManager.Instance.JoinMenu);
    }

    private void _host_Pressed()
    {
		GetTree().ChangeSceneToPacked(MenuManager.Instance.HostMenu);
    }
}
