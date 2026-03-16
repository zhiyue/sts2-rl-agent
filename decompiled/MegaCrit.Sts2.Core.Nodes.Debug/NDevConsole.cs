using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NDevConsole.cs")]
public class NDevConsole : Panel
{
	public new class MethodName : Panel.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName PrintUsage = "PrintUsage";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName HandleReadlineKeybinding = "HandleReadlineKeybinding";

		public static readonly StringName DeleteWordBackward = "DeleteWordBackward";

		public static readonly StringName KillToEndOfLine = "KillToEndOfLine";

		public static readonly StringName Yank = "Yank";

		public static readonly StringName EnableTabBuffer = "EnableTabBuffer";

		public static readonly StringName DisableTabBuffer = "DisableTabBuffer";

		public static readonly StringName SetBackgroundColor = "SetBackgroundColor";

		public static readonly StringName HideGhostText = "HideGhostText";

		public static readonly StringName ShowGhostText = "ShowGhostText";

		public static readonly StringName UpdateGhostText = "UpdateGhostText";

		public static readonly StringName AutocompleteCommand = "AutocompleteCommand";

		public static readonly StringName RenderSelectionMenu = "RenderSelectionMenu";

		public static readonly StringName OnInputTextChanged = "OnInputTextChanged";

		public static readonly StringName ExitSelectionMode = "ExitSelectionMode";

		public static readonly StringName NavigateSelection = "NavigateSelection";

		public static readonly StringName AcceptSelection = "AcceptSelection";

		public static readonly StringName ProcessCommand = "ProcessCommand";

		public static readonly StringName ShowConsole = "ShowConsole";

		public static readonly StringName HideConsole = "HideConsole";

		public static readonly StringName MakeHalfScreen = "MakeHalfScreen";

		public static readonly StringName MakeFullScreen = "MakeFullScreen";

		public static readonly StringName OnToggleMaximizeButtonPressed = "OnToggleMaximizeButtonPressed";

		public static readonly StringName MoveInputCursorToEndOfLine = "MoveInputCursorToEndOfLine";

		public static readonly StringName UpdatePromptStyle = "UpdatePromptStyle";

		public static readonly StringName AddChildToTree = "AddChildToTree";
	}

	public new class PropertyName : Panel.PropertyName
	{
		public static readonly StringName _outputBuffer = "_outputBuffer";

		public static readonly StringName _tabBuffer = "_tabBuffer";

		public static readonly StringName _inputContainer = "_inputContainer";

		public static readonly StringName _inputBuffer = "_inputBuffer";

		public static readonly StringName _promptLabel = "_promptLabel";

		public static readonly StringName _ghostTextLabel = "_ghostTextLabel";

		public static readonly StringName _isFullscreen = "_isFullscreen";

		public static readonly StringName _yankBuffer = "_yankBuffer";
	}

	public new class SignalName : Panel.SignalName
	{
	}

	private static NDevConsole? _instance;

	private RichTextLabel _outputBuffer;

	private RichTextLabel _tabBuffer;

	private Control _inputContainer;

	private LineEdit _inputBuffer;

	private Label _promptLabel;

	private Label _ghostTextLabel;

	private bool _isFullscreen;

	private MegaCrit.Sts2.Core.DevConsole.DevConsole _devConsole;

	private const float _inputBufferSizeY = 40f;

	private readonly TabCompletionState _tabCompletion = new TabCompletionState();

	private string _yankBuffer = string.Empty;

	public static NDevConsole Instance => _instance ?? throw new InvalidOperationException("Dev console used before being created.");

	public override void _Ready()
	{
		if (TestMode.IsOn)
		{
			this.QueueFreeSafely();
			return;
		}
		if (_instance != null)
		{
			this.QueueFreeSafely();
			return;
		}
		_instance = this;
		bool shouldAllowDebugCommands = OS.HasFeature("editor") || TestMode.IsOn || ModManager.LoadedMods.Count > 0 || SaveManager.Instance.SettingsSave.FullConsole;
		HideConsole();
		_devConsole = new MegaCrit.Sts2.Core.DevConsole.DevConsole(shouldAllowDebugCommands);
		_outputBuffer = GetNode<RichTextLabel>("OutputContainer/OutputBuffer");
		_tabBuffer = GetNode<RichTextLabel>("OutputContainer/TabBuffer");
		_inputContainer = GetNode<Control>("InputContainer");
		_inputBuffer = GetNode<LineEdit>("InputContainer/InputBufferContainer/InputBuffer");
		_promptLabel = GetNode<Label>("InputContainer/PromptLabel");
		_ghostTextLabel = GetNode<Label>("InputContainer/InputBufferContainer/GhostText");
		MakeHalfScreen();
		DisableTabBuffer();
		HideGhostText();
		_inputBuffer.CaretBlink = true;
		UpdatePromptStyle();
		_inputBuffer.TextChanged += OnInputTextChanged;
		PrintUsage();
	}

	public override void _ExitTree()
	{
		if (TestMode.IsOff)
		{
			_inputBuffer.TextChanged -= OnInputTextChanged;
		}
		base._ExitTree();
	}

	private void PrintUsage()
	{
		_outputBuffer.Text += "[color=#888888]Use 'F11' to toggle console fullscreen. Press 'up arrow' to use the last command. You can autocomplete commands with 'tab'.[/color]";
		_outputBuffer.Text += "\n\n";
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!(inputEvent is InputEventKey { Pressed: not false, Keycode: var keycode } inputEventKey))
		{
			return;
		}
		bool flag;
		switch (keycode)
		{
		case Key.Apostrophe:
		case Key.Asterisk:
		case Key.Asciicircum:
		case Key.Quoteleft:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag || (inputEventKey.IsShiftPressed() && inputEventKey.Keycode == Key.Key8))
		{
			if (base.Visible)
			{
				HideConsole();
			}
			else
			{
				Control control = GetViewport().GuiGetFocusOwner();
				if ((!(control is TextEdit) && !(control is LineEdit)) || 1 == 0)
				{
					ShowConsole();
				}
			}
		}
		if (!base.Visible)
		{
			return;
		}
		if (inputEventKey.Keycode == Key.Escape)
		{
			if (_tabCompletion.InSelectionMode)
			{
				ExitSelectionMode();
			}
			else
			{
				HideConsole();
			}
		}
		else if (inputEventKey.Keycode == Key.F11)
		{
			OnToggleMaximizeButtonPressed();
		}
		else if (inputEventKey.Keycode == Key.Tab)
		{
			if (_tabCompletion.InSelectionMode)
			{
				NavigateSelection(1);
			}
			else
			{
				AutocompleteCommand();
			}
		}
		else if (inputEventKey.Keycode == Key.Up)
		{
			if (_tabCompletion.InSelectionMode)
			{
				NavigateSelection(-1);
			}
			else if (_devConsole.historyIndex < _devConsole.history.Count)
			{
				_tabCompletion.ProgrammaticTextChange = true;
				string text = _devConsole.history[_devConsole.historyIndex];
				_inputBuffer.Text = text;
				if (_devConsole.historyIndex < _devConsole.history.Count - 1)
				{
					_devConsole.historyIndex++;
					while (_devConsole.historyIndex < _devConsole.history.Count - 1 && _devConsole.history[_devConsole.historyIndex] == text)
					{
						_devConsole.historyIndex++;
					}
				}
				MoveInputCursorToEndOfLine();
			}
			GetViewport().SetInputAsHandled();
		}
		else if (inputEventKey.Keycode == Key.Down)
		{
			if (_tabCompletion.InSelectionMode)
			{
				NavigateSelection(1);
			}
			else if (_devConsole.historyIndex < _devConsole.history.Count)
			{
				_tabCompletion.ProgrammaticTextChange = true;
				string text2 = _devConsole.history[_devConsole.historyIndex];
				_inputBuffer.Text = text2;
				if (_devConsole.historyIndex > 0)
				{
					_devConsole.historyIndex--;
					while (_devConsole.historyIndex > 0 && _devConsole.history[_devConsole.historyIndex] == text2)
					{
						_devConsole.historyIndex--;
					}
				}
				MoveInputCursorToEndOfLine();
			}
			GetViewport().SetInputAsHandled();
		}
		else if (inputEventKey.Keycode == Key.Enter)
		{
			if (_tabCompletion.InSelectionMode)
			{
				AcceptSelection();
			}
			else
			{
				ProcessCommand();
			}
		}
		else if (inputEventKey.IsCtrlPressed())
		{
			HandleReadlineKeybinding(inputEventKey);
		}
	}

	private void HandleReadlineKeybinding(InputEventKey keyEvent)
	{
		Key keycode = keyEvent.Keycode;
		if (keycode <= Key.K)
		{
			Key num = keycode - 65;
			if ((ulong)num <= 4uL)
			{
				switch (num)
				{
				case Key.None:
					GetViewport().SetInputAsHandled();
					_inputBuffer.CaretColumn = 0;
					_inputBuffer.CallDeferred(LineEdit.MethodName.Deselect);
					return;
				case (Key)4L:
					GetViewport().SetInputAsHandled();
					MoveInputCursorToEndOfLine();
					return;
				case (Key)2L:
					GetViewport().SetInputAsHandled();
					_inputBuffer.Text = string.Empty;
					ExitSelectionMode();
					return;
				case (Key)3L:
					GetViewport().SetInputAsHandled();
					HideConsole();
					return;
				case (Key)1L:
					return;
				}
			}
			if (keycode == Key.K)
			{
				GetViewport().SetInputAsHandled();
				KillToEndOfLine();
			}
		}
		else if (keycode != Key.L)
		{
			Key num2 = keycode - 85;
			if ((ulong)num2 <= 4uL)
			{
				switch (num2)
				{
				case (Key)2L:
					GetViewport().SetInputAsHandled();
					DeleteWordBackward();
					break;
				case Key.None:
					GetViewport().SetInputAsHandled();
					_yankBuffer = _inputBuffer.Text;
					_inputBuffer.Text = string.Empty;
					ExitSelectionMode();
					break;
				case (Key)4L:
					GetViewport().SetInputAsHandled();
					Yank();
					break;
				case (Key)1L:
				case (Key)3L:
					break;
				}
			}
		}
		else
		{
			GetViewport().SetInputAsHandled();
			_outputBuffer.Text = string.Empty;
		}
	}

	private void DeleteWordBackward()
	{
		string text = _inputBuffer.Text;
		int caretColumn = _inputBuffer.CaretColumn;
		if (caretColumn != 0 && !string.IsNullOrEmpty(text))
		{
			int num = caretColumn - 1;
			while (num >= 0 && char.IsWhiteSpace(text[num]))
			{
				num--;
			}
			while (num >= 0 && !char.IsWhiteSpace(text[num]))
			{
				num--;
			}
			num++;
			_yankBuffer = text.Substring(num, caretColumn - num);
			string text2 = text.Substring(0, num) + text.Substring(caretColumn);
			_tabCompletion.ProgrammaticTextChange = true;
			_inputBuffer.Text = text2;
			_inputBuffer.CaretColumn = num;
		}
	}

	private void KillToEndOfLine()
	{
		string text = _inputBuffer.Text;
		int caretColumn = _inputBuffer.CaretColumn;
		if (caretColumn < text.Length)
		{
			_yankBuffer = text.Substring(caretColumn);
			string text2 = text.Substring(0, caretColumn);
			_tabCompletion.ProgrammaticTextChange = true;
			_inputBuffer.Text = text2;
			_inputBuffer.CaretColumn = caretColumn;
		}
	}

	private void Yank()
	{
		if (!string.IsNullOrEmpty(_yankBuffer))
		{
			string text = _inputBuffer.Text;
			int caretColumn = _inputBuffer.CaretColumn;
			string text2 = text.Insert(caretColumn, _yankBuffer);
			_tabCompletion.ProgrammaticTextChange = true;
			_inputBuffer.Text = text2;
			_inputBuffer.CaretColumn = caretColumn + _yankBuffer.Length;
		}
	}

	private void EnableTabBuffer()
	{
		_outputBuffer.Visible = false;
		_tabBuffer.Visible = true;
	}

	private void DisableTabBuffer()
	{
		_outputBuffer.Visible = true;
		_tabBuffer.Visible = false;
	}

	public void SetBackgroundColor(Color color)
	{
		base.Modulate = color;
	}

	private void HideGhostText()
	{
		_ghostTextLabel.Visible = false;
		_ghostTextLabel.Text = string.Empty;
	}

	private void ShowGhostText(string ghostText)
	{
		_ghostTextLabel.Text = ghostText;
		_ghostTextLabel.Visible = true;
	}

	private void UpdateGhostText()
	{
		if (_tabCompletion.InSelectionMode)
		{
			HideGhostText();
			return;
		}
		string text = _inputBuffer.Text;
		if (string.IsNullOrWhiteSpace(text))
		{
			HideGhostText();
			return;
		}
		CompletionResult completionResults = _devConsole.GetCompletionResults(text);
		if (completionResults.Candidates.Count == 1 && !string.IsNullOrEmpty(completionResults.CommonPrefix))
		{
			string commonPrefix = completionResults.CommonPrefix;
			if (!commonPrefix.StartsWith(text, StringComparison.OrdinalIgnoreCase))
			{
				GD.PushError($"BUG: CommonPrefix '{commonPrefix}' doesn't start with input '{text}'");
				HideGhostText();
			}
			else
			{
				string text2 = commonPrefix.Substring(text.Length);
				string text3 = new string(' ', text.Length);
				ShowGhostText(text3 + text2);
			}
		}
		else
		{
			HideGhostText();
		}
	}

	private void AutocompleteCommand()
	{
		string text = _inputBuffer.Text;
		CompletionResult completionResults = _devConsole.GetCompletionResults(text);
		_tabCompletion.LastCompletionResult = completionResults;
		if (string.IsNullOrWhiteSpace(text) && completionResults.Candidates.Count == 0)
		{
			completionResults = _devConsole.GetCompletionResults("");
			_tabCompletion.LastCompletionResult = completionResults;
		}
		if (completionResults.Candidates.Count == 0)
		{
			ExitSelectionMode();
		}
		else if (completionResults.Candidates.Count == 1)
		{
			_tabCompletion.ProgrammaticTextChange = true;
			_inputBuffer.Text = completionResults.CommonPrefix;
			MoveInputCursorToEndOfLine();
			ExitSelectionMode();
		}
		else
		{
			_tabCompletion.CompletionCandidates.Clear();
			_tabCompletion.CompletionCandidates.AddRange(completionResults.Candidates);
			_tabCompletion.InSelectionMode = true;
			_tabCompletion.SelectionIndex = 0;
			HideGhostText();
			RenderSelectionMenu();
			MoveInputCursorToEndOfLine();
		}
	}

	private void RenderSelectionMenu()
	{
		if (!_tabCompletion.InSelectionMode || _tabCompletion.CompletionCandidates.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>();
		list.Add(_tabCompletion.LastCompletionResult?.Type switch
		{
			CompletionType.Command => "Select command:", 
			CompletionType.Subcommand => "Select " + _tabCompletion.LastCompletionResult.ArgumentContext + " action:", 
			CompletionType.Argument => "Select " + _tabCompletion.LastCompletionResult.ArgumentContext + " argument:", 
			_ => "Select option:", 
		});
		list.Add("[color=gray]Tab/↑↓: navigate, Enter: accept, Esc: cancel, Type to filter[/color]");
		list.Add("");
		int count = _tabCompletion.CompletionCandidates.Count;
		if (count <= 12)
		{
			for (int i = 0; i < count; i++)
			{
				AddCandidateToDisplay(list, i);
			}
		}
		else
		{
			int num = Math.Max(0, _tabCompletion.SelectionIndex - 6);
			int num2 = Math.Min(count, num + 12);
			if (num2 - num < 12)
			{
				num = Math.Max(0, num2 - 12);
			}
			if (num > 0)
			{
				list.Add($"[color=gray]↑ {num} more above ↑[/color]");
			}
			for (int j = num; j < num2; j++)
			{
				AddCandidateToDisplay(list, j);
			}
			if (num2 < count)
			{
				int value = count - num2;
				list.Add($"[color=gray]↓ {value} more below ↓[/color]");
			}
		}
		list.Add("");
		list.Add($"[color=gray]({_tabCompletion.CompletionCandidates.Count} matches)[/color]");
		_tabBuffer.Text = string.Join("\n", list);
		EnableTabBuffer();
	}

	private void AddCandidateToDisplay(List<string> displayLines, int index)
	{
		if (index >= 0 && index < _tabCompletion.CompletionCandidates.Count)
		{
			string text = _tabCompletion.CompletionCandidates[index];
			string item = ((index == _tabCompletion.SelectionIndex) ? ("[color=yellow]➜ " + text + "[/color]") : ("  " + text));
			displayLines.Add(item);
		}
	}

	private void OnInputTextChanged(string newText)
	{
		bool programmaticTextChange = _tabCompletion.ProgrammaticTextChange;
		_tabCompletion.ProgrammaticTextChange = false;
		if (_tabCompletion.InSelectionMode && !programmaticTextChange)
		{
			CompletionResult completionResults = _devConsole.GetCompletionResults(newText);
			_tabCompletion.LastCompletionResult = completionResults;
			if (completionResults.Candidates.Count == 0)
			{
				ExitSelectionMode();
			}
			else if (completionResults.Candidates.Count == 1)
			{
				_tabCompletion.ProgrammaticTextChange = true;
				_inputBuffer.Text = completionResults.CommonPrefix;
				MoveInputCursorToEndOfLine();
				ExitSelectionMode();
			}
			else
			{
				_tabCompletion.CompletionCandidates.Clear();
				_tabCompletion.CompletionCandidates.AddRange(completionResults.Candidates);
				_tabCompletion.SelectionIndex = 0;
				RenderSelectionMenu();
			}
		}
		if (!programmaticTextChange)
		{
			UpdateGhostText();
		}
	}

	private void ExitSelectionMode()
	{
		_tabCompletion.Reset();
		_tabBuffer.Text = string.Empty;
		DisableTabBuffer();
		UpdateGhostText();
	}

	private void NavigateSelection(int direction)
	{
		if (_tabCompletion.InSelectionMode && _tabCompletion.CompletionCandidates.Count != 0)
		{
			_tabCompletion.SelectionIndex = (_tabCompletion.SelectionIndex + direction + _tabCompletion.CompletionCandidates.Count) % _tabCompletion.CompletionCandidates.Count;
			RenderSelectionMenu();
		}
	}

	private void AcceptSelection()
	{
		if (!_tabCompletion.InSelectionMode || _tabCompletion.SelectionIndex < 0 || _tabCompletion.SelectionIndex >= _tabCompletion.CompletionCandidates.Count)
		{
			return;
		}
		string text = _tabCompletion.CompletionCandidates[_tabCompletion.SelectionIndex];
		_tabCompletion.ProgrammaticTextChange = true;
		string text2 = _inputBuffer.Text;
		CompletionResult lastCompletionResult = _tabCompletion.LastCompletionResult;
		if (lastCompletionResult != null)
		{
			switch (lastCompletionResult.Type)
			{
			case CompletionType.Command:
				_inputBuffer.Text = text + " ";
				break;
			case CompletionType.Subcommand:
			case CompletionType.Argument:
			{
				string text3 = (text.Contains(' ') ? ("\"" + text + "\"") : text);
				if (text2.EndsWith(' '))
				{
					_inputBuffer.Text = lastCompletionResult.CommandPrefix + text3 + " ";
				}
				else
				{
					_inputBuffer.Text = lastCompletionResult.CommandPrefix + text3 + " ";
				}
				break;
			}
			}
		}
		else
		{
			string[] array = text2.Trim().Split(' ');
			string[] array2 = ((array.Length > 1) ? array.Take(array.Length - 1).ToArray() : Array.Empty<string>());
			string text4 = ((array2.Length != 0) ? (string.Join(" ", array2) + " ") : string.Empty);
			_inputBuffer.Text = text4 + text + " ";
		}
		MoveInputCursorToEndOfLine();
		ExitSelectionMode();
	}

	private void ProcessCommand()
	{
		if (string.IsNullOrWhiteSpace(_inputBuffer.Text))
		{
			return;
		}
		RichTextLabel outputBuffer = _outputBuffer;
		outputBuffer.Text = outputBuffer.Text + "[color=#00ff00]➜[/color] " + _inputBuffer.Text + "\n";
		if (_inputBuffer.Text.Trim().Equals("clear"))
		{
			_outputBuffer.Text = string.Empty;
			_inputBuffer.Text = string.Empty;
			return;
		}
		if (_inputBuffer.Text.Trim().Equals("exit"))
		{
			HideConsole();
			return;
		}
		Exception ex = null;
		CmdResult cmdResult;
		try
		{
			cmdResult = _devConsole.ProcessCommand(_inputBuffer.Text);
		}
		catch (Exception ex2)
		{
			cmdResult = new CmdResult(success: false, $"An exception occurred: {ex2}");
			ex = ex2;
		}
		if (cmdResult.success)
		{
			RichTextLabel outputBuffer2 = _outputBuffer;
			outputBuffer2.Text = outputBuffer2.Text + cmdResult.msg + "\n";
		}
		else
		{
			RichTextLabel outputBuffer3 = _outputBuffer;
			outputBuffer3.Text = outputBuffer3.Text + "[color=#ff5555]⚠ " + cmdResult.msg + "[/color]\n";
		}
		_inputBuffer.Text = string.Empty;
		_tabBuffer.Text = string.Empty;
		DisableTabBuffer();
		HideGhostText();
		if (ex != null)
		{
			ExceptionDispatchInfo.Capture(ex).Throw();
		}
	}

	public Task ProcessNetCommand(Player? player, string netCommand)
	{
		Exception ex = null;
		CmdResult cmdResult;
		try
		{
			cmdResult = _devConsole.ProcessNetCommand(player, netCommand);
		}
		catch (Exception ex2)
		{
			cmdResult = new CmdResult(success: false, $"An exception occurred: {ex2}");
			ex = ex2;
		}
		if (cmdResult.success)
		{
			RichTextLabel outputBuffer = _outputBuffer;
			outputBuffer.Text = outputBuffer.Text + cmdResult.msg + "\n";
		}
		else
		{
			RichTextLabel outputBuffer2 = _outputBuffer;
			outputBuffer2.Text = outputBuffer2.Text + "[color=#ff5555]⚠ " + cmdResult.msg + "[/color]\n";
		}
		if (ex != null)
		{
			ExceptionDispatchInfo.Capture(ex).Throw();
		}
		return cmdResult.task ?? Task.CompletedTask;
	}

	public void ShowConsole()
	{
		base.Visible = true;
		_inputBuffer.CallDeferred(Control.MethodName.GrabFocus);
	}

	public void HideConsole()
	{
		base.Visible = false;
		GetViewport()?.GuiReleaseFocus();
	}

	public void MakeHalfScreen()
	{
		Vector2 size = GetViewportRect().Size;
		float num = size.Y * 0.5f;
		_inputContainer.SetSize(new Vector2(size.X, 40f));
		_inputContainer.Position = new Vector2(0f, num - 40f);
		_outputBuffer.SetSize(new Vector2(size.X, num));
		_tabBuffer.SetSize(new Vector2(size.X, num));
		SetSize(new Vector2(size.X, size.Y / 2f));
		_isFullscreen = false;
	}

	public void MakeFullScreen()
	{
		Vector2 size = GetViewportRect().Size;
		float y = size.Y;
		_inputContainer.SetSize(new Vector2(size.X, 40f));
		_inputContainer.Position = new Vector2(0f, y - 40f);
		_outputBuffer.SetSize(new Vector2(_outputBuffer.Size.X, y - 40f));
		_tabBuffer.SetSize(new Vector2(_outputBuffer.Size.X, y - 40f));
		SetSize(size);
		_isFullscreen = true;
	}

	private void OnToggleMaximizeButtonPressed()
	{
		if (!_isFullscreen)
		{
			MakeFullScreen();
		}
		else
		{
			MakeHalfScreen();
		}
	}

	public void MoveInputCursorToEndOfLine()
	{
		_inputBuffer.CaretColumn = _inputBuffer.Text.Length;
	}

	private void UpdatePromptStyle()
	{
		_promptLabel.Text = "➜";
		_promptLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, new Color(0f, 0.831f, 1f));
		_promptLabel.AddThemeFontSizeOverride(ThemeConstants.Label.fontSize, 18);
	}

	public void AddChildToTree(Node node)
	{
		this.AddChildSafely(node);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(29);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PrintUsage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HandleReadlineKeybinding, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "keyEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEventKey"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DeleteWordBackward, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.KillToEndOfLine, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Yank, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableTabBuffer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableTabBuffer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetBackgroundColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "color", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideGhostText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowGhostText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "ghostText", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateGhostText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AutocompleteCommand, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RenderSelectionMenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnInputTextChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "newText", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ExitSelectionMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.NavigateSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "direction", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AcceptSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ProcessCommand, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowConsole, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideConsole, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MakeHalfScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MakeFullScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnToggleMaximizeButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MoveInputCursorToEndOfLine, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdatePromptStyle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddChildToTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PrintUsage && args.Count == 0)
		{
			PrintUsage();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HandleReadlineKeybinding && args.Count == 1)
		{
			HandleReadlineKeybinding(VariantUtils.ConvertTo<InputEventKey>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DeleteWordBackward && args.Count == 0)
		{
			DeleteWordBackward();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.KillToEndOfLine && args.Count == 0)
		{
			KillToEndOfLine();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Yank && args.Count == 0)
		{
			Yank();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableTabBuffer && args.Count == 0)
		{
			EnableTabBuffer();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableTabBuffer && args.Count == 0)
		{
			DisableTabBuffer();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetBackgroundColor && args.Count == 1)
		{
			SetBackgroundColor(VariantUtils.ConvertTo<Color>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideGhostText && args.Count == 0)
		{
			HideGhostText();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowGhostText && args.Count == 1)
		{
			ShowGhostText(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateGhostText && args.Count == 0)
		{
			UpdateGhostText();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AutocompleteCommand && args.Count == 0)
		{
			AutocompleteCommand();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RenderSelectionMenu && args.Count == 0)
		{
			RenderSelectionMenu();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnInputTextChanged && args.Count == 1)
		{
			OnInputTextChanged(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExitSelectionMode && args.Count == 0)
		{
			ExitSelectionMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NavigateSelection && args.Count == 1)
		{
			NavigateSelection(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AcceptSelection && args.Count == 0)
		{
			AcceptSelection();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessCommand && args.Count == 0)
		{
			ProcessCommand();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowConsole && args.Count == 0)
		{
			ShowConsole();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideConsole && args.Count == 0)
		{
			HideConsole();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MakeHalfScreen && args.Count == 0)
		{
			MakeHalfScreen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MakeFullScreen && args.Count == 0)
		{
			MakeFullScreen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnToggleMaximizeButtonPressed && args.Count == 0)
		{
			OnToggleMaximizeButtonPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MoveInputCursorToEndOfLine && args.Count == 0)
		{
			MoveInputCursorToEndOfLine();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePromptStyle && args.Count == 0)
		{
			UpdatePromptStyle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddChildToTree && args.Count == 1)
		{
			AddChildToTree(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.PrintUsage)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.HandleReadlineKeybinding)
		{
			return true;
		}
		if (method == MethodName.DeleteWordBackward)
		{
			return true;
		}
		if (method == MethodName.KillToEndOfLine)
		{
			return true;
		}
		if (method == MethodName.Yank)
		{
			return true;
		}
		if (method == MethodName.EnableTabBuffer)
		{
			return true;
		}
		if (method == MethodName.DisableTabBuffer)
		{
			return true;
		}
		if (method == MethodName.SetBackgroundColor)
		{
			return true;
		}
		if (method == MethodName.HideGhostText)
		{
			return true;
		}
		if (method == MethodName.ShowGhostText)
		{
			return true;
		}
		if (method == MethodName.UpdateGhostText)
		{
			return true;
		}
		if (method == MethodName.AutocompleteCommand)
		{
			return true;
		}
		if (method == MethodName.RenderSelectionMenu)
		{
			return true;
		}
		if (method == MethodName.OnInputTextChanged)
		{
			return true;
		}
		if (method == MethodName.ExitSelectionMode)
		{
			return true;
		}
		if (method == MethodName.NavigateSelection)
		{
			return true;
		}
		if (method == MethodName.AcceptSelection)
		{
			return true;
		}
		if (method == MethodName.ProcessCommand)
		{
			return true;
		}
		if (method == MethodName.ShowConsole)
		{
			return true;
		}
		if (method == MethodName.HideConsole)
		{
			return true;
		}
		if (method == MethodName.MakeHalfScreen)
		{
			return true;
		}
		if (method == MethodName.MakeFullScreen)
		{
			return true;
		}
		if (method == MethodName.OnToggleMaximizeButtonPressed)
		{
			return true;
		}
		if (method == MethodName.MoveInputCursorToEndOfLine)
		{
			return true;
		}
		if (method == MethodName.UpdatePromptStyle)
		{
			return true;
		}
		if (method == MethodName.AddChildToTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._outputBuffer)
		{
			_outputBuffer = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._tabBuffer)
		{
			_tabBuffer = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._inputContainer)
		{
			_inputContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._inputBuffer)
		{
			_inputBuffer = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._promptLabel)
		{
			_promptLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._ghostTextLabel)
		{
			_ghostTextLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._isFullscreen)
		{
			_isFullscreen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._yankBuffer)
		{
			_yankBuffer = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._outputBuffer)
		{
			value = VariantUtils.CreateFrom(in _outputBuffer);
			return true;
		}
		if (name == PropertyName._tabBuffer)
		{
			value = VariantUtils.CreateFrom(in _tabBuffer);
			return true;
		}
		if (name == PropertyName._inputContainer)
		{
			value = VariantUtils.CreateFrom(in _inputContainer);
			return true;
		}
		if (name == PropertyName._inputBuffer)
		{
			value = VariantUtils.CreateFrom(in _inputBuffer);
			return true;
		}
		if (name == PropertyName._promptLabel)
		{
			value = VariantUtils.CreateFrom(in _promptLabel);
			return true;
		}
		if (name == PropertyName._ghostTextLabel)
		{
			value = VariantUtils.CreateFrom(in _ghostTextLabel);
			return true;
		}
		if (name == PropertyName._isFullscreen)
		{
			value = VariantUtils.CreateFrom(in _isFullscreen);
			return true;
		}
		if (name == PropertyName._yankBuffer)
		{
			value = VariantUtils.CreateFrom(in _yankBuffer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outputBuffer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tabBuffer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inputContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inputBuffer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._promptLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ghostTextLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isFullscreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._yankBuffer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._outputBuffer, Variant.From(in _outputBuffer));
		info.AddProperty(PropertyName._tabBuffer, Variant.From(in _tabBuffer));
		info.AddProperty(PropertyName._inputContainer, Variant.From(in _inputContainer));
		info.AddProperty(PropertyName._inputBuffer, Variant.From(in _inputBuffer));
		info.AddProperty(PropertyName._promptLabel, Variant.From(in _promptLabel));
		info.AddProperty(PropertyName._ghostTextLabel, Variant.From(in _ghostTextLabel));
		info.AddProperty(PropertyName._isFullscreen, Variant.From(in _isFullscreen));
		info.AddProperty(PropertyName._yankBuffer, Variant.From(in _yankBuffer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._outputBuffer, out var value))
		{
			_outputBuffer = value.As<RichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._tabBuffer, out var value2))
		{
			_tabBuffer = value2.As<RichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._inputContainer, out var value3))
		{
			_inputContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._inputBuffer, out var value4))
		{
			_inputBuffer = value4.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._promptLabel, out var value5))
		{
			_promptLabel = value5.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._ghostTextLabel, out var value6))
		{
			_ghostTextLabel = value6.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._isFullscreen, out var value7))
		{
			_isFullscreen = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._yankBuffer, out var value8))
		{
			_yankBuffer = value8.As<string>();
		}
	}
}
