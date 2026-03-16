using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Rooms;

[ScriptPath("res://src/Core/Nodes/Rooms/NMerchantButton.cs")]
public class NMerchantButton : NButton
{
	[Signal]
	public delegate void MerchantOpenedEventHandler(NMerchantButton merchantButton);

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName RefreshFocus = "RefreshFocus";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public new static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName IsLocalPlayerDead = "IsLocalPlayerDead";

		public static readonly StringName _merchantSelectionReticle = "_merchantSelectionReticle";

		public static readonly StringName _focusedWhileTargeting = "_focusedWhileTargeting";
	}

	public new class SignalName : NButton.SignalName
	{
		public static readonly StringName MerchantOpened = "MerchantOpened";
	}

	private MegaSkeleton _merchantSkeleton;

	private NSelectionReticle _merchantSelectionReticle;

	private bool _focusedWhileTargeting;

	private MerchantOpenedEventHandler backing_MerchantOpened;

	protected override string[] Hotkeys => new string[1] { MegaInput.select };

	public bool IsLocalPlayerDead { get; set; }

	public IReadOnlyList<LocString> PlayerDeadLines { get; set; } = Array.Empty<LocString>();

	public event MerchantOpenedEventHandler MerchantOpened
	{
		add
		{
			backing_MerchantOpened = (MerchantOpenedEventHandler)Delegate.Combine(backing_MerchantOpened, value);
		}
		remove
		{
			backing_MerchantOpened = (MerchantOpenedEventHandler)Delegate.Remove(backing_MerchantOpened, value);
		}
	}

	public override void _Ready()
	{
		ConnectSignals();
		_merchantSelectionReticle = GetNode<NSelectionReticle>("%MerchantSelectionReticle");
		MegaSprite megaSprite = new MegaSprite(GetNode("%MerchantVisual"));
		_merchantSkeleton = megaSprite.GetSkeleton();
		megaSprite.GetAnimationState().SetAnimation("idle_loop");
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		RefreshFocus();
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_merchantSelectionReticle.OnDeselect();
		if (_focusedWhileTargeting)
		{
			NTargetManager.Instance.OnNodeUnhovered(this);
		}
		else
		{
			_merchantSkeleton.SetSkinByName("default");
			_merchantSkeleton.SetSlotsToSetupPose();
		}
		_focusedWhileTargeting = false;
	}

	protected override void OnRelease()
	{
		if (_focusedWhileTargeting)
		{
			_merchantSelectionReticle.OnDeselect();
			_focusedWhileTargeting = false;
			RefreshFocus();
		}
		else if (IsLocalPlayerDead)
		{
			LocString locString = Rng.Chaotic.NextItem(PlayerDeadLines);
			if (locString != null)
			{
				PlayDialogue(locString);
			}
		}
		else
		{
			EmitSignalMerchantOpened(this);
		}
	}

	public NSpeechBubbleVfx? PlayDialogue(LocString line, double duration = 2.0)
	{
		NSpeechBubbleVfx nSpeechBubbleVfx = NSpeechBubbleVfx.Create(line.GetFormattedText(), DialogueSide.Right, base.GlobalPosition + base.Size.X * Vector2.Left, duration, VfxColor.Blue);
		if (nSpeechBubbleVfx != null)
		{
			GetParent().AddChildSafely(nSpeechBubbleVfx);
		}
		return nSpeechBubbleVfx;
	}

	private void RefreshFocus()
	{
		if (NTargetManager.Instance.IsInSelection && NTargetManager.Instance.AllowedToTargetNode(this))
		{
			NTargetManager.Instance.OnNodeHovered(this);
			_merchantSelectionReticle.OnSelect();
			_focusedWhileTargeting = true;
		}
		else
		{
			_merchantSkeleton.SetSkinByName("outline");
			_merchantSkeleton.SetSlotsToSetupPose();
			_focusedWhileTargeting = false;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshFocus && args.Count == 0)
		{
			RefreshFocus();
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.RefreshFocus)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsLocalPlayerDead)
		{
			IsLocalPlayerDead = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._merchantSelectionReticle)
		{
			_merchantSelectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._focusedWhileTargeting)
		{
			_focusedWhileTargeting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName.IsLocalPlayerDead)
		{
			value = VariantUtils.CreateFrom<bool>(IsLocalPlayerDead);
			return true;
		}
		if (name == PropertyName._merchantSelectionReticle)
		{
			value = VariantUtils.CreateFrom(in _merchantSelectionReticle);
			return true;
		}
		if (name == PropertyName._focusedWhileTargeting)
		{
			value = VariantUtils.CreateFrom(in _focusedWhileTargeting);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._merchantSelectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._focusedWhileTargeting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsLocalPlayerDead, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsLocalPlayerDead, Variant.From<bool>(IsLocalPlayerDead));
		info.AddProperty(PropertyName._merchantSelectionReticle, Variant.From(in _merchantSelectionReticle));
		info.AddProperty(PropertyName._focusedWhileTargeting, Variant.From(in _focusedWhileTargeting));
		info.AddSignalEventDelegate(SignalName.MerchantOpened, backing_MerchantOpened);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsLocalPlayerDead, out var value))
		{
			IsLocalPlayerDead = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._merchantSelectionReticle, out var value2))
		{
			_merchantSelectionReticle = value2.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._focusedWhileTargeting, out var value3))
		{
			_focusedWhileTargeting = value3.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<MerchantOpenedEventHandler>(SignalName.MerchantOpened, out var value4))
		{
			backing_MerchantOpened = value4;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.MerchantOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "merchantButton", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalMerchantOpened(NMerchantButton merchantButton)
	{
		EmitSignal(SignalName.MerchantOpened, merchantButton);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.MerchantOpened && args.Count == 1)
		{
			backing_MerchantOpened?.Invoke(VariantUtils.ConvertTo<NMerchantButton>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.MerchantOpened)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
