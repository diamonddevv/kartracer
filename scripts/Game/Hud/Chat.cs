using Godot;
using System.Collections.Generic;

public partial class Chat : VBoxContainer
{
	[Export] public int MaxHistory = 100;
    [Export] public int MaxMsgLength = 200;

    public Queue<Msg> Messages;

	private RichTextLabel _chat;
	private LineEdit _msgBox;

	public struct Msg
	{
		public string From;
		public string Content;
	}

	public override void _Ready()
	{
		_chat = GetNode<RichTextLabel>("Label");
		_msgBox = GetNode<LineEdit>("LineEdit");

		Messages = new Queue<Msg>();

        _msgBox.TextSubmitted += _msgBox_TextSubmitted;

		GlobalManager.Instance.Chat = this;
	}

    public override void _Process(double delta)
    {
		if (Messages.Count > MaxHistory)
		{
			Messages.Dequeue();
        }

		string fullChat = "";
		foreach (var msg in Messages)
		{
			fullChat += $"\n<{msg.From}>: {msg.Content}";
		}
		_chat.Text = fullChat;
    }

    private void _msgBox_TextSubmitted(string text)
    {
        text = text.Limit(MaxMsgLength);
        if (text.Length <= 0) return;

        string currentUsernameOrUid = GlobalManager.Instance.Lobby.LocalKart.NetworkData.Username.GetOrEmptyFallback("Unnamed (UID:" + NetworkManager.Instance.LocalUid + ")");

        NetworkManager.Instance.Send(Packets.Packet_Msg(currentUsernameOrUid, text));
		
		_msgBox.Clear();
		_msgBox.ReleaseFocus();
    }
}
