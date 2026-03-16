using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline.UnlockScreens;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/UnlockScreens/NUnlockEpochScreen.cs")]
public class NUnlockEpochScreen : NUnlockScreen
{
	public new class MethodName : NUnlockScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName Open = "Open";
	}

	public new class PropertyName : NUnlockScreen.PropertyName
	{
		public static readonly StringName _cardFlyTween = "_cardFlyTween";

		public static readonly StringName _infoLabel = "_infoLabel";
	}

	public new class SignalName : NUnlockScreen.SignalName
	{
	}

	private IReadOnlyList<EpochModel> _unlockedEpochs;

	private Tween? _cardFlyTween;

	private const double _initDelay = 0.3;

	private RichTextLabel _infoLabel;

	public override void _Ready()
	{
		ConnectSignals();
		_infoLabel = GetNode<RichTextLabel>("%InfoLabel");
		LocString locString = new LocString("timeline", "UNLOCK_EPOCHS");
		_infoLabel.Text = "[center]" + locString.GetFormattedText() + "[/center]";
		_infoLabel.Modulate = StsColors.transparentWhite;
	}

	public override void Open()
	{
		base.Open();
		_cardFlyTween = CreateTween().SetParallel();
		double num = 0.3;
		Vector2 position = GetNode<Control>("%Center").Position;
		PackedScene scene = PreloadManager.Cache.GetScene("res://scenes/timeline_screen/epoch.tscn");
		if (_unlockedEpochs.Count == 3)
		{
			for (int i = 0; i < _unlockedEpochs.Count; i++)
			{
				NEpochCard nEpochCard = scene.Instantiate<NEpochCard>(PackedScene.GenEditState.Disabled);
				nEpochCard.Init(_unlockedEpochs[i]);
				Control node = GetNode<Control>($"Slot{i}");
				node.AddChildSafely(nEpochCard);
				nEpochCard.SetToWigglyUnlockPreviewMode();
				_cardFlyTween.TweenProperty(node, "modulate", Colors.White, 1.0).SetDelay(num - 0.3);
				_cardFlyTween.TweenProperty(node, "position", node.Position, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
					.SetDelay(num);
				node.Modulate = StsColors.transparentBlack;
				node.Position = position;
				num += 0.25;
			}
		}
		else if (_unlockedEpochs.Count == 2)
		{
			for (int j = 0; j < _unlockedEpochs.Count; j++)
			{
				NEpochCard nEpochCard2 = scene.Instantiate<NEpochCard>(PackedScene.GenEditState.Disabled);
				nEpochCard2.Init(_unlockedEpochs[j]);
				Control node2 = GetNode<Control>($"Slot{3 + j}");
				node2.AddChildSafely(nEpochCard2);
				nEpochCard2.SetToWigglyUnlockPreviewMode();
				_cardFlyTween.TweenProperty(node2, "modulate", Colors.White, 1.0).SetDelay(num - 0.3);
				_cardFlyTween.TweenProperty(node2, "position", node2.Position, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
					.SetDelay(num);
				node2.Modulate = StsColors.transparentBlack;
				node2.Position = position;
				num += 0.33;
			}
		}
		else
		{
			Log.Error("Unlocking exactly 1 OR more than 3 Epochs are not supported.");
		}
		_cardFlyTween.TweenProperty(_infoLabel, "modulate", Colors.White, 1.0).SetDelay(0.25);
	}

	public void SetUnlocks(IReadOnlyList<EpochModel> epochs)
	{
		_unlockedEpochs = epochs;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Open && args.Count == 0)
		{
			Open();
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
		if (method == MethodName.Open)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._cardFlyTween)
		{
			_cardFlyTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._infoLabel)
		{
			_infoLabel = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._cardFlyTween)
		{
			value = VariantUtils.CreateFrom(in _cardFlyTween);
			return true;
		}
		if (name == PropertyName._infoLabel)
		{
			value = VariantUtils.CreateFrom(in _infoLabel);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardFlyTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._cardFlyTween, Variant.From(in _cardFlyTween));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._cardFlyTween, out var value))
		{
			_cardFlyTween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._infoLabel, out var value2))
		{
			_infoLabel = value2.As<RichTextLabel>();
		}
	}
}
