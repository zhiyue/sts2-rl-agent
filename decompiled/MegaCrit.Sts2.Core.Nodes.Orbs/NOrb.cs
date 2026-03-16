using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Orbs;

[ScriptPath("res://src/Core/Nodes/Orbs/NOrb.cs")]
public class NOrb : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName UpdateVisuals = "UpdateVisuals";

		public static readonly StringName Flash = "Flash";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _outline = "_outline";

		public static readonly StringName _visualContainer = "_visualContainer";

		public static readonly StringName _labelContainer = "_labelContainer";

		public static readonly StringName _passiveLabel = "_passiveLabel";

		public static readonly StringName _evokeLabel = "_evokeLabel";

		public static readonly StringName _bounds = "_bounds";

		public static readonly StringName _flashParticle = "_flashParticle";

		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _isLocal = "_isLocal";

		public static readonly StringName _sprite = "_sprite";

		public static readonly StringName _curTween = "_curTween";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private TextureRect _outline;

	private Control _visualContainer;

	private Control _labelContainer;

	private MegaLabel _passiveLabel;

	private MegaLabel _evokeLabel;

	private Control _bounds;

	private CpuParticles2D _flashParticle;

	private NSelectionReticle _selectionReticle;

	private bool _isLocal;

	private Node2D? _sprite;

	private Tween? _curTween;

	public OrbModel? Model { get; private set; }

	private static string ScenePath => SceneHelper.GetScenePath("/orbs/orb");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NOrb Create(bool isLocal)
	{
		NOrb nOrb = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NOrb>(PackedScene.GenEditState.Disabled);
		nOrb._isLocal = isLocal;
		return nOrb;
	}

	public static NOrb Create(bool isLocal, OrbModel? model)
	{
		NOrb nOrb = Create(isLocal);
		nOrb.Model = model;
		return nOrb;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_outline = GetNode<TextureRect>("%Outline");
		_visualContainer = GetNode<Control>("%VisualContainer");
		_passiveLabel = GetNode<MegaLabel>("%PassiveAmount");
		_evokeLabel = GetNode<MegaLabel>("%EvokeAmount");
		_flashParticle = GetNode<CpuParticles2D>("%Flash");
		_bounds = GetNode<Control>("Bounds");
		_labelContainer = GetNode<Control>("%LabelContainer");
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		if (Model != null)
		{
			CreateTween().TweenProperty(_outline, "scale", Vector2.One, 0.25).From(Vector2.Zero);
		}
		if (_isLocal)
		{
			base.Scale *= 0.85f;
		}
		UpdateVisuals(isEvoking: false);
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (Model != null)
		{
			Model.Triggered += Flash;
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (Model != null)
		{
			Model.Triggered -= Flash;
		}
	}

	public void ReplaceOrb(OrbModel model)
	{
		_sprite?.QueueFreeSafely();
		_sprite = null;
		Model = model;
		UpdateVisuals(isEvoking: false);
	}

	public void UpdateVisuals(bool isEvoking)
	{
		if (!IsNodeReady() || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		if (Model == null)
		{
			_sprite?.QueueFreeSafely();
			_passiveLabel.Visible = false;
			_evokeLabel.Visible = false;
			_outline.Visible = _isLocal;
			_flashParticle.Visible = false;
			return;
		}
		if (_sprite == null)
		{
			_sprite = Model.CreateSprite();
			_visualContainer.AddChildSafely(_sprite);
			_sprite.Position = Vector2.Zero;
			_curTween?.Kill();
			_curTween = CreateTween();
			_curTween.TweenProperty(_sprite, "scale", Vector2.One, 0.5).From(Vector2.Zero).SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);
		}
		_outline.Visible = false;
		_flashParticle.Visible = true;
		_flashParticle.Texture = Model.Icon;
		_labelContainer.Visible = _isLocal;
		if (!_isLocal)
		{
			base.Modulate = Model.DarkenedColor;
		}
		OrbModel model = Model;
		if (!(model is PlasmaOrb))
		{
			if (!(model is DarkOrb))
			{
				if (model is GlassOrb)
				{
					_passiveLabel.Visible = !isEvoking;
					_evokeLabel.Visible = isEvoking;
					_sprite.Modulate = ((Model.PassiveVal == 0m) ? Model.DarkenedColor : Colors.White);
					_passiveLabel.SetTextAutoSize(Model.PassiveVal.ToString("0"));
					_evokeLabel.SetTextAutoSize(Model.EvokeVal.ToString("0"));
				}
				else
				{
					_passiveLabel.Visible = !isEvoking;
					_evokeLabel.Visible = isEvoking;
					_passiveLabel.SetTextAutoSize(Model.PassiveVal.ToString("0"));
					_evokeLabel.SetTextAutoSize(Model.EvokeVal.ToString("0"));
				}
			}
			else
			{
				_passiveLabel.Visible = true;
				_evokeLabel.Visible = true;
				_passiveLabel.SetTextAutoSize(Model.PassiveVal.ToString("0"));
				_evokeLabel.SetTextAutoSize(Model.EvokeVal.ToString("0"));
			}
		}
		else
		{
			_passiveLabel.Visible = false;
			_evokeLabel.Visible = false;
		}
	}

	private void Flash()
	{
		_flashParticle.Emitting = true;
	}

	protected override void OnFocus()
	{
		if (Model != null || _isLocal)
		{
			IEnumerable<IHoverTip> enumerable;
			if (Model != null)
			{
				enumerable = Model.HoverTips;
			}
			else
			{
				IEnumerable<IHoverTip> enumerable2 = new List<IHoverTip> { OrbModel.EmptySlotHoverTipHoverTip };
				enumerable = enumerable2;
			}
			IEnumerable<IHoverTip> hoverTips = enumerable;
			NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(_bounds, hoverTips, HoverTip.GetHoverTipAlignment(_bounds));
			nHoverTipSet.SetFollowOwner();
			_labelContainer.Visible = true;
			base.Modulate = Colors.White;
			if (NControllerManager.Instance.IsUsingController)
			{
				_selectionReticle.OnSelect();
			}
		}
	}

	protected override void OnUnfocus()
	{
		_labelContainer.Visible = _isLocal;
		if (Model != null)
		{
			base.Modulate = (_isLocal ? Colors.White : Model.DarkenedColor);
		}
		NHoverTipSet.Remove(_bounds);
		_selectionReticle.OnDeselect();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isLocal", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isEvoking", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Flash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NOrb>(Create(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
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
		if (method == MethodName.UpdateVisuals && args.Count == 1)
		{
			UpdateVisuals(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Flash && args.Count == 0)
		{
			Flash();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NOrb>(Create(VariantUtils.ConvertTo<bool>(in args[0])));
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.UpdateVisuals)
		{
			return true;
		}
		if (method == MethodName.Flash)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._visualContainer)
		{
			_visualContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._labelContainer)
		{
			_labelContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._passiveLabel)
		{
			_passiveLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._evokeLabel)
		{
			_evokeLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._bounds)
		{
			_bounds = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._flashParticle)
		{
			_flashParticle = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._isLocal)
		{
			_isLocal = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._sprite)
		{
			_sprite = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._curTween)
		{
			_curTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._visualContainer)
		{
			value = VariantUtils.CreateFrom(in _visualContainer);
			return true;
		}
		if (name == PropertyName._labelContainer)
		{
			value = VariantUtils.CreateFrom(in _labelContainer);
			return true;
		}
		if (name == PropertyName._passiveLabel)
		{
			value = VariantUtils.CreateFrom(in _passiveLabel);
			return true;
		}
		if (name == PropertyName._evokeLabel)
		{
			value = VariantUtils.CreateFrom(in _evokeLabel);
			return true;
		}
		if (name == PropertyName._bounds)
		{
			value = VariantUtils.CreateFrom(in _bounds);
			return true;
		}
		if (name == PropertyName._flashParticle)
		{
			value = VariantUtils.CreateFrom(in _flashParticle);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._isLocal)
		{
			value = VariantUtils.CreateFrom(in _isLocal);
			return true;
		}
		if (name == PropertyName._sprite)
		{
			value = VariantUtils.CreateFrom(in _sprite);
			return true;
		}
		if (name == PropertyName._curTween)
		{
			value = VariantUtils.CreateFrom(in _curTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._visualContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._labelContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._passiveLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._evokeLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bounds, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flashParticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isLocal, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._curTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._visualContainer, Variant.From(in _visualContainer));
		info.AddProperty(PropertyName._labelContainer, Variant.From(in _labelContainer));
		info.AddProperty(PropertyName._passiveLabel, Variant.From(in _passiveLabel));
		info.AddProperty(PropertyName._evokeLabel, Variant.From(in _evokeLabel));
		info.AddProperty(PropertyName._bounds, Variant.From(in _bounds));
		info.AddProperty(PropertyName._flashParticle, Variant.From(in _flashParticle));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._isLocal, Variant.From(in _isLocal));
		info.AddProperty(PropertyName._sprite, Variant.From(in _sprite));
		info.AddProperty(PropertyName._curTween, Variant.From(in _curTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._outline, out var value))
		{
			_outline = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._visualContainer, out var value2))
		{
			_visualContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._labelContainer, out var value3))
		{
			_labelContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._passiveLabel, out var value4))
		{
			_passiveLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._evokeLabel, out var value5))
		{
			_evokeLabel = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._bounds, out var value6))
		{
			_bounds = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._flashParticle, out var value7))
		{
			_flashParticle = value7.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value8))
		{
			_selectionReticle = value8.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._isLocal, out var value9))
		{
			_isLocal = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._sprite, out var value10))
		{
			_sprite = value10.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._curTween, out var value11))
		{
			_curTween = value11.As<Tween>();
		}
	}
}
