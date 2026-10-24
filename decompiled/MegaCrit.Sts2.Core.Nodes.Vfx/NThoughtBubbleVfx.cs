using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

/// <summary>
/// Thought bubble vfx used when creatures think during combat. Generally, this is the player character breaking the 4th wall.
/// Out of energy! No more cards in the draw pile. Etc
/// </summary>
[ScriptPath("res://src/Core/Nodes/Vfx/NThoughtBubbleVfx.cs")]
public class NThoughtBubbleVfx : Control
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
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'SetTexture' method.
		/// </summary>
		public static readonly StringName SetTexture = "SetTexture";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_container' field.
		/// </summary>
		public static readonly StringName _container = "_container";

		/// <summary>
		/// Cached name for the '_label' field.
		/// </summary>
		public static readonly StringName _label = "_label";

		/// <summary>
		/// Cached name for the '_textureRect' field.
		/// </summary>
		public static readonly StringName _textureRect = "_textureRect";

		/// <summary>
		/// Cached name for the '_contents' field.
		/// </summary>
		public static readonly StringName _contents = "_contents";

		/// <summary>
		/// Cached name for the '_tail' field.
		/// </summary>
		public static readonly StringName _tail = "_tail";

		/// <summary>
		/// Cached name for the '_style' field.
		/// </summary>
		public static readonly StringName _style = "_style";

		/// <summary>
		/// Cached name for the '_side' field.
		/// </summary>
		public static readonly StringName _side = "_side";

		/// <summary>
		/// Cached name for the '_text' field.
		/// </summary>
		public static readonly StringName _text = "_text";

		/// <summary>
		/// Cached name for the '_texture' field.
		/// </summary>
		public static readonly StringName _texture = "_texture";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private Control _container;

	private MegaRichTextLabel _label;

	private TextureRect _textureRect;

	private Node2D _contents;

	private Node2D _tail;

	private const string _path = "res://scenes/vfx/vfx_thought_bubble.tscn";

	private const float _spawnProportionToTopOfHitbox = 0.75f;

	private const float _spawnProportionToEdgeOfHitbox = 0.75f;

	private Vector2? _startPos;

	private DialogueStyle _style;

	private DialogueSide _side;

	private string? _text;

	private Texture2D? _texture;

	private double? _secondsToDisplay;

	private Tween? _tween;

	private CancellationTokenSource? _cts;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/vfx_thought_bubble.tscn");

	/// <summary>
	/// Creates a thought bubble given a creature. Use this when you want creatures to think out loud during combat!
	/// </summary>
	public static NThoughtBubbleVfx? Create(string text, Creature speaker, double? secondsToDisplay)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NThoughtBubbleVfx nThoughtBubbleVfx = CreateInternal(text, null, speaker.Side switch
		{
			CombatSide.Player => DialogueSide.Left, 
			CombatSide.Enemy => DialogueSide.Right, 
			_ => throw new ArgumentOutOfRangeException(), 
		}, secondsToDisplay);
		nThoughtBubbleVfx._startPos = GetCreatureSpeechPosition(speaker);
		return nThoughtBubbleVfx;
	}

	/// <summary>
	/// When we want to specify the absolute position of the thought bubble vfx.
	/// </summary>
	public static NThoughtBubbleVfx? Create(string text, DialogueSide side, double? secondsToDisplay)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return CreateInternal(text, null, side, secondsToDisplay);
	}

	/// <summary>
	/// When we want to specify the absolute position of the thought bubble vfx with an image.
	/// </summary>
	public static NThoughtBubbleVfx? Create(Texture2D texture, DialogueSide side, double? secondsToDisplay)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return CreateInternal(null, texture, side, secondsToDisplay);
	}

	public override void _Ready()
	{
		_contents = GetNode<Node2D>("%Contents");
		_container = GetNode<Control>("%Container");
		_label = GetNode<MegaRichTextLabel>("%Text");
		_textureRect = GetNode<TextureRect>("%Image");
		_tail = GetNode<Node2D>("%Tail");
		if (_startPos.HasValue)
		{
			base.GlobalPosition = _startPos.Value;
		}
		if (_side == DialogueSide.Right)
		{
			_container.Position = new Vector2(0f - _container.Size.X - _container.Position.X, _container.Position.Y);
			_tail.Scale = new Vector2(-1f, 1f);
		}
		TaskHelper.RunSafely(AnimateThoughtBubble());
	}

	private async Task AnimateThoughtBubble()
	{
		_tween = CreateTween().SetParallel();
		float value = 30f * ((_side == DialogueSide.Left) ? (-1f) : 1f);
		if (_text != null)
		{
			_label.Text = $"[center][fly_in offset_x={value} offset_y=40]{_text}[/fly_in][/center]";
		}
		_label.Visible = _text != null;
		if (_texture != null)
		{
			_textureRect.Texture = _texture;
		}
		_textureRect.Visible = _texture != null;
		base.Scale = Vector2.One * 0.75f;
		base.Modulate = StsColors.transparentWhite;
		_tween.TweenProperty(_label, "visible_ratio", 1f, 0.4).From(0f);
		_tween.TweenProperty(_textureRect, "modulate:a", 1f, 0.4).From(0f);
		_tween.TweenProperty(this, "modulate:a", 1f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(this, "scale", Vector2.One, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		if (_secondsToDisplay.HasValue)
		{
			double num = Math.Max(_secondsToDisplay.Value, 1.0);
			_cts = new CancellationTokenSource();
			await Cmd.Wait((float)num, _cts.Token);
			await GoAway();
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
		_cts?.Cancel();
	}

	public async Task GoAway()
	{
		if (GodotObject.IsInstanceValid(this))
		{
			_tween?.Kill();
			_tween = CreateTween();
			_tween.TweenProperty(this, "modulate:a", 0f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			await _tween.AwaitFinished(this);
			this.QueueFreeSafely();
		}
	}

	private static NThoughtBubbleVfx CreateInternal(string? text, Texture2D? texture, DialogueSide side, double? secondsToDisplay)
	{
		NThoughtBubbleVfx nThoughtBubbleVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/vfx_thought_bubble.tscn").Instantiate<NThoughtBubbleVfx>(PackedScene.GenEditState.Disabled);
		nThoughtBubbleVfx._side = side;
		nThoughtBubbleVfx._text = text;
		nThoughtBubbleVfx._texture = texture;
		nThoughtBubbleVfx._secondsToDisplay = secondsToDisplay;
		return nThoughtBubbleVfx;
	}

	/// <summary>
	/// Given a creature, dynamically calculates and returns the position of where the "mouth" should be.
	/// Used during combat, where we don't know which side a creature could be on.
	/// </summary>
	/// <param name="speaker"></param>
	/// <returns></returns>
	public static Vector2 GetCreatureSpeechPosition(Creature speaker)
	{
		NCreature creatureNode = speaker.GetCreatureNode();
		if (creatureNode == null)
		{
			return Vector2.Zero;
		}
		if (creatureNode.Visuals.TalkPosition != null)
		{
			return creatureNode.Visuals.TalkPosition.GlobalPosition;
		}
		Vector2 result = creatureNode.VfxSpawnPosition + new Vector2(0f, (0f - creatureNode.Hitbox.Size.Y) * 0.5f * 0.75f);
		if (speaker.Side == CombatSide.Player)
		{
			result.X += creatureNode.Hitbox.Size.X * 0.75f;
		}
		else
		{
			result.X -= creatureNode.Hitbox.Size.X * 0.75f;
		}
		return result;
	}

	public void SetTexture(Texture2D texture)
	{
		if (_texture == null)
		{
			throw new NotImplementedException("Can't set texture unless thought bubble was initialized with a texture");
		}
		_texture = texture;
		_textureRect.Texture = texture;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetTexture, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "texture", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false)
		}, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTexture && args.Count == 1)
		{
			SetTexture(VariantUtils.ConvertTo<Texture2D>(in args[0]));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.SetTexture)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._container)
		{
			_container = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._textureRect)
		{
			_textureRect = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._contents)
		{
			_contents = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._tail)
		{
			_tail = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._style)
		{
			_style = VariantUtils.ConvertTo<DialogueStyle>(in value);
			return true;
		}
		if (name == PropertyName._side)
		{
			_side = VariantUtils.ConvertTo<DialogueSide>(in value);
			return true;
		}
		if (name == PropertyName._text)
		{
			_text = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._texture)
		{
			_texture = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._container)
		{
			value = VariantUtils.CreateFrom(in _container);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._textureRect)
		{
			value = VariantUtils.CreateFrom(in _textureRect);
			return true;
		}
		if (name == PropertyName._contents)
		{
			value = VariantUtils.CreateFrom(in _contents);
			return true;
		}
		if (name == PropertyName._tail)
		{
			value = VariantUtils.CreateFrom(in _tail);
			return true;
		}
		if (name == PropertyName._style)
		{
			value = VariantUtils.CreateFrom(in _style);
			return true;
		}
		if (name == PropertyName._side)
		{
			value = VariantUtils.CreateFrom(in _side);
			return true;
		}
		if (name == PropertyName._text)
		{
			value = VariantUtils.CreateFrom(in _text);
			return true;
		}
		if (name == PropertyName._texture)
		{
			value = VariantUtils.CreateFrom(in _texture);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._container, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textureRect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._contents, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tail, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._style, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._side, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._text, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._texture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._container, Variant.From(in _container));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._textureRect, Variant.From(in _textureRect));
		info.AddProperty(PropertyName._contents, Variant.From(in _contents));
		info.AddProperty(PropertyName._tail, Variant.From(in _tail));
		info.AddProperty(PropertyName._style, Variant.From(in _style));
		info.AddProperty(PropertyName._side, Variant.From(in _side));
		info.AddProperty(PropertyName._text, Variant.From(in _text));
		info.AddProperty(PropertyName._texture, Variant.From(in _texture));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._container, out var value))
		{
			_container = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value2))
		{
			_label = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._textureRect, out var value3))
		{
			_textureRect = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._contents, out var value4))
		{
			_contents = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._tail, out var value5))
		{
			_tail = value5.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._style, out var value6))
		{
			_style = value6.As<DialogueStyle>();
		}
		if (info.TryGetProperty(PropertyName._side, out var value7))
		{
			_side = value7.As<DialogueSide>();
		}
		if (info.TryGetProperty(PropertyName._text, out var value8))
		{
			_text = value8.As<string>();
		}
		if (info.TryGetProperty(PropertyName._texture, out var value9))
		{
			_texture = value9.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value10))
		{
			_tween = value10.As<Tween>();
		}
	}
}
