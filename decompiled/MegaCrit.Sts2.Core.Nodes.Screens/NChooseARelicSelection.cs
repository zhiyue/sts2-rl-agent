using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NChooseARelicSelection.cs")]
public class NChooseARelicSelection : Control, IOverlayScreen, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName SelectHolder = "SelectHolder";

		public static readonly StringName OnSkipButtonReleased = "OnSkipButtonReleased";

		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName AfterOverlayClosed = "AfterOverlayClosed";

		public static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		public static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _banner = "_banner";

		public static readonly StringName _relicRow = "_relicRow";

		public static readonly StringName _skipButton = "_skipButton";

		public static readonly StringName _screenComplete = "_screenComplete";

		public static readonly StringName _relicSelected = "_relicSelected";

		public static readonly StringName _cardTween = "_cardTween";

		public static readonly StringName _fadeTween = "_fadeTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const float _relicXSpacing = 200f;

	private NCommonBanner _banner;

	private Control _relicRow;

	private NChoiceSelectionSkipButton _skipButton;

	private readonly TaskCompletionSource<IEnumerable<RelicModel>> _completionSource = new TaskCompletionSource<IEnumerable<RelicModel>>();

	private bool _screenComplete;

	private bool _relicSelected;

	private Tween? _cardTween;

	private Tween? _fadeTween;

	private IReadOnlyList<RelicModel> _relics;

	public NetScreenType ScreenType => NetScreenType.Rewards;

	private static string ScenePath => SceneHelper.GetScenePath("screens/choose_a_relic_selection_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public bool UseSharedBackstop => true;

	public Control DefaultFocusedControl
	{
		get
		{
			List<NRelicBasicHolder> list = _relicRow.GetChildren().OfType<NRelicBasicHolder>().ToList();
			return list[list.Count / 2];
		}
	}

	public static NChooseARelicSelection? ShowScreen(IReadOnlyList<RelicModel> relics)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NChooseARelicSelection nChooseARelicSelection = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NChooseARelicSelection>(PackedScene.GenEditState.Disabled);
		nChooseARelicSelection.Name = "NChooseACardSelectionScreen";
		nChooseARelicSelection._relics = relics;
		NOverlayStack.Instance.Push(nChooseARelicSelection);
		return nChooseARelicSelection;
	}

	public override void _Ready()
	{
		_banner = GetNode<NCommonBanner>("Banner");
		_banner.label.SetTextAutoSize(new LocString("gameplay_ui", "CHOOSE_RELIC_HEADER").GetRawText());
		_banner.AnimateIn();
		_relicRow = GetNode<Control>("RelicRow");
		Vector2 vector = Vector2.Left * (_relics.Count - 1) * 200f * 0.5f;
		for (int i = 0; i < _relics.Count; i++)
		{
			RelicModel relic = _relics[i];
			NRelicBasicHolder holder = NRelicBasicHolder.Create(relic);
			holder.Scale = Vector2.One * 2f;
			_relicRow.AddChildSafely(holder);
			holder.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
			{
				SelectHolder(holder);
			}));
			_cardTween = CreateTween().SetParallel();
			_cardTween.TweenProperty(holder, "position", holder.Position + vector + Vector2.Right * 200f * i, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_cardTween.TweenProperty(holder, "modulate", Colors.White, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
				.From(Colors.Black);
		}
		_skipButton = GetNode<NChoiceSelectionSkipButton>("SkipButton");
		_skipButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnSkipButtonReleased));
		_skipButton.AnimateIn();
		List<NRelicBasicHolder> list = _relicRow.GetChildren().OfType<NRelicBasicHolder>().ToList();
		NRelicBasicHolder nRelicBasicHolder = _relicRow.GetChildren().OfType<NRelicBasicHolder>().ToList()[list.Count / 2];
		_skipButton.FocusNeighborTop = nRelicBasicHolder.GetPath();
		_skipButton.FocusNeighborBottom = _skipButton.GetPath();
		_skipButton.FocusNeighborLeft = _skipButton.GetPath();
		_skipButton.FocusNeighborRight = _skipButton.GetPath();
		for (int num = 0; num < _relicRow.GetChildCount(); num++)
		{
			Control child = _relicRow.GetChild<Control>(num);
			child.FocusNeighborBottom = child.GetPath();
			child.FocusNeighborTop = child.GetPath();
			child.FocusNeighborLeft = ((num > 0) ? _relicRow.GetChild(num - 1).GetPath() : _relicRow.GetChild(_relicRow.GetChildCount() - 1).GetPath());
			child.FocusNeighborRight = ((num < _relicRow.GetChildCount() - 1) ? _relicRow.GetChild(num + 1).GetPath() : _relicRow.GetChild(0).GetPath());
		}
	}

	public override void _ExitTree()
	{
		if (!_completionSource.Task.IsCompleted)
		{
			_completionSource.SetCanceled();
		}
	}

	private void SelectHolder(NRelicBasicHolder relicHolder)
	{
		RelicModel model = relicHolder.Relic.Model;
		_screenComplete = true;
		_relicSelected = true;
		_completionSource.SetResult(new RelicModel[1] { model });
	}

	public async Task<IEnumerable<RelicModel>> RelicsSelected()
	{
		IEnumerable<RelicModel> result = await _completionSource.Task;
		NOverlayStack.Instance.Remove(this);
		return result;
	}

	private void OnSkipButtonReleased(NButton _)
	{
		_screenComplete = true;
		_completionSource.SetResult(Array.Empty<RelicModel>());
	}

	public void AfterOverlayOpened()
	{
		base.Modulate = Colors.Transparent;
		_fadeTween?.Kill();
		_fadeTween = CreateTween();
		_fadeTween.TweenProperty(this, "modulate:a", 1f, 0.2);
	}

	public void AfterOverlayClosed()
	{
		_fadeTween?.Kill();
		this.QueueFreeSafely();
	}

	public void AfterOverlayShown()
	{
		base.Visible = true;
	}

	public void AfterOverlayHidden()
	{
		base.Visible = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SelectHolder, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "relicHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSkipButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SelectHolder && args.Count == 1)
		{
			SelectHolder(VariantUtils.ConvertTo<NRelicBasicHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSkipButtonReleased && args.Count == 1)
		{
			OnSkipButtonReleased(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayOpened && args.Count == 0)
		{
			AfterOverlayOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayClosed && args.Count == 0)
		{
			AfterOverlayClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayShown && args.Count == 0)
		{
			AfterOverlayShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayHidden && args.Count == 0)
		{
			AfterOverlayHidden();
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
		if (method == MethodName.SelectHolder)
		{
			return true;
		}
		if (method == MethodName.OnSkipButtonReleased)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayOpened)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayClosed)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayShown)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayHidden)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._banner)
		{
			_banner = VariantUtils.ConvertTo<NCommonBanner>(in value);
			return true;
		}
		if (name == PropertyName._relicRow)
		{
			_relicRow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._skipButton)
		{
			_skipButton = VariantUtils.ConvertTo<NChoiceSelectionSkipButton>(in value);
			return true;
		}
		if (name == PropertyName._screenComplete)
		{
			_screenComplete = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._relicSelected)
		{
			_relicSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			_cardTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			_fadeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._banner)
		{
			value = VariantUtils.CreateFrom(in _banner);
			return true;
		}
		if (name == PropertyName._relicRow)
		{
			value = VariantUtils.CreateFrom(in _relicRow);
			return true;
		}
		if (name == PropertyName._skipButton)
		{
			value = VariantUtils.CreateFrom(in _skipButton);
			return true;
		}
		if (name == PropertyName._screenComplete)
		{
			value = VariantUtils.CreateFrom(in _screenComplete);
			return true;
		}
		if (name == PropertyName._relicSelected)
		{
			value = VariantUtils.CreateFrom(in _relicSelected);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			value = VariantUtils.CreateFrom(in _cardTween);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			value = VariantUtils.CreateFrom(in _fadeTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicRow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._skipButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._screenComplete, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._relicSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._relicRow, Variant.From(in _relicRow));
		info.AddProperty(PropertyName._skipButton, Variant.From(in _skipButton));
		info.AddProperty(PropertyName._screenComplete, Variant.From(in _screenComplete));
		info.AddProperty(PropertyName._relicSelected, Variant.From(in _relicSelected));
		info.AddProperty(PropertyName._cardTween, Variant.From(in _cardTween));
		info.AddProperty(PropertyName._fadeTween, Variant.From(in _fadeTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._banner, out var value))
		{
			_banner = value.As<NCommonBanner>();
		}
		if (info.TryGetProperty(PropertyName._relicRow, out var value2))
		{
			_relicRow = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._skipButton, out var value3))
		{
			_skipButton = value3.As<NChoiceSelectionSkipButton>();
		}
		if (info.TryGetProperty(PropertyName._screenComplete, out var value4))
		{
			_screenComplete = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._relicSelected, out var value5))
		{
			_relicSelected = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cardTween, out var value6))
		{
			_cardTween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._fadeTween, out var value7))
		{
			_fadeTween = value7.As<Tween>();
		}
	}
}
