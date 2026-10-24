using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Potions;

[ScriptPath("res://src/Core/Nodes/Potions/NPotion.cs")]
public class NPotion : Control
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
		/// Cached name for the 'Reload' method.
		/// </summary>
		public static readonly StringName Reload = "Reload";

		/// <summary>
		/// Cached name for the 'DoFlash' method.
		/// </summary>
		public static readonly StringName DoFlash = "DoFlash";

		/// <summary>
		/// Cached name for the 'DoBounce' method.
		/// </summary>
		public static readonly StringName DoBounce = "DoBounce";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Image' property.
		/// </summary>
		public static readonly StringName Image = "Image";

		/// <summary>
		/// Cached name for the 'Outline' property.
		/// </summary>
		public static readonly StringName Outline = "Outline";

		/// <summary>
		/// Cached name for the '_container' field.
		/// </summary>
		public static readonly StringName _container = "_container";

		/// <summary>
		/// Cached name for the '_bounceTween' field.
		/// </summary>
		public static readonly StringName _bounceTween = "_bounceTween";

		/// <summary>
		/// Cached name for the '_obtainedTween' field.
		/// </summary>
		public static readonly StringName _obtainedTween = "_obtainedTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private const float _newlyAcquiredPopDuration = 0.35f;

	private const float _newlyAcquiredFadeInDuration = 0.1f;

	private const float _newlyAcquiredPopDistance = 40f;

	private PotionModel? _model;

	private Control _container;

	private Tween? _bounceTween;

	private Tween? _obtainedTween;

	private CancellationTokenSource? _cancellationTokenSource;

	public TextureRect Image { get; private set; }

	public TextureRect Outline { get; private set; }

	private static string ScenePath => SceneHelper.GetScenePath("/potions/potion");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2]
	{
		ScenePath,
		NPotionFlashVfx.ScenePath
	});

	public PotionModel Model
	{
		get
		{
			return _model ?? throw new InvalidOperationException("Model was accessed before it was set.");
		}
		set
		{
			_model = value;
			Reload();
		}
	}

	/// <summary>
	/// Create an instance of the node.
	/// Null if we're in test mode.
	/// </summary>
	public static NPotion? Create(PotionModel potion)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NPotion nPotion = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NPotion>(PackedScene.GenEditState.Disabled);
		nPotion.Model = potion;
		return nPotion;
	}

	public override void _Ready()
	{
		Image = GetNode<TextureRect>("%Image");
		Outline = GetNode<TextureRect>("%Outline");
		_container = GetNode<Control>("Container");
		Reload();
	}

	public override void _ExitTree()
	{
		_cancellationTokenSource?.Cancel();
	}

	private void Reload()
	{
		if (IsNodeReady() && _model != null)
		{
			Image.Texture = _model.Image;
			Outline.Texture = _model.Outline;
		}
	}

	public async Task PlayNewlyAcquiredAnimation(Vector2? startLocation)
	{
		if (_cancellationTokenSource != null)
		{
			await _cancellationTokenSource.CancelAsync();
		}
		CancellationTokenSource cancelTokenSource = (_cancellationTokenSource = new CancellationTokenSource());
		await this.AwaitProcessFrame();
		if (!cancelTokenSource.IsCancellationRequested)
		{
			_obtainedTween?.Kill();
			if (!startLocation.HasValue)
			{
				Control container = _container;
				Vector2 position = _container.Position;
				position.Y = 40f;
				container.Position = position;
				Control container2 = _container;
				Color modulate = _container.Modulate;
				modulate.A = 0f;
				container2.Modulate = modulate;
				_obtainedTween = GetTree().CreateTween();
				_obtainedTween.TweenProperty(_container, "modulate:a", 1f, 0.10000000149011612);
				_obtainedTween.Parallel();
				_obtainedTween.SetEase(Tween.EaseType.Out);
				_obtainedTween.SetTrans(Tween.TransitionType.Back);
				_obtainedTween.TweenProperty(_container, "position:y", 0f, 0.3499999940395355);
				_obtainedTween.TweenCallback(Callable.From(DoFlash));
			}
			else
			{
				_container.GlobalPosition = startLocation.Value;
				Control container3 = _container;
				Color modulate = _container.Modulate;
				modulate.A = 1f;
				container3.Modulate = modulate;
				_obtainedTween = GetTree().CreateTween();
				_obtainedTween.SetEase(Tween.EaseType.Out);
				_obtainedTween.SetTrans(Tween.TransitionType.Quad);
				_obtainedTween.TweenProperty(_container, "position", Vector2.Zero, 0.3499999940395355);
				_obtainedTween.TweenCallback(Callable.From(DoFlash));
			}
		}
	}

	private void DoFlash()
	{
		this.AddChildSafely(NPotionFlashVfx.Create(this));
	}

	public void DoBounce()
	{
		_bounceTween?.Kill();
		_bounceTween = CreateTween();
		_bounceTween.TweenProperty(_container, "position:y", base.Position.Y - 12f, 0.125).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		_bounceTween.TweenProperty(_container, "position:y", 0f, 0.125).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DoFlash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DoBounce, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoFlash && args.Count == 0)
		{
			DoFlash();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoBounce && args.Count == 0)
		{
			DoBounce();
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
		if (method == MethodName.Reload)
		{
			return true;
		}
		if (method == MethodName.DoFlash)
		{
			return true;
		}
		if (method == MethodName.DoBounce)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Image)
		{
			Image = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName.Outline)
		{
			Outline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._container)
		{
			_container = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bounceTween)
		{
			_bounceTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._obtainedTween)
		{
			_obtainedTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		TextureRect from;
		if (name == PropertyName.Image)
		{
			from = Image;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Outline)
		{
			from = Outline;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._container)
		{
			value = VariantUtils.CreateFrom(in _container);
			return true;
		}
		if (name == PropertyName._bounceTween)
		{
			value = VariantUtils.CreateFrom(in _bounceTween);
			return true;
		}
		if (name == PropertyName._obtainedTween)
		{
			value = VariantUtils.CreateFrom(in _obtainedTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bounceTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._obtainedTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Image, Variant.From<TextureRect>(Image));
		info.AddProperty(PropertyName.Outline, Variant.From<TextureRect>(Outline));
		info.AddProperty(PropertyName._container, Variant.From(in _container));
		info.AddProperty(PropertyName._bounceTween, Variant.From(in _bounceTween));
		info.AddProperty(PropertyName._obtainedTween, Variant.From(in _obtainedTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Image, out var value))
		{
			Image = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName.Outline, out var value2))
		{
			Outline = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._container, out var value3))
		{
			_container = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bounceTween, out var value4))
		{
			_bounceTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._obtainedTween, out var value5))
		{
			_obtainedTween = value5.As<Tween>();
		}
	}
}
