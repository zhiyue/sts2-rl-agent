using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;

[ScriptPath("res://src/Core/Nodes/Screens/FeedbackScreen/NSendFeedbackFlower.cs")]
public class NSendFeedbackFlower : Control
{
	public enum State
	{
		None,
		Nodding,
		Anticipation,
		NoddingFast
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetState = "SetState";

		public static readonly StringName SetRandomPosition = "SetRandomPosition";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Cartoon = "Cartoon";

		public static readonly StringName MyState = "MyState";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _originalPosition = "_originalPosition";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _normalImage = "res://images/atlases/compressed.sprites/feedback/flower.tres";

	private const string _noddingImage = "res://images/atlases/compressed.sprites/feedback/flower_happy.tres";

	private const string _anticipationImage = "res://images/atlases/compressed.sprites/feedback/flower_anticipation.tres";

	private Tween? _tween;

	private Vector2 _originalPosition;

	public NSendFeedbackCartoon Cartoon { get; private set; }

	public State MyState { get; private set; }

	public override void _Ready()
	{
		_originalPosition = base.Position;
		Cartoon = GetNode<NSendFeedbackCartoon>("Flower");
	}

	public void SetState(State state)
	{
		switch (state)
		{
		case State.Nodding:
			_tween?.Kill();
			_tween = CreateTween();
			_tween.TweenProperty(this, "rotation", Mathf.DegToRad(8f), 0.5);
			_tween.TweenProperty(this, "rotation", Mathf.DegToRad(-8f), 0.5);
			_tween.SetLoops();
			Cartoon.Texture = PreloadManager.Cache.GetTexture2D("res://images/atlases/compressed.sprites/feedback/flower_happy.tres");
			break;
		case State.NoddingFast:
			_tween?.Kill();
			_tween = CreateTween();
			_tween.TweenProperty(this, "rotation", Mathf.DegToRad(8f), 0.2);
			_tween.TweenProperty(this, "rotation", Mathf.DegToRad(-8f), 0.2);
			_tween.SetLoops();
			Cartoon.Texture = PreloadManager.Cache.GetTexture2D("res://images/atlases/compressed.sprites/feedback/flower_happy.tres");
			break;
		case State.Anticipation:
			_tween?.Kill();
			_tween = null;
			Cartoon.Texture = PreloadManager.Cache.GetTexture2D("res://images/atlases/compressed.sprites/feedback/flower_anticipation.tres");
			_tween = CreateTween();
			_tween.TweenInterval(0.05000000074505806);
			_tween.TweenCallback(Callable.From(SetRandomPosition));
			_tween.SetLoops();
			break;
		default:
			base.Rotation = 0f;
			_tween?.Kill();
			_tween = null;
			Cartoon.Texture = PreloadManager.Cache.GetTexture2D("res://images/atlases/compressed.sprites/feedback/flower.tres");
			break;
		}
		MyState = state;
	}

	private void SetRandomPosition()
	{
		base.Position = _originalPosition + new Vector2(Rng.Chaotic.NextFloat(-3f, 3f), Rng.Chaotic.NextFloat(-3f, 3f));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "state", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetRandomPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetState && args.Count == 1)
		{
			SetState(VariantUtils.ConvertTo<State>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetRandomPosition && args.Count == 0)
		{
			SetRandomPosition();
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
		if (method == MethodName.SetState)
		{
			return true;
		}
		if (method == MethodName.SetRandomPosition)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Cartoon)
		{
			Cartoon = VariantUtils.ConvertTo<NSendFeedbackCartoon>(in value);
			return true;
		}
		if (name == PropertyName.MyState)
		{
			MyState = VariantUtils.ConvertTo<State>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._originalPosition)
		{
			_originalPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Cartoon)
		{
			value = VariantUtils.CreateFrom<NSendFeedbackCartoon>(Cartoon);
			return true;
		}
		if (name == PropertyName.MyState)
		{
			value = VariantUtils.CreateFrom<State>(MyState);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._originalPosition)
		{
			value = VariantUtils.CreateFrom(in _originalPosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Cartoon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.MyState, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Cartoon, Variant.From<NSendFeedbackCartoon>(Cartoon));
		info.AddProperty(PropertyName.MyState, Variant.From<State>(MyState));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._originalPosition, Variant.From(in _originalPosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Cartoon, out var value))
		{
			Cartoon = value.As<NSendFeedbackCartoon>();
		}
		if (info.TryGetProperty(PropertyName.MyState, out var value2))
		{
			MyState = value2.As<State>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._originalPosition, out var value4))
		{
			_originalPosition = value4.As<Vector2>();
		}
	}
}
