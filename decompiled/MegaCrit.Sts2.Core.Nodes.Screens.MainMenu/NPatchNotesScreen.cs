using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NPatchNotesScreen.cs")]
public class NPatchNotesScreen : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName CreateNewPatchEntry = "CreateNewPatchEntry";

		public static readonly StringName NextPatchNote = "NextPatchNote";

		public static readonly StringName PreviousPatchNote = "PreviousPatchNote";

		public static readonly StringName Open = "Open";

		public static readonly StringName Close = "Close";

		public static readonly StringName LoadPatchNoteText = "LoadPatchNoteText";

		public static readonly StringName ReadPatchNoteFile = "ReadPatchNoteFile";

		public static readonly StringName UpdateDateLabel = "UpdateDateLabel";

		public static readonly StringName GetFileNameFromPath = "GetFileNameFromPath";

		public static readonly StringName RemoveFileExtension = "RemoveFileExtension";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName IsOpen = "IsOpen";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _screenContents = "_screenContents";

		public static readonly StringName _marginContainer = "_marginContainer";

		public static readonly StringName _prevButton = "_prevButton";

		public static readonly StringName _nextButton = "_nextButton";

		public static readonly StringName _patchNotesToggle = "_patchNotesToggle";

		public static readonly StringName _backButton = "_backButton";

		public static readonly StringName _dateLabel = "_dateLabel";

		public static readonly StringName _patchText = "_patchText";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _cachedScene = "_cachedScene";

		public static readonly StringName _index = "_index";

		public static readonly StringName _currentScrollLine = "_currentScrollLine";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NScrollableContainer _screenContents;

	private MarginContainer _marginContainer;

	private NButton _prevButton;

	private NButton _nextButton;

	private NButton _patchNotesToggle;

	private NButton _backButton;

	private MegaLabel _dateLabel;

	private MegaRichTextLabel _patchText;

	private Tween? _tween;

	private PackedScene _cachedScene;

	private const string _patchNotesPath = "res://localization/eng/patch_notes";

	private List<string>? _patchNotePaths;

	private int _index;

	private int _currentScrollLine;

	public bool IsOpen { get; private set; }

	public Control? DefaultFocusedControl => null;

	public override void _Ready()
	{
		_cachedScene = ResourceLoader.Load<PackedScene>("res://scenes/screens/patch_screen_contents.tscn", null, ResourceLoader.CacheMode.Reuse);
		CreateNewPatchEntry();
		_prevButton = GetNode<NButton>("PrevButton");
		_prevButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			PreviousPatchNote();
		}));
		_nextButton = GetNode<NButton>("NextButton");
		_nextButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			NextPatchNote();
		}));
		_nextButton.Visible = false;
		_patchNotesToggle = GetNode<NButton>("%PatchNotesToggle");
		_patchNotesToggle.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			Close();
		}));
		_patchNotesToggle.Disable();
		_backButton = GetNode<NButton>("%BackButton");
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			Close();
		}));
	}

	private void CreateNewPatchEntry()
	{
		_screenContents = _cachedScene.Instantiate<NScrollableContainer>(PackedScene.GenEditState.Disabled);
		this.AddChildSafely(_screenContents);
		MoveChild(_screenContents, 0);
		_marginContainer = _screenContents.GetNode<MarginContainer>("Content");
		_patchText = _screenContents.GetNode<MegaRichTextLabel>("Content/PatchText");
		_dateLabel = _patchText.GetNode<MegaLabel>("DateLabel");
		if (_patchNotePaths != null)
		{
			string patchNotePath = _patchNotePaths[_index];
			LoadPatchNoteText(patchNotePath);
		}
	}

	private void NextPatchNote()
	{
		if (!_nextButton.Visible)
		{
			return;
		}
		if (_patchNotePaths == null)
		{
			Log.Error("NPatchNotesScreen: No patch paths available!");
			return;
		}
		_index--;
		_prevButton.Visible = true;
		if (_index == 0)
		{
			_nextButton.Visible = false;
		}
		_screenContents.QueueFreeSafely();
		CreateNewPatchEntry();
	}

	private void PreviousPatchNote()
	{
		if (_patchNotePaths == null)
		{
			Log.Error("NPatchNotesScreen: No patch paths available!");
			return;
		}
		_index++;
		_nextButton.Visible = true;
		if (_index == _patchNotePaths.Count - 1)
		{
			_prevButton.Visible = false;
		}
		_screenContents.QueueFreeSafely();
		CreateNewPatchEntry();
	}

	public void Open()
	{
		IsOpen = true;
		NGame.Instance.MainMenu?.EnableBackstop();
		_patchNotesToggle.Enable();
		_backButton.Enable();
		base.Visible = true;
		_tween?.FastForwardToCompletion();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 1f, 0.25);
		if (_patchNotePaths == null)
		{
			_patchNotePaths = (from fileName in DirAccess.GetFilesAt("res://localization/eng/patch_notes")
				select "res://localization/eng/patch_notes/" + fileName).Reverse().ToList();
		}
		LoadPatchNoteText(_patchNotePaths[_index]);
		ActiveScreenContext.Instance.Update();
		NHotkeyManager.Instance.PushHotkeyReleasedBinding(MegaInput.left, PreviousPatchNote);
		NHotkeyManager.Instance.PushHotkeyReleasedBinding(MegaInput.right, NextPatchNote);
		NHotkeyManager.Instance.PushHotkeyReleasedBinding(MegaInput.pauseAndBack, Close);
	}

	private void Close()
	{
		NHotkeyManager.Instance.RemoveHotkeyReleasedBinding(MegaInput.left, PreviousPatchNote);
		NHotkeyManager.Instance.RemoveHotkeyReleasedBinding(MegaInput.right, NextPatchNote);
		NHotkeyManager.Instance.RemoveHotkeyReleasedBinding(MegaInput.pauseAndBack, Close);
		_patchNotesToggle.Disable();
		_backButton.Disable();
		NGame.Instance.MainMenu?.DisableBackstop();
		_tween?.FastForwardToCompletion();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 0f, 0.25);
		_tween.TweenCallback(Callable.From(delegate
		{
			IsOpen = false;
			SetVisible(visible: false);
			ActiveScreenContext.Instance.Update();
		}));
	}

	private void LoadPatchNoteText(string patchNotePath)
	{
		_patchText.ScrollToLine(0);
		_currentScrollLine = 0;
		string textAutoSize = ReadPatchNoteFile(patchNotePath);
		_patchText.SetTextAutoSize(textAutoSize);
		UpdateDateLabel(patchNotePath);
	}

	private static string ReadPatchNoteFile(string patchNotePath)
	{
		using FileAccess fileAccess = FileAccess.Open(patchNotePath, FileAccess.ModeFlags.Read);
		return fileAccess.GetAsText();
	}

	private void UpdateDateLabel(string patchNotePath)
	{
		string fileNameFromPath = GetFileNameFromPath(patchNotePath);
		string text = RemoveFileExtension(fileNameFromPath);
		if (TryParseDate(text, out string formattedDate))
		{
			_dateLabel.SetTextAutoSize(formattedDate);
		}
		else
		{
			Log.Error("Invalid date format in file name: " + text);
		}
	}

	private static string GetFileNameFromPath(string path)
	{
		int num = path.LastIndexOf('/') + 1;
		return path.Substring(num, path.Length - num);
	}

	private static string RemoveFileExtension(string fileName)
	{
		return fileName.Split('.')[0];
	}

	private static bool TryParseDate(string dateString, out string formattedDate)
	{
		if (DateTime.TryParseExact(dateString, "yyyy_MM_d", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			formattedDate = result.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);
			return true;
		}
		formattedDate = string.Empty;
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateNewPatchEntry, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.NextPatchNote, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PreviousPatchNote, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LoadPatchNoteText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "patchNotePath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ReadPatchNoteFile, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "patchNotePath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateDateLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "patchNotePath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetFileNameFromPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveFileExtension, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "fileName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.CreateNewPatchEntry && args.Count == 0)
		{
			CreateNewPatchEntry();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NextPatchNote && args.Count == 0)
		{
			NextPatchNote();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PreviousPatchNote && args.Count == 0)
		{
			PreviousPatchNote();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Open && args.Count == 0)
		{
			Open();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Close && args.Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LoadPatchNoteText && args.Count == 1)
		{
			LoadPatchNoteText(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReadPatchNoteFile && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ReadPatchNoteFile(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.UpdateDateLabel && args.Count == 1)
		{
			UpdateDateLabel(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetFileNameFromPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetFileNameFromPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.RemoveFileExtension && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(RemoveFileExtension(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ReadPatchNoteFile && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ReadPatchNoteFile(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetFileNameFromPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetFileNameFromPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.RemoveFileExtension && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(RemoveFileExtension(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.CreateNewPatchEntry)
		{
			return true;
		}
		if (method == MethodName.NextPatchNote)
		{
			return true;
		}
		if (method == MethodName.PreviousPatchNote)
		{
			return true;
		}
		if (method == MethodName.Open)
		{
			return true;
		}
		if (method == MethodName.Close)
		{
			return true;
		}
		if (method == MethodName.LoadPatchNoteText)
		{
			return true;
		}
		if (method == MethodName.ReadPatchNoteFile)
		{
			return true;
		}
		if (method == MethodName.UpdateDateLabel)
		{
			return true;
		}
		if (method == MethodName.GetFileNameFromPath)
		{
			return true;
		}
		if (method == MethodName.RemoveFileExtension)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsOpen)
		{
			IsOpen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._screenContents)
		{
			_screenContents = VariantUtils.ConvertTo<NScrollableContainer>(in value);
			return true;
		}
		if (name == PropertyName._marginContainer)
		{
			_marginContainer = VariantUtils.ConvertTo<MarginContainer>(in value);
			return true;
		}
		if (name == PropertyName._prevButton)
		{
			_prevButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._nextButton)
		{
			_nextButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._patchNotesToggle)
		{
			_patchNotesToggle = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._dateLabel)
		{
			_dateLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._patchText)
		{
			_patchText = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._cachedScene)
		{
			_cachedScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._index)
		{
			_index = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._currentScrollLine)
		{
			_currentScrollLine = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsOpen)
		{
			value = VariantUtils.CreateFrom<bool>(IsOpen);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._screenContents)
		{
			value = VariantUtils.CreateFrom(in _screenContents);
			return true;
		}
		if (name == PropertyName._marginContainer)
		{
			value = VariantUtils.CreateFrom(in _marginContainer);
			return true;
		}
		if (name == PropertyName._prevButton)
		{
			value = VariantUtils.CreateFrom(in _prevButton);
			return true;
		}
		if (name == PropertyName._nextButton)
		{
			value = VariantUtils.CreateFrom(in _nextButton);
			return true;
		}
		if (name == PropertyName._patchNotesToggle)
		{
			value = VariantUtils.CreateFrom(in _patchNotesToggle);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._dateLabel)
		{
			value = VariantUtils.CreateFrom(in _dateLabel);
			return true;
		}
		if (name == PropertyName._patchText)
		{
			value = VariantUtils.CreateFrom(in _patchText);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._cachedScene)
		{
			value = VariantUtils.CreateFrom(in _cachedScene);
			return true;
		}
		if (name == PropertyName._index)
		{
			value = VariantUtils.CreateFrom(in _index);
			return true;
		}
		if (name == PropertyName._currentScrollLine)
		{
			value = VariantUtils.CreateFrom(in _currentScrollLine);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenContents, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._marginContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._prevButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nextButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._patchNotesToggle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dateLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._patchText, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cachedScene, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._index, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentScrollLine, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsOpen, Variant.From<bool>(IsOpen));
		info.AddProperty(PropertyName._screenContents, Variant.From(in _screenContents));
		info.AddProperty(PropertyName._marginContainer, Variant.From(in _marginContainer));
		info.AddProperty(PropertyName._prevButton, Variant.From(in _prevButton));
		info.AddProperty(PropertyName._nextButton, Variant.From(in _nextButton));
		info.AddProperty(PropertyName._patchNotesToggle, Variant.From(in _patchNotesToggle));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._dateLabel, Variant.From(in _dateLabel));
		info.AddProperty(PropertyName._patchText, Variant.From(in _patchText));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._cachedScene, Variant.From(in _cachedScene));
		info.AddProperty(PropertyName._index, Variant.From(in _index));
		info.AddProperty(PropertyName._currentScrollLine, Variant.From(in _currentScrollLine));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsOpen, out var value))
		{
			IsOpen = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._screenContents, out var value2))
		{
			_screenContents = value2.As<NScrollableContainer>();
		}
		if (info.TryGetProperty(PropertyName._marginContainer, out var value3))
		{
			_marginContainer = value3.As<MarginContainer>();
		}
		if (info.TryGetProperty(PropertyName._prevButton, out var value4))
		{
			_prevButton = value4.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._nextButton, out var value5))
		{
			_nextButton = value5.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._patchNotesToggle, out var value6))
		{
			_patchNotesToggle = value6.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value7))
		{
			_backButton = value7.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._dateLabel, out var value8))
		{
			_dateLabel = value8.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._patchText, out var value9))
		{
			_patchText = value9.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value10))
		{
			_tween = value10.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._cachedScene, out var value11))
		{
			_cachedScene = value11.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._index, out var value12))
		{
			_index = value12.As<int>();
		}
		if (info.TryGetProperty(PropertyName._currentScrollLine, out var value13))
		{
			_currentScrollLine = value13.As<int>();
		}
	}
}
