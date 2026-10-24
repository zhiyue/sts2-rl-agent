using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Intents;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NIntent.cs")]
public class NIntent : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the 'DebugToggleVisibility' method.
		/// </summary>
		public static readonly StringName DebugToggleVisibility = "DebugToggleVisibility";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'UpdateVisuals' method.
		/// </summary>
		public static readonly StringName UpdateVisuals = "UpdateVisuals";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'PlayPerform' method.
		/// </summary>
		public static readonly StringName PlayPerform = "PlayPerform";

		/// <summary>
		/// Cached name for the 'SetFrozen' method.
		/// </summary>
		public static readonly StringName SetFrozen = "SetFrozen";

		/// <summary>
		/// Cached name for the 'OnHovered' method.
		/// </summary>
		public static readonly StringName OnHovered = "OnHovered";

		/// <summary>
		/// Cached name for the 'OnUnhovered' method.
		/// </summary>
		public static readonly StringName OnUnhovered = "OnUnhovered";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_intentHolder' field.
		/// </summary>
		public static readonly StringName _intentHolder = "_intentHolder";

		/// <summary>
		/// Cached name for the '_intentSprite' field.
		/// </summary>
		public static readonly StringName _intentSprite = "_intentSprite";

		/// <summary>
		/// Cached name for the '_valueLabel' field.
		/// </summary>
		public static readonly StringName _valueLabel = "_valueLabel";

		/// <summary>
		/// Cached name for the '_intentParticle' field.
		/// </summary>
		public static readonly StringName _intentParticle = "_intentParticle";

		/// <summary>
		/// Cached name for the '_timeOffset' field.
		/// </summary>
		public static readonly StringName _timeOffset = "_timeOffset";

		/// <summary>
		/// Cached name for the '_timeAccumulator' field.
		/// </summary>
		public static readonly StringName _timeAccumulator = "_timeAccumulator";

		/// <summary>
		/// Cached name for the '_isFrozen' field.
		/// </summary>
		public static readonly StringName _isFrozen = "_isFrozen";

		/// <summary>
		/// Cached name for the '_animationName' field.
		/// </summary>
		public static readonly StringName _animationName = "_animationName";

		/// <summary>
		/// Cached name for the '_combatRoom' field.
		/// </summary>
		public static readonly StringName _combatRoom = "_combatRoom";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "res://scenes/combat/intent.tscn";

	private const float _bobSpeed = (float)Math.PI;

	private const float _bobDistance = 10f;

	private const float _bobOffset = 8f;

	private const int _animationFps = 15;

	private Control _intentHolder;

	private Sprite2D _intentSprite;

	private MegaRichTextLabel _valueLabel;

	private CpuParticles2D _intentParticle;

	private Creature _owner;

	private IEnumerable<Creature> _targets;

	private AbstractIntent _intent;

	private float _timeOffset;

	private float _timeAccumulator;

	private bool _isFrozen;

	private string? _animationName;

	private readonly List<Texture2D> _animationFrames = new List<Texture2D>();

	private int? _animationFrame;

	private NCombatRoom? _combatRoom;

	public static IEnumerable<string> AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add("res://scenes/combat/intent.tscn");
			list.AddRange(IntentAnimData.AssetPaths);
			return new _003C_003Ez__ReadOnlyList<string>(list);
		}
	}

	public override void _Ready()
	{
		_intentHolder = GetNode<Control>("%IntentHolder");
		_intentSprite = GetNode<Sprite2D>("%Intent");
		_valueLabel = GetNode<MegaRichTextLabel>("%Value");
		_intentParticle = GetNode<CpuParticles2D>("%IntentParticle");
		Connect(Control.SignalName.MouseEntered, Callable.From(OnHovered));
		Connect(Control.SignalName.MouseExited, Callable.From(OnUnhovered));
		_intentHolder.Modulate = (NCombatUi.IsDebugHidingIntent ? Colors.Transparent : Colors.White);
	}

	public override void _EnterTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
		if (NCombatRoom.Instance != null)
		{
			_combatRoom = NCombatRoom.Instance;
			_combatRoom.Ui.DebugToggleIntent += DebugToggleVisibility;
		}
	}

	private void DebugToggleVisibility()
	{
		_intentHolder.Modulate = (NCombatUi.IsDebugHidingIntent ? Colors.Transparent : Colors.White);
	}

	public override void _ExitTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
		if (_combatRoom != null)
		{
			_combatRoom.Ui.DebugToggleIntent -= DebugToggleVisibility;
		}
	}

	public void UpdateIntent(AbstractIntent intent, IEnumerable<Creature> targets, Creature owner)
	{
		_owner = owner;
		_targets = targets;
		_intent = intent;
		UpdateVisuals();
	}

	private void OnCombatStateChanged(CombatState _)
	{
		if (!_isFrozen)
		{
			UpdateVisuals();
		}
	}

	private void UpdateVisuals()
	{
		string animation = _intent.GetAnimation(_targets, _owner);
		if (_animationName != animation)
		{
			_animationName = animation;
			_animationFrame = null;
			_timeAccumulator = 0f;
			int animationFrameCount = IntentAnimData.GetAnimationFrameCount(animation);
			_animationFrames.Clear();
			for (int i = 0; i < animationFrameCount; i++)
			{
				_animationFrames.Add(PreloadManager.Cache.GetTexture2D(IntentAnimData.GetAnimationFrame(animation, i)));
			}
		}
		_intentParticle.Texture = _intent.GetTexture(_targets, _owner);
		MegaRichTextLabel valueLabel = _valueLabel;
		AbstractIntent intent = _intent;
		string text = ((intent is AttackIntent attackIntent) ? (attackIntent.GetIntentLabel(_targets, _owner).GetFormattedText() ?? "") : ((!(intent is StatusIntent)) ? string.Empty : (_intent.GetIntentLabel(_targets, _owner).GetFormattedText() ?? "")));
		valueLabel.Text = text;
	}

	public override void _Process(double delta)
	{
		_intentHolder.Position = Vector2.Up * (Mathf.Sin((float)Time.GetTicksMsec() * 0.001f * (float)Math.PI + _timeOffset) * 10f + 8f);
		if (_animationFrames.Count > 0)
		{
			int num = (int)(_timeAccumulator * 15f) % _animationFrames.Count;
			if (_animationFrame != num)
			{
				_animationFrame = num;
				_intentSprite.Texture = _animationFrames[num];
			}
			_timeAccumulator += (float)delta;
		}
	}

	public static NIntent Create(float startTime)
	{
		NIntent nIntent = PreloadManager.Cache.GetScene("res://scenes/combat/intent.tscn").Instantiate<NIntent>(PackedScene.GenEditState.Disabled);
		nIntent._timeOffset = startTime;
		return nIntent;
	}

	public void PlayPerform()
	{
		_intentParticle.Emitting = true;
	}

	public void SetFrozen(bool isFrozen)
	{
		_isFrozen = isFrozen;
	}

	private void OnHovered()
	{
		if (_intent.HasIntentTip)
		{
			NCombatRoom.Instance?.GetCreatureNode(_owner)?.ShowHoverTips(new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(_intent.GetHoverTip(_targets, _owner)));
		}
	}

	private void OnUnhovered()
	{
		NCombatRoom.Instance?.GetCreatureNode(_owner)?.HideHoverTips();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DebugToggleVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "startTime", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayPerform, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFrozen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isFrozen", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugToggleVisibility && args.Count == 0)
		{
			DebugToggleVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateVisuals && args.Count == 0)
		{
			UpdateVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NIntent>(Create(VariantUtils.ConvertTo<float>(in args[0])));
			return true;
		}
		if (method == MethodName.PlayPerform && args.Count == 0)
		{
			PlayPerform();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetFrozen && args.Count == 1)
		{
			SetFrozen(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHovered && args.Count == 0)
		{
			OnHovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnhovered && args.Count == 0)
		{
			OnUnhovered();
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
			ret = VariantUtils.CreateFrom<NIntent>(Create(VariantUtils.ConvertTo<float>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName.DebugToggleVisibility)
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.PlayPerform)
		{
			return true;
		}
		if (method == MethodName.SetFrozen)
		{
			return true;
		}
		if (method == MethodName.OnHovered)
		{
			return true;
		}
		if (method == MethodName.OnUnhovered)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._intentHolder)
		{
			_intentHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._intentSprite)
		{
			_intentSprite = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._valueLabel)
		{
			_valueLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._intentParticle)
		{
			_intentParticle = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._timeOffset)
		{
			_timeOffset = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._timeAccumulator)
		{
			_timeAccumulator = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isFrozen)
		{
			_isFrozen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._animationName)
		{
			_animationName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._combatRoom)
		{
			_combatRoom = VariantUtils.ConvertTo<NCombatRoom>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._intentHolder)
		{
			value = VariantUtils.CreateFrom(in _intentHolder);
			return true;
		}
		if (name == PropertyName._intentSprite)
		{
			value = VariantUtils.CreateFrom(in _intentSprite);
			return true;
		}
		if (name == PropertyName._valueLabel)
		{
			value = VariantUtils.CreateFrom(in _valueLabel);
			return true;
		}
		if (name == PropertyName._intentParticle)
		{
			value = VariantUtils.CreateFrom(in _intentParticle);
			return true;
		}
		if (name == PropertyName._timeOffset)
		{
			value = VariantUtils.CreateFrom(in _timeOffset);
			return true;
		}
		if (name == PropertyName._timeAccumulator)
		{
			value = VariantUtils.CreateFrom(in _timeAccumulator);
			return true;
		}
		if (name == PropertyName._isFrozen)
		{
			value = VariantUtils.CreateFrom(in _isFrozen);
			return true;
		}
		if (name == PropertyName._animationName)
		{
			value = VariantUtils.CreateFrom(in _animationName);
			return true;
		}
		if (name == PropertyName._combatRoom)
		{
			value = VariantUtils.CreateFrom(in _combatRoom);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._intentHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._intentSprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._valueLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._intentParticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._timeOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._timeAccumulator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isFrozen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._animationName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._intentHolder, Variant.From(in _intentHolder));
		info.AddProperty(PropertyName._intentSprite, Variant.From(in _intentSprite));
		info.AddProperty(PropertyName._valueLabel, Variant.From(in _valueLabel));
		info.AddProperty(PropertyName._intentParticle, Variant.From(in _intentParticle));
		info.AddProperty(PropertyName._timeOffset, Variant.From(in _timeOffset));
		info.AddProperty(PropertyName._timeAccumulator, Variant.From(in _timeAccumulator));
		info.AddProperty(PropertyName._isFrozen, Variant.From(in _isFrozen));
		info.AddProperty(PropertyName._animationName, Variant.From(in _animationName));
		info.AddProperty(PropertyName._combatRoom, Variant.From(in _combatRoom));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._intentHolder, out var value))
		{
			_intentHolder = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._intentSprite, out var value2))
		{
			_intentSprite = value2.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._valueLabel, out var value3))
		{
			_valueLabel = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._intentParticle, out var value4))
		{
			_intentParticle = value4.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._timeOffset, out var value5))
		{
			_timeOffset = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._timeAccumulator, out var value6))
		{
			_timeAccumulator = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isFrozen, out var value7))
		{
			_isFrozen = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._animationName, out var value8))
		{
			_animationName = value8.As<string>();
		}
		if (info.TryGetProperty(PropertyName._combatRoom, out var value9))
		{
			_combatRoom = value9.As<NCombatRoom>();
		}
	}
}
