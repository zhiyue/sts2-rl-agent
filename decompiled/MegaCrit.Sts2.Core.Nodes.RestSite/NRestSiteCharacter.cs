using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.RestSite;

[ScriptPath("res://src/Core/Nodes/RestSite/NRestSiteCharacter.cs")]
public class NRestSiteCharacter : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RandomizeFire = "RandomizeFire";

		public static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName Deselect = "Deselect";

		public static readonly StringName FlipX = "FlipX";

		public static readonly StringName HideFlameGlow = "HideFlameGlow";

		public static readonly StringName RefreshThoughtBubbleVfx = "RefreshThoughtBubbleVfx";

		public static readonly StringName Shake = "Shake";

		public static readonly StringName GetRestSiteOptionAnchor = "GetRestSiteOptionAnchor";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName Hitbox = "Hitbox";

		public static readonly StringName _controlRoot = "_controlRoot";

		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _leftThoughtAnchor = "_leftThoughtAnchor";

		public static readonly StringName _rightThoughtAnchor = "_rightThoughtAnchor";

		public static readonly StringName _characterIndex = "_characterIndex";

		public static readonly StringName _thoughtBubbleVfx = "_thoughtBubbleVfx";

		public static readonly StringName _selectedOptionConfirmation = "_selectedOptionConfirmation";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly StringName _noise2Panning = new StringName("Noise2Panning");

	private static readonly StringName _noise1Panning = new StringName("Noise1Panning");

	private static readonly StringName _globalOffset = new StringName("GlobalOffset");

	private static readonly Vector2 _multiplayerConfirmationOffset = new Vector2(-25f, -123f);

	private static readonly Vector2 _multiplayerConfirmationFlipOffset = new Vector2(-155f, 0f);

	private static readonly string _multiplayerConfirmationScenePath = SceneHelper.GetScenePath("rest_site/rest_site_multiplayer_confirmation");

	private Control _controlRoot;

	private NSelectionReticle _selectionReticle;

	private Control _leftThoughtAnchor;

	private Control _rightThoughtAnchor;

	private int _characterIndex;

	private NThoughtBubbleVfx? _thoughtBubbleVfx;

	private CancellationTokenSource? _thoughtBubbleGoAwayCancellation;

	private Control? _selectedOptionConfirmation;

	private RestSiteOption? _hoveredRestSiteOption;

	private RestSiteOption? _selectingRestSiteOption;

	private RestSiteOption? _restSiteOptionInThoughtBubble;

	public Control Hitbox { get; private set; }

	public Player Player { get; private set; }

	public static NRestSiteCharacter Create(Player player, int characterIndex)
	{
		NRestSiteCharacter nRestSiteCharacter = PreloadManager.Cache.GetScene(player.Character.RestSiteAnimPath).Instantiate<NRestSiteCharacter>(PackedScene.GenEditState.Disabled);
		nRestSiteCharacter.Player = player;
		nRestSiteCharacter._characterIndex = characterIndex;
		return nRestSiteCharacter;
	}

	public override void _Ready()
	{
		_controlRoot = GetNode<Control>("ControlRoot");
		Hitbox = GetNode<Control>("%Hitbox");
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		_leftThoughtAnchor = GetNode<Control>("%ThoughtBubbleLeft");
		_rightThoughtAnchor = GetNode<Control>("%ThoughtBubbleRight");
		string animationName = Player.RunState.CurrentActIndex switch
		{
			0 => "overgrowth_loop", 
			1 => "hive_loop", 
			2 => "glory_loop", 
			_ => throw new InvalidOperationException("Unexpected act"), 
		};
		foreach (Node2D childSpineNode in GetChildSpineNodes())
		{
			MegaTrackEntry megaTrackEntry = new MegaSprite(childSpineNode).GetAnimationState().SetAnimation(animationName);
			megaTrackEntry?.SetTrackTime(megaTrackEntry.GetAnimationEnd() * Rng.Chaotic.NextFloat());
		}
		if (Player.Character is Necrobinder)
		{
			Sprite2D node = GetNode<Sprite2D>("%NecroFire");
			Sprite2D node2 = GetNode<Sprite2D>("%OstyFire");
			RandomizeFire((ShaderMaterial)node.Material);
			RandomizeFire((ShaderMaterial)node2.Material);
			if (_characterIndex >= 2)
			{
				Node2D node3 = GetNode<Node2D>("Osty");
				Node2D node4 = GetNode<Node2D>("OstyRightAnchor");
				node3.Position = node4.Position;
				MoveChild(node3, 0);
			}
		}
		Hitbox.Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
		Hitbox.Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
		Hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnFocus));
		Hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));
	}

	private void RandomizeFire(ShaderMaterial mat)
	{
		mat.SetShaderParameter(_globalOffset, new Vector2(Rng.Chaotic.NextFloat(-50f, 50f), Rng.Chaotic.NextFloat(-50f, 50f)));
		mat.SetShaderParameter(_noise1Panning, mat.GetShaderParameter(_noise1Panning).AsVector2() + new Vector2(Rng.Chaotic.NextFloat(-0.1f, 0.1f), Rng.Chaotic.NextFloat(-0.1f, 0.1f)));
		mat.SetShaderParameter(_noise2Panning, mat.GetShaderParameter(_noise2Panning).AsVector2() + new Vector2(Rng.Chaotic.NextFloat(-0.1f, 0.1f), Rng.Chaotic.NextFloat(-0.1f, 0.1f)));
	}

	private void OnFocus()
	{
		if (NTargetManager.Instance.IsInSelection && NTargetManager.Instance.AllowedToTargetNode(this))
		{
			NTargetManager.Instance.OnNodeHovered(this);
			_selectionReticle.OnSelect();
			NRun.Instance.GlobalUi.MultiplayerPlayerContainer.HighlightPlayer(Player);
		}
	}

	private void OnUnfocus()
	{
		if (NTargetManager.Instance.IsInSelection && NTargetManager.Instance.AllowedToTargetNode(this))
		{
			NTargetManager.Instance.OnNodeUnhovered(this);
		}
		Deselect();
	}

	public void Deselect()
	{
		if (_selectionReticle.IsSelected)
		{
			_selectionReticle.OnDeselect();
		}
		NRun.Instance.GlobalUi.MultiplayerPlayerContainer.UnhighlightPlayer(Player);
	}

	public void FlipX()
	{
		Vector2 scale;
		foreach (Node2D childSpineNode in GetChildSpineNodes())
		{
			scale = childSpineNode.Scale;
			scale.X = 0f - childSpineNode.Scale.X;
			childSpineNode.Scale = scale;
			scale = childSpineNode.Position;
			scale.X = 0f - childSpineNode.Position.X;
			childSpineNode.Position = scale;
		}
		Control controlRoot = _controlRoot;
		scale = _controlRoot.Scale;
		scale.X = 0f - _controlRoot.Scale.X;
		controlRoot.Scale = scale;
	}

	public void HideFlameGlow()
	{
		foreach (Node2D childSpineNode in GetChildSpineNodes())
		{
			MegaSprite megaSprite = new MegaSprite(childSpineNode);
			if (megaSprite.HasAnimation("_tracks/light_off"))
			{
				megaSprite.GetAnimationState().SetAnimation("_tracks/light_off", loop: true, 1);
			}
		}
	}

	public void ShowHoveredRestSiteOption(RestSiteOption? option)
	{
		_hoveredRestSiteOption = option;
		RefreshThoughtBubbleVfx();
	}

	public void SetSelectingRestSiteOption(RestSiteOption? option)
	{
		_selectingRestSiteOption = option;
		RefreshThoughtBubbleVfx();
	}

	private void RefreshThoughtBubbleVfx()
	{
		if (_selectedOptionConfirmation != null)
		{
			return;
		}
		RestSiteOption restSiteOption = _selectingRestSiteOption ?? _hoveredRestSiteOption;
		if (_restSiteOptionInThoughtBubble == restSiteOption)
		{
			return;
		}
		_restSiteOptionInThoughtBubble = restSiteOption;
		if (restSiteOption == null)
		{
			TaskHelper.RunSafely(RemoveThoughtBubbleAfterDelay());
			return;
		}
		_thoughtBubbleGoAwayCancellation?.Cancel();
		if (_thoughtBubbleVfx == null)
		{
			int characterIndex = _characterIndex;
			bool flag = ((characterIndex == 0 || characterIndex == 3) ? true : false);
			bool flag2 = flag;
			_thoughtBubbleVfx = NThoughtBubbleVfx.Create(restSiteOption.Icon, (!flag2) ? DialogueSide.Left : DialogueSide.Right, null);
			ShaderMaterial shaderMaterial = (ShaderMaterial)_thoughtBubbleVfx.GetNode<TextureRect>("%Image").Material;
			shaderMaterial.SetShaderParameter(_s, 0.145f);
			shaderMaterial.SetShaderParameter(_v, 0.85f);
			this.AddChildSafely(_thoughtBubbleVfx);
			_thoughtBubbleVfx.GlobalPosition = GetRestSiteOptionAnchor().GlobalPosition;
		}
		else
		{
			_thoughtBubbleVfx.SetTexture(restSiteOption.Icon);
		}
	}

	public void ShowSelectedRestSiteOption(RestSiteOption option)
	{
		_thoughtBubbleVfx?.GoAway();
		_thoughtBubbleVfx = null;
		_selectedOptionConfirmation = PreloadManager.Cache.GetScene(_multiplayerConfirmationScenePath).Instantiate<Control>(PackedScene.GenEditState.Disabled);
		_selectedOptionConfirmation.GetNode<TextureRect>("%Icon").Texture = option.Icon;
		this.AddChildSafely(_selectedOptionConfirmation);
		int characterIndex = _characterIndex;
		bool flag = ((characterIndex == 0 || characterIndex == 3) ? true : false);
		bool flag2 = flag;
		_selectedOptionConfirmation.GlobalPosition = GetRestSiteOptionAnchor().GlobalPosition;
		_selectedOptionConfirmation.Position += _multiplayerConfirmationOffset + (flag2 ? _multiplayerConfirmationFlipOffset : Vector2.Zero);
	}

	public void Shake()
	{
		TaskHelper.RunSafely(DoShake());
	}

	private async Task DoShake()
	{
		ScreenPunchInstance shake = new ScreenPunchInstance(15f, 0.4, 0f);
		Vector2 originalPosition = base.Position;
		while (!shake.IsDone)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			Vector2 vector = shake.Update(GetProcessDeltaTime());
			base.Position = originalPosition + vector;
		}
		base.Position = originalPosition;
	}

	private Control GetRestSiteOptionAnchor()
	{
		if (_characterIndex < 2)
		{
			return _leftThoughtAnchor;
		}
		return _rightThoughtAnchor;
	}

	private async Task RemoveThoughtBubbleAfterDelay()
	{
		_thoughtBubbleGoAwayCancellation = new CancellationTokenSource();
		await Cmd.Wait(0.5f, _thoughtBubbleGoAwayCancellation.Token);
		if (!_thoughtBubbleGoAwayCancellation.IsCancellationRequested)
		{
			_thoughtBubbleVfx?.GoAway();
			_thoughtBubbleVfx = null;
		}
	}

	private IEnumerable<Node2D> GetChildSpineNodes()
	{
		foreach (Node2D item in GetChildren().OfType<Node2D>())
		{
			if (!(item.GetClass() != "SpineSprite"))
			{
				yield return item;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RandomizeFire, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "mat", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("ShaderMaterial"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Deselect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FlipX, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideFlameGlow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshThoughtBubbleVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Shake, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetRestSiteOptionAnchor, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.RandomizeFire && args.Count == 1)
		{
			RandomizeFire(VariantUtils.ConvertTo<ShaderMaterial>(in args[0]));
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
		if (method == MethodName.Deselect && args.Count == 0)
		{
			Deselect();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlipX && args.Count == 0)
		{
			FlipX();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideFlameGlow && args.Count == 0)
		{
			HideFlameGlow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshThoughtBubbleVfx && args.Count == 0)
		{
			RefreshThoughtBubbleVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Shake && args.Count == 0)
		{
			Shake();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetRestSiteOptionAnchor && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Control>(GetRestSiteOptionAnchor());
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
		if (method == MethodName.RandomizeFire)
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
		if (method == MethodName.Deselect)
		{
			return true;
		}
		if (method == MethodName.FlipX)
		{
			return true;
		}
		if (method == MethodName.HideFlameGlow)
		{
			return true;
		}
		if (method == MethodName.RefreshThoughtBubbleVfx)
		{
			return true;
		}
		if (method == MethodName.Shake)
		{
			return true;
		}
		if (method == MethodName.GetRestSiteOptionAnchor)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Hitbox)
		{
			Hitbox = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._controlRoot)
		{
			_controlRoot = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._leftThoughtAnchor)
		{
			_leftThoughtAnchor = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._rightThoughtAnchor)
		{
			_rightThoughtAnchor = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._characterIndex)
		{
			_characterIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._thoughtBubbleVfx)
		{
			_thoughtBubbleVfx = VariantUtils.ConvertTo<NThoughtBubbleVfx>(in value);
			return true;
		}
		if (name == PropertyName._selectedOptionConfirmation)
		{
			_selectedOptionConfirmation = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hitbox)
		{
			value = VariantUtils.CreateFrom<Control>(Hitbox);
			return true;
		}
		if (name == PropertyName._controlRoot)
		{
			value = VariantUtils.CreateFrom(in _controlRoot);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._leftThoughtAnchor)
		{
			value = VariantUtils.CreateFrom(in _leftThoughtAnchor);
			return true;
		}
		if (name == PropertyName._rightThoughtAnchor)
		{
			value = VariantUtils.CreateFrom(in _rightThoughtAnchor);
			return true;
		}
		if (name == PropertyName._characterIndex)
		{
			value = VariantUtils.CreateFrom(in _characterIndex);
			return true;
		}
		if (name == PropertyName._thoughtBubbleVfx)
		{
			value = VariantUtils.CreateFrom(in _thoughtBubbleVfx);
			return true;
		}
		if (name == PropertyName._selectedOptionConfirmation)
		{
			value = VariantUtils.CreateFrom(in _selectedOptionConfirmation);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controlRoot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftThoughtAnchor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightThoughtAnchor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._characterIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._thoughtBubbleVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedOptionConfirmation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Hitbox, Variant.From<Control>(Hitbox));
		info.AddProperty(PropertyName._controlRoot, Variant.From(in _controlRoot));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._leftThoughtAnchor, Variant.From(in _leftThoughtAnchor));
		info.AddProperty(PropertyName._rightThoughtAnchor, Variant.From(in _rightThoughtAnchor));
		info.AddProperty(PropertyName._characterIndex, Variant.From(in _characterIndex));
		info.AddProperty(PropertyName._thoughtBubbleVfx, Variant.From(in _thoughtBubbleVfx));
		info.AddProperty(PropertyName._selectedOptionConfirmation, Variant.From(in _selectedOptionConfirmation));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Hitbox, out var value))
		{
			Hitbox = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._controlRoot, out var value2))
		{
			_controlRoot = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value3))
		{
			_selectionReticle = value3.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._leftThoughtAnchor, out var value4))
		{
			_leftThoughtAnchor = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._rightThoughtAnchor, out var value5))
		{
			_rightThoughtAnchor = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._characterIndex, out var value6))
		{
			_characterIndex = value6.As<int>();
		}
		if (info.TryGetProperty(PropertyName._thoughtBubbleVfx, out var value7))
		{
			_thoughtBubbleVfx = value7.As<NThoughtBubbleVfx>();
		}
		if (info.TryGetProperty(PropertyName._selectedOptionConfirmation, out var value8))
		{
			_selectedOptionConfirmation = value8.As<Control>();
		}
	}
}
