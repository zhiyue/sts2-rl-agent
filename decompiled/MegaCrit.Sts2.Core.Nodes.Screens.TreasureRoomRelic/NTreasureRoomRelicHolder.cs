using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

[ScriptPath("res://src/Core/Nodes/Screens/TreasureRoomRelic/NTreasureRoomRelicHolder.cs")]
public class NTreasureRoomRelicHolder : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'AnimateAwayVotes' method.
		/// </summary>
		public static readonly StringName AnimateAwayVotes = "AnimateAwayVotes";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'OnPress' method.
		/// </summary>
		public new static readonly StringName OnPress = "OnPress";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Index' property.
		/// </summary>
		public static readonly StringName Index = "Index";

		/// <summary>
		/// Cached name for the 'VoteContainer' property.
		/// </summary>
		public static readonly StringName VoteContainer = "VoteContainer";

		/// <summary>
		/// Cached name for the 'Relic' property.
		/// </summary>
		public static readonly StringName Relic = "Relic";

		/// <summary>
		/// Cached name for the '_uncommonGlow' field.
		/// </summary>
		public static readonly StringName _uncommonGlow = "_uncommonGlow";

		/// <summary>
		/// Cached name for the '_rareGlow' field.
		/// </summary>
		public static readonly StringName _rareGlow = "_rareGlow";

		/// <summary>
		/// Cached name for the '_animatedIn' field.
		/// </summary>
		public static readonly StringName _animatedIn = "_animatedIn";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_initTween' field.
		/// </summary>
		public static readonly StringName _initTween = "_initTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private GpuParticles2D _uncommonGlow;

	private GpuParticles2D _rareGlow;

	private bool _animatedIn;

	private Tween? _tween;

	private Tween? _initTween;

	public int Index { get; set; }

	public NMultiplayerVoteContainer VoteContainer { get; private set; }

	public NRelic Relic { get; private set; }

	public override void _Ready()
	{
		ConnectSignals();
		VoteContainer = GetNode<NMultiplayerVoteContainer>("%MultiplayerVoteContainer");
		Relic = GetNode<NRelic>("%Relic");
		_uncommonGlow = GetNode<GpuParticles2D>("%UncommonGlow");
		_rareGlow = GetNode<GpuParticles2D>("%RareGlow");
	}

	public void Initialize(RelicModel relic, IRunState runState)
	{
		Relic.Model = relic;
		VoteContainer.Initialize(PlayerVotedForRelic, runState.Players);
		Relic.Modulate = StsColors.transparentBlack;
		_initTween?.Kill();
		_initTween = CreateTween().SetParallel();
		_initTween.TweenProperty(Relic, "modulate", Colors.White, 0.25);
		if (Relic.Model.Rarity == RelicRarity.Uncommon)
		{
			Tween tween = CreateTween().SetParallel();
			_uncommonGlow.Visible = true;
			_uncommonGlow.Modulate = StsColors.transparentWhite;
			_uncommonGlow.GlobalPosition = Relic.GlobalPosition + Vector2.One * 68f;
			tween.TweenProperty(_uncommonGlow, "modulate", Colors.White, 0.25);
		}
		else if (Relic.Model.Rarity == RelicRarity.Rare)
		{
			Tween tween2 = CreateTween().SetParallel();
			_rareGlow.Visible = true;
			_rareGlow.Modulate = StsColors.transparentWhite;
			_rareGlow.GlobalPosition = Relic.GlobalPosition + Vector2.One * 68f;
			tween2.TweenProperty(_rareGlow, "modulate", Colors.White, 0.25);
		}
	}

	private bool PlayerVotedForRelic(Player player)
	{
		TreasureRoomRelicSynchronizer.PlayerVote playerVote = RunManager.Instance.TreasureRoomRelicSynchronizer.GetPlayerVote(player);
		if (playerVote.voteReceived)
		{
			return playerVote.index == Index;
		}
		return false;
	}

	public void AnimateAwayVotes()
	{
		CreateTween().TweenProperty(VoteContainer, "modulate:a", 0f, 0.25);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(Relic, "scale", Vector2.One * 2.1f, 0.05);
		NHoverTipSet.CreateAndShow(this, Relic.Model.HoverTips)?.SetAlignmentForRelic(Relic);
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(Relic, "scale", Vector2.One * 2f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(Relic, "scale", Vector2.One * 1.9f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(Relic, "scale", Vector2.One * 2f, 0.05).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateAwayVotes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateAwayVotes && args.Count == 0)
		{
			AnimateAwayVotes();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.AnimateAwayVotes)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Index)
		{
			Index = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.VoteContainer)
		{
			VoteContainer = VariantUtils.ConvertTo<NMultiplayerVoteContainer>(in value);
			return true;
		}
		if (name == PropertyName.Relic)
		{
			Relic = VariantUtils.ConvertTo<NRelic>(in value);
			return true;
		}
		if (name == PropertyName._uncommonGlow)
		{
			_uncommonGlow = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._rareGlow)
		{
			_rareGlow = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._animatedIn)
		{
			_animatedIn = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._initTween)
		{
			_initTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Index)
		{
			value = VariantUtils.CreateFrom<int>(Index);
			return true;
		}
		if (name == PropertyName.VoteContainer)
		{
			value = VariantUtils.CreateFrom<NMultiplayerVoteContainer>(VoteContainer);
			return true;
		}
		if (name == PropertyName.Relic)
		{
			value = VariantUtils.CreateFrom<NRelic>(Relic);
			return true;
		}
		if (name == PropertyName._uncommonGlow)
		{
			value = VariantUtils.CreateFrom(in _uncommonGlow);
			return true;
		}
		if (name == PropertyName._rareGlow)
		{
			value = VariantUtils.CreateFrom(in _rareGlow);
			return true;
		}
		if (name == PropertyName._animatedIn)
		{
			value = VariantUtils.CreateFrom(in _animatedIn);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._initTween)
		{
			value = VariantUtils.CreateFrom(in _initTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Index, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VoteContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Relic, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._uncommonGlow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rareGlow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._animatedIn, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._initTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Index, Variant.From<int>(Index));
		info.AddProperty(PropertyName.VoteContainer, Variant.From<NMultiplayerVoteContainer>(VoteContainer));
		info.AddProperty(PropertyName.Relic, Variant.From<NRelic>(Relic));
		info.AddProperty(PropertyName._uncommonGlow, Variant.From(in _uncommonGlow));
		info.AddProperty(PropertyName._rareGlow, Variant.From(in _rareGlow));
		info.AddProperty(PropertyName._animatedIn, Variant.From(in _animatedIn));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._initTween, Variant.From(in _initTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Index, out var value))
		{
			Index = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName.VoteContainer, out var value2))
		{
			VoteContainer = value2.As<NMultiplayerVoteContainer>();
		}
		if (info.TryGetProperty(PropertyName.Relic, out var value3))
		{
			Relic = value3.As<NRelic>();
		}
		if (info.TryGetProperty(PropertyName._uncommonGlow, out var value4))
		{
			_uncommonGlow = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._rareGlow, out var value5))
		{
			_rareGlow = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._animatedIn, out var value6))
		{
			_animatedIn = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value7))
		{
			_tween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._initTween, out var value8))
		{
			_initTween = value8.As<Tween>();
		}
	}
}
