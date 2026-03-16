using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Credits;

[ScriptPath("res://src/Core/Nodes/Screens/Credits/NCreditsScreen.cs")]
public class NCreditsScreen : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName InitMegaCrit = "InitMegaCrit";

		public static readonly StringName InitComposer = "InitComposer";

		public static readonly StringName InitAdditionalProgramming = "InitAdditionalProgramming";

		public static readonly StringName InitAdditionalVfx = "InitAdditionalVfx";

		public static readonly StringName InitMarketingSupport = "InitMarketingSupport";

		public static readonly StringName InitConsultants = "InitConsultants";

		public static readonly StringName InitVoices = "InitVoices";

		public static readonly StringName InitLocalization = "InitLocalization";

		public static readonly StringName InitTwitchExtension = "InitTwitchExtension";

		public static readonly StringName InitModdingSupport = "InitModdingSupport";

		public static readonly StringName InitPlaytesters = "InitPlaytesters";

		public static readonly StringName InitTrailer = "InitTrailer";

		public static readonly StringName InitFmod = "InitFmod";

		public static readonly StringName InitSpine = "InitSpine";

		public static readonly StringName InitGodot = "InitGodot";

		public static readonly StringName InitExitMessage = "InitExitMessage";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName CloseScreenDebug = "CloseScreenDebug";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName ProcessScrollEvent = "ProcessScrollEvent";

		public static readonly StringName ShuffleOneColumn = "ShuffleOneColumn";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _canClose = "_canClose";

		public static readonly StringName _exitingScreen = "_exitingScreen";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _screenContents = "_screenContents";

		public static readonly StringName _backButton = "_backButton";

		public static readonly StringName _targetPosition = "_targetPosition";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/credits_screen");

	private bool _canClose;

	private bool _exitingScreen;

	private Tween? _tween;

	private Control _screenContents;

	private NBackButton _backButton;

	private const string _table = "credits";

	private float _targetPosition;

	private const float _scrollSpeed = 50f;

	private const float _trackpadScrollSpeed = 20f;

	private const float _autoScrollSpeed = 80f;

	private const float _lerpSmoothness = 20f;

	public Control DefaultFocusedControl => this;

	public static NCreditsScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCreditsScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		NHotkeyManager.Instance.PushHotkeyReleasedBinding(MegaInput.back, CloseScreenDebug);
		_screenContents = GetNode<Control>("%ScreenContents");
		_backButton = GetNode<NBackButton>("BackButton");
		_screenContents.Modulate = StsColors.transparentWhite;
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_screenContents, "modulate", Colors.White, 2.0);
		_targetPosition = _screenContents.Position.Y;
		TaskHelper.RunSafely(EnableScreenExit());
		InitMegaCrit();
		InitComposer();
		InitAdditionalProgramming();
		InitAdditionalVfx();
		InitMarketingSupport();
		InitConsultants();
		InitVoices();
		InitLocalization();
		InitTwitchExtension();
		InitModdingSupport();
		InitPlaytesters();
		InitTrailer();
		InitFmod();
		InitSpine();
		InitGodot();
		InitExitMessage();
	}

	private void InitMegaCrit()
	{
		GetNode<MegaLabel>("%MegaCritHeader").Text = new LocString("credits", "MEGA_CRIT.header").GetRawText();
		GetNode<MegaRichTextLabel>("%CreatedByNames").Text = new LocString("credits", "MEGA_CRIT.names").GetRawText();
		var (text, text2) = SplitTwoColumn(new LocString("credits", "MEGA_CRIT_TEAM.names").GetRawText());
		GetNode<MegaRichTextLabel>("%MegaCritTeamRoles").Text = text;
		GetNode<MegaRichTextLabel>("%MegaCritTeamNames").Text = text2;
	}

	private void InitComposer()
	{
		GetNode<MegaLabel>("%ComposerHeader").Text = new LocString("credits", "COMPOSER.header").GetRawText();
		GetNode<MegaRichTextLabel>("%ComposerNames").Text = new LocString("credits", "COMPOSER.names").GetRawText();
	}

	private void InitAdditionalProgramming()
	{
		GetNode<MegaLabel>("%AdditionalProgrammingHeader").Text = new LocString("credits", "ADDITIONAL_PROGRAMMING.header").GetRawText();
		GetNode<MegaRichTextLabel>("%AdditionalProgrammingNames").Text = new LocString("credits", "ADDITIONAL_PROGRAMMING.names").GetRawText();
	}

	private void InitAdditionalVfx()
	{
		GetNode<MegaLabel>("%AdditionalVfxHeader").Text = new LocString("credits", "ADDITIONAL_VFX.header").GetRawText();
		GetNode<MegaRichTextLabel>("%AdditionalVfxNames").Text = new LocString("credits", "ADDITIONAL_VFX.names").GetRawText();
	}

	private void InitMarketingSupport()
	{
		GetNode<MegaLabel>("%MarketingSupportHeader").Text = new LocString("credits", "MARKETING_SUPPORT.header").GetRawText();
		GetNode<MegaRichTextLabel>("%MarketingSupportNames").Text = new LocString("credits", "MARKETING_SUPPORT.names").GetRawText();
	}

	private void InitConsultants()
	{
		GetNode<MegaLabel>("%ConsultantsHeader").Text = new LocString("credits", "CONSULTANTS.header").GetRawText();
		GetNode<MegaRichTextLabel>("%ConsultantsNames").Text = new LocString("credits", "CONSULTANTS.names").GetRawText();
	}

	private void InitVoices()
	{
		GetNode<MegaLabel>("%VoicesHeader").Text = new LocString("credits", "VOICES.header").GetRawText();
		var (text, text2) = SplitTwoColumnMultiRole(new LocString("credits", "VOICES.names").GetRawText());
		GetNode<MegaRichTextLabel>("%VoicesRoles").Text = text;
		GetNode<MegaRichTextLabel>("%VoicesNames").Text = text2;
	}

	private void InitLocalization()
	{
		GetNode<MegaLabel>("%LocalizationHeader").Text = new LocString("credits", "LOC.header").GetRawText();
		GetNode<MegaLabel>("%ptbHeader").Text = new LocString("credits", "LOC_PTB.header").GetRawText();
		GetNode<MegaRichTextLabel>("%ptbNames").Text = new LocString("credits", "LOC_PTB.names").GetRawText();
		GetNode<MegaLabel>("%zhsHeader").Text = new LocString("credits", "LOC_ZHS.header").GetRawText();
		GetNode<MegaRichTextLabel>("%zhsNames").Text = new LocString("credits", "LOC_ZHS.names").GetRawText();
		GetNode<MegaLabel>("%fraHeader").Text = new LocString("credits", "LOC_FRA.header").GetRawText();
		(string, string) tuple = SplitTwoColumn(new LocString("credits", "LOC_FRA.names").GetRawText());
		GetNode<MegaRichTextLabel>("%fraRoles").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%fraNames").Text = tuple.Item2;
		GetNode<MegaLabel>("%deuHeader").Text = new LocString("credits", "LOC_DEU.header").GetRawText();
		GetNode<MegaRichTextLabel>("%deuNames").Text = new LocString("credits", "LOC_DEU.names").GetRawText();
		GetNode<MegaLabel>("%itaHeader").Text = new LocString("credits", "LOC_ITA.header").GetRawText();
		GetNode<MegaRichTextLabel>("%itaTeam").Text = new LocString("credits", "LOC_ITA.team").GetRawText();
		tuple = SplitTwoColumn(new LocString("credits", "LOC_ITA.names").GetRawText());
		GetNode<MegaRichTextLabel>("%itaRoles").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%itaNames").Text = tuple.Item2;
		GetNode<MegaLabel>("%jpnHeader").Text = new LocString("credits", "LOC_JPN.header").GetRawText();
		GetNode<MegaRichTextLabel>("%jpnNames").Text = new LocString("credits", "LOC_JPN.names").GetRawText();
		GetNode<MegaLabel>("%korHeader").Text = new LocString("credits", "LOC_KOR.header").GetRawText();
		GetNode<MegaRichTextLabel>("%korNames").Text = new LocString("credits", "LOC_KOR.names").GetRawText();
		GetNode<MegaLabel>("%polHeader").Text = new LocString("credits", "LOC_POL.header").GetRawText();
		GetNode<MegaRichTextLabel>("%polNames").Text = new LocString("credits", "LOC_POL.names").GetRawText();
		GetNode<MegaLabel>("%rusHeader").Text = new LocString("credits", "LOC_RUS.header").GetRawText();
		tuple = SplitTwoColumn(new LocString("credits", "LOC_RUS.names").GetRawText());
		GetNode<MegaRichTextLabel>("%rusRoles").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%rusNames").Text = tuple.Item2;
		GetNode<MegaLabel>("%spaHeader").Text = new LocString("credits", "LOC_SPA.header").GetRawText();
		GetNode<MegaRichTextLabel>("%spaNames").Text = new LocString("credits", "LOC_SPA.names").GetRawText();
		GetNode<MegaLabel>("%espHeader").Text = new LocString("credits", "LOC_ESP.header").GetRawText();
		GetNode<MegaRichTextLabel>("%espTeam").Text = new LocString("credits", "LOC_ESP.team").GetRawText();
		tuple = SplitTwoColumn(new LocString("credits", "LOC_ESP.names").GetRawText());
		GetNode<MegaRichTextLabel>("%espRoles").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%espNames").Text = tuple.Item2;
		GetNode<MegaLabel>("%thaHeader").Text = new LocString("credits", "LOC_THA.header").GetRawText();
		GetNode<MegaRichTextLabel>("%thaNames").Text = new LocString("credits", "LOC_THA.names").GetRawText();
		GetNode<MegaLabel>("%turHeader").Text = new LocString("credits", "LOC_TUR.header").GetRawText();
		tuple = SplitTwoColumn(new LocString("credits", "LOC_TUR.names").GetRawText());
		GetNode<MegaRichTextLabel>("%turRoles").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%turNames").Text = tuple.Item2;
	}

	private void InitTwitchExtension()
	{
		GetNode<MegaLabel>("%TwitchHeader").Text = new LocString("credits", "TWITCH.header").GetRawText();
		(string, string) tuple = SplitTwoColumn(new LocString("credits", "TWITCH.names").GetRawText());
		GetNode<MegaRichTextLabel>("%TwitchRoles").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%TwitchNames").Text = tuple.Item2;
	}

	private void InitModdingSupport()
	{
		GetNode<MegaLabel>("%ModdingSupportHeader").Text = new LocString("credits", "MODDING_SUPPORT.header").GetRawText();
		GetNode<MegaRichTextLabel>("%ModdingSupportNames").Text = ShuffleOneColumn(new LocString("credits", "MODDING_SUPPORT.names").GetRawText());
	}

	private void InitPlaytesters()
	{
		GetNode<MegaLabel>("%PlaytestersHeader").Text = new LocString("credits", "PLAYTESTERS.header").GetRawText();
		(string, string, string) tuple = SplitThreeColumnPlaytesters(new LocString("credits", "PLAYTESTERS.names").GetRawText());
		GetNode<MegaRichTextLabel>("%PlaytesterNames1").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%PlaytesterNames2").Text = tuple.Item2;
		GetNode<MegaRichTextLabel>("%PlaytesterNames3").Text = tuple.Item3;
	}

	private void InitTrailer()
	{
		GetNode<MegaLabel>("%TrailerHeader").Text = new LocString("credits", "TRAILER.header").GetRawText();
		GetNode<MegaRichTextLabel>("%TrailerAnimationTeam").Text = new LocString("credits", "TRAILER_ANIMATION.team").GetRawText();
		GetNode<MegaLabel>("%TrailerAnimationHeader").Text = new LocString("credits", "TRAILER_ANIMATION.header").GetRawText();
		(string, string) tuple = SplitTwoColumn(new LocString("credits", "TRAILER_ANIMATION.names").GetRawText());
		GetNode<MegaRichTextLabel>("%TrailerAnimationRoles").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%TrailerAnimationNames").Text = tuple.Item2;
		GetNode<MegaLabel>("%TrailerEditorHeader").Text = new LocString("credits", "TRAILER_EDITOR.header").GetRawText();
		tuple = SplitTwoColumn(new LocString("credits", "TRAILER_EDITOR.names").GetRawText());
		GetNode<MegaRichTextLabel>("%TrailerEditorRoles").Text = tuple.Item1;
		GetNode<MegaRichTextLabel>("%TrailerEditorNames").Text = tuple.Item2;
	}

	private void InitFmod()
	{
		GetNode<MegaLabel>("%FmodHeader").Text = new LocString("credits", "FMOD").GetRawText();
	}

	private void InitSpine()
	{
		GetNode<MegaLabel>("%SpineHeader").Text = new LocString("credits", "SPINE").GetRawText();
	}

	private void InitGodot()
	{
		GetNode<MegaLabel>("%GodotHeader").Text = new LocString("credits", "GODOT").GetRawText();
	}

	private void InitExitMessage()
	{
		GetNode<MegaRichTextLabel>("%ExitMessage").Text = new LocString("credits", "EXIT_MESSAGE").GetRawText();
	}

	public override void _EnterTree()
	{
		NHotkeyManager.Instance.AddBlockingScreen(this);
		NHotkeyManager.Instance.PushHotkeyReleasedBinding(MegaInput.cancel, CloseScreenDebug);
	}

	public override void _ExitTree()
	{
		NHotkeyManager.Instance.RemoveHotkeyReleasedBinding(MegaInput.cancel, CloseScreenDebug);
		NHotkeyManager.Instance.RemoveBlockingScreen(this);
	}

	private async Task EnableScreenExit()
	{
		await Task.Delay(2000);
		_canClose = true;
	}

	private void CloseScreenDebug()
	{
		if (_canClose && !_exitingScreen)
		{
			_exitingScreen = true;
			TaskHelper.RunSafely(FadeAndExitScreen());
		}
	}

	private async Task FadeAndExitScreen()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_screenContents, "modulate:a", 0f, 1.0);
		await ToSignal(_tween, Tween.SignalName.Finished);
		NModalContainer.Instance.Clear();
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.Pressed)
		{
			CloseScreenDebug();
		}
		ProcessScrollEvent(inputEvent);
	}

	public override void _Process(double delta)
	{
		float num = (float)delta;
		_targetPosition -= 80f * num;
		_screenContents.Position = _screenContents.Position.Lerp(new Vector2(_screenContents.Position.X, _targetPosition), num * 20f);
	}

	private void ProcessScrollEvent(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				_targetPosition += 50f;
			}
			else if (inputEventMouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				_targetPosition -= 50f;
			}
		}
		else if (inputEvent is InputEventPanGesture inputEventPanGesture)
		{
			_targetPosition += (0f - inputEventPanGesture.Delta.Y) * 20f;
		}
	}

	private static string ShuffleOneColumn(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return string.Empty;
		}
		List<string> list = input.Split(new string[1] { "||" }, StringSplitOptions.None).ToList();
		for (int num = list.Count - 1; num > 0; num--)
		{
			int num2 = Rng.Chaotic.NextInt(num + 1);
			List<string> list2 = list;
			int index = num;
			int index2 = num2;
			string value = list[num2];
			string value2 = list[num];
			list2[index] = value;
			list[index2] = value2;
		}
		return string.Join("\n", list);
	}

	private (string Roles, string Names) SplitTwoColumn(string input)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		string[] array = input.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = (from p in text.Split(new string[1] { "||" }, StringSplitOptions.RemoveEmptyEntries)
				select p.Trim() into p
				where !string.IsNullOrWhiteSpace(p)
				select p).ToArray();
			if (array3.Length == 2)
			{
				list.Add(array3[0]);
				list2.Add(array3[1]);
			}
		}
		return (Roles: string.Join("\n", list), Names: string.Join("\n", list2));
	}

	private (string Column1, string Column2, string Column3) SplitThreeColumnPlaytesters(string input)
	{
		string[] array = (from p in input.Split(new string[1] { "||" }, StringSplitOptions.RemoveEmptyEntries)
			select p.Trim() into p
			where !string.IsNullOrWhiteSpace(p)
			select p).ToArray();
		for (int num = array.Length - 1; num > 0; num--)
		{
			int num2 = Rng.Chaotic.NextInt(num + 1);
			ref string reference = ref array[num];
			ref string reference2 = ref array[num2];
			string text = array[num2];
			string text2 = array[num];
			reference = text;
			reference2 = text2;
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			switch (num3 % 3)
			{
			case 0:
				list.Add(array[num3]);
				break;
			case 1:
				list2.Add(array[num3]);
				break;
			case 2:
				list3.Add(array[num3]);
				break;
			}
		}
		return (Column1: string.Join("\n", list), Column2: string.Join("\n", list2), Column3: string.Join("\n", list3));
	}

	private static (string left, string right) SplitTwoColumnMultiRole(string input)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		string[] array = input.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new string[1] { "||" }, StringSplitOptions.None);
			if (array2.Length == 2)
			{
				string text = array2[0].Trim();
				string text2 = array2[1].Trim();
				List<string> list3 = (from r in text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					select r.Trim()).ToList();
				for (int num = 0; num < list3.Count; num++)
				{
					list.Add(list3[num]);
					list2.Add((num == 0) ? text2 : "");
				}
				bool flag = list3.Count > 1;
				bool flag2 = i == array.Length - 1;
				if (flag && !flag2)
				{
					list.Add("");
					list2.Add("");
				}
			}
		}
		return (left: string.Join("\n", list), right: string.Join("\n", list2));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(25);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitMegaCrit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitComposer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitAdditionalProgramming, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitAdditionalVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitMarketingSupport, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitConsultants, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitVoices, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitLocalization, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitTwitchExtension, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitModdingSupport, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitPlaytesters, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitTrailer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitFmod, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitSpine, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitGodot, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitExitMessage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CloseScreenDebug, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessScrollEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShuffleOneColumn, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCreditsScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitMegaCrit && args.Count == 0)
		{
			InitMegaCrit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitComposer && args.Count == 0)
		{
			InitComposer();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitAdditionalProgramming && args.Count == 0)
		{
			InitAdditionalProgramming();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitAdditionalVfx && args.Count == 0)
		{
			InitAdditionalVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitMarketingSupport && args.Count == 0)
		{
			InitMarketingSupport();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitConsultants && args.Count == 0)
		{
			InitConsultants();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitVoices && args.Count == 0)
		{
			InitVoices();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitLocalization && args.Count == 0)
		{
			InitLocalization();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitTwitchExtension && args.Count == 0)
		{
			InitTwitchExtension();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitModdingSupport && args.Count == 0)
		{
			InitModdingSupport();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitPlaytesters && args.Count == 0)
		{
			InitPlaytesters();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitTrailer && args.Count == 0)
		{
			InitTrailer();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitFmod && args.Count == 0)
		{
			InitFmod();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitSpine && args.Count == 0)
		{
			InitSpine();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitGodot && args.Count == 0)
		{
			InitGodot();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitExitMessage && args.Count == 0)
		{
			InitExitMessage();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CloseScreenDebug && args.Count == 0)
		{
			CloseScreenDebug();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessScrollEvent && args.Count == 1)
		{
			ProcessScrollEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShuffleOneColumn && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ShuffleOneColumn(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCreditsScreen>(Create());
			return true;
		}
		if (method == MethodName.ShuffleOneColumn && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ShuffleOneColumn(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.InitMegaCrit)
		{
			return true;
		}
		if (method == MethodName.InitComposer)
		{
			return true;
		}
		if (method == MethodName.InitAdditionalProgramming)
		{
			return true;
		}
		if (method == MethodName.InitAdditionalVfx)
		{
			return true;
		}
		if (method == MethodName.InitMarketingSupport)
		{
			return true;
		}
		if (method == MethodName.InitConsultants)
		{
			return true;
		}
		if (method == MethodName.InitVoices)
		{
			return true;
		}
		if (method == MethodName.InitLocalization)
		{
			return true;
		}
		if (method == MethodName.InitTwitchExtension)
		{
			return true;
		}
		if (method == MethodName.InitModdingSupport)
		{
			return true;
		}
		if (method == MethodName.InitPlaytesters)
		{
			return true;
		}
		if (method == MethodName.InitTrailer)
		{
			return true;
		}
		if (method == MethodName.InitFmod)
		{
			return true;
		}
		if (method == MethodName.InitSpine)
		{
			return true;
		}
		if (method == MethodName.InitGodot)
		{
			return true;
		}
		if (method == MethodName.InitExitMessage)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.CloseScreenDebug)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.ProcessScrollEvent)
		{
			return true;
		}
		if (method == MethodName.ShuffleOneColumn)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._canClose)
		{
			_canClose = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._exitingScreen)
		{
			_exitingScreen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._screenContents)
		{
			_screenContents = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			_targetPosition = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._canClose)
		{
			value = VariantUtils.CreateFrom(in _canClose);
			return true;
		}
		if (name == PropertyName._exitingScreen)
		{
			value = VariantUtils.CreateFrom(in _exitingScreen);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._screenContents)
		{
			value = VariantUtils.CreateFrom(in _screenContents);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			value = VariantUtils.CreateFrom(in _targetPosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._canClose, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._exitingScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenContents, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._targetPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._canClose, Variant.From(in _canClose));
		info.AddProperty(PropertyName._exitingScreen, Variant.From(in _exitingScreen));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._screenContents, Variant.From(in _screenContents));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._targetPosition, Variant.From(in _targetPosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._canClose, out var value))
		{
			_canClose = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._exitingScreen, out var value2))
		{
			_exitingScreen = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._screenContents, out var value4))
		{
			_screenContents = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value5))
		{
			_backButton = value5.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._targetPosition, out var value6))
		{
			_targetPosition = value6.As<float>();
		}
	}
}
