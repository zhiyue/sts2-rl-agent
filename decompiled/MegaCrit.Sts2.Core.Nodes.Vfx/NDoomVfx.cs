using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NDoomVfx.cs")]
public class NDoomVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ShowOrHideParticles = "ShowOrHideParticles";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _tween = "_tween";

		public static readonly StringName _back = "_back";

		public static readonly StringName _front = "_front";

		public static readonly StringName _creatureVisuals = "_creatureVisuals";

		public static readonly StringName _position = "_position";

		public static readonly StringName _size = "_size";

		public static readonly StringName _shouldDie = "_shouldDie";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private Tween? _tween;

	private NDoomSubEmitterVfx _back;

	private NDoomSubEmitterVfx _front;

	private NCreatureVisuals _creatureVisuals;

	private Vector2 _position;

	private Vector2 _size;

	private bool _shouldDie;

	private CancellationToken _cancelToken;

	private const float _doomVfxSize = 260f;

	private CancellationTokenSource VfxCancellationToken { get; } = new CancellationTokenSource();

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_doom");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public Task? VfxTask { get; private set; }

	public static NDoomVfx? Create(NCreatureVisuals creatureVisuals, Vector2 position, Vector2 size, bool shouldDie)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NDoomVfx nDoomVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NDoomVfx>(PackedScene.GenEditState.Disabled);
		nDoomVfx._creatureVisuals = creatureVisuals;
		nDoomVfx._position = position;
		nDoomVfx._size = size;
		nDoomVfx._shouldDie = shouldDie;
		return nDoomVfx;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		VfxCancellationToken.Cancel();
		_tween?.Kill();
	}

	public override void _Ready()
	{
		_back = GetNode<NDoomSubEmitterVfx>("DoomVfxBack");
		_front = GetNode<NDoomSubEmitterVfx>("DoomVfxFront");
		_cancelToken = VfxCancellationToken.Token;
		VfxTask = TaskHelper.RunSafely(PlayVfx(_creatureVisuals, _position, _size, _shouldDie));
	}

	private async Task PlayVfx(NCreatureVisuals creatureVisuals, Vector2 position, Vector2 size, bool shouldDie)
	{
		if (!_cancelToken.IsCancellationRequested)
		{
			SfxCmd.Play("event:/sfx/characters/necrobinder/necrobinder_doom_kill");
			base.GlobalPosition = position + new Vector2(size.X * 0.5f, size.Y) * NCombatRoom.Instance.SceneContainer.Scale;
			base.Scale = NCombatRoom.Instance.SceneContainer.Scale;
			SubViewport node = GetNode<SubViewport>("Viewport");
			Vector2 vector = size;
			vector.X *= 1.5f;
			vector.Y *= 1.5f;
			node.Size = (Vector2I)vector;
			if (shouldDie)
			{
				Vector2 creatureOffset = new Vector2(vector.X / 2f, node.Size.Y) + creatureVisuals.Body.Position;
				Vector2 originalGlobalScale = creatureVisuals.Body.GlobalScale;
				await Reparent(creatureVisuals.Body, node);
				creatureVisuals.Body.Position = creatureOffset;
				creatureVisuals.Body.Scale = originalGlobalScale;
			}
			if (!_cancelToken.IsCancellationRequested)
			{
				await PlayVfxInternal();
			}
		}
	}

	private async Task PlayVfxInternal()
	{
		_ = 1;
		try
		{
			SubViewport node = GetNode<SubViewport>("Viewport");
			Sprite2D node2 = GetNode<Sprite2D>("%Visual");
			node2.Position += Vector2.Up * node.Size.Y * 0.5f;
			NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short, 180f + Rng.Chaotic.NextFloat(-10f, 10f));
			ShowOrHideParticles((float)node.Size.X / 260f, 0.5f);
			_tween = CreateTween();
			_tween.TweenProperty(node2, "position:y", node2.Position.Y + (float)node.Size.Y, 0.75).SetEase(Tween.EaseType.In).SetDelay(0.75)
				.SetTrans(Tween.TransitionType.Expo);
			await ToSignal(_tween, Tween.SignalName.Finished);
			ShowOrHideParticles(0f, 0.25f);
			await Task.Delay(2000, _cancelToken);
		}
		finally
		{
			if (GodotObject.IsInstanceValid(this))
			{
				this.QueueFreeSafely();
			}
		}
	}

	private void ShowOrHideParticles(float widthScale, float tweenTime)
	{
		_back.ShowOrHide(widthScale, tweenTime);
		_front.ShowOrHide(widthScale, tweenTime);
	}

	private async Task Reparent(Node creatureNode, Node newParent)
	{
		Node parent = creatureNode.GetParent();
		bool removeCompleted = false;
		Callable reparent = Callable.From(() => removeCompleted = true);
		creatureNode.Connect(Node.SignalName.TreeExited, reparent);
		parent.RemoveChildSafely(creatureNode);
		while (!removeCompleted)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		newParent.AddChildSafely(creatureNode);
		creatureNode.Disconnect(Node.SignalName.TreeExited, reparent);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "creatureVisuals", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "shouldDie", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowOrHideParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "widthScale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "tweenTime", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<NDoomVfx>(Create(VariantUtils.ConvertTo<NCreatureVisuals>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<Vector2>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3])));
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowOrHideParticles && args.Count == 2)
		{
			ShowOrHideParticles(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<NDoomVfx>(Create(VariantUtils.ConvertTo<NCreatureVisuals>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<Vector2>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3])));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ShowOrHideParticles)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._back)
		{
			_back = VariantUtils.ConvertTo<NDoomSubEmitterVfx>(in value);
			return true;
		}
		if (name == PropertyName._front)
		{
			_front = VariantUtils.ConvertTo<NDoomSubEmitterVfx>(in value);
			return true;
		}
		if (name == PropertyName._creatureVisuals)
		{
			_creatureVisuals = VariantUtils.ConvertTo<NCreatureVisuals>(in value);
			return true;
		}
		if (name == PropertyName._position)
		{
			_position = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._size)
		{
			_size = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._shouldDie)
		{
			_shouldDie = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._back)
		{
			value = VariantUtils.CreateFrom(in _back);
			return true;
		}
		if (name == PropertyName._front)
		{
			value = VariantUtils.CreateFrom(in _front);
			return true;
		}
		if (name == PropertyName._creatureVisuals)
		{
			value = VariantUtils.CreateFrom(in _creatureVisuals);
			return true;
		}
		if (name == PropertyName._position)
		{
			value = VariantUtils.CreateFrom(in _position);
			return true;
		}
		if (name == PropertyName._size)
		{
			value = VariantUtils.CreateFrom(in _size);
			return true;
		}
		if (name == PropertyName._shouldDie)
		{
			value = VariantUtils.CreateFrom(in _shouldDie);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._back, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._front, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._creatureVisuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._position, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._size, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._shouldDie, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._back, Variant.From(in _back));
		info.AddProperty(PropertyName._front, Variant.From(in _front));
		info.AddProperty(PropertyName._creatureVisuals, Variant.From(in _creatureVisuals));
		info.AddProperty(PropertyName._position, Variant.From(in _position));
		info.AddProperty(PropertyName._size, Variant.From(in _size));
		info.AddProperty(PropertyName._shouldDie, Variant.From(in _shouldDie));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._back, out var value2))
		{
			_back = value2.As<NDoomSubEmitterVfx>();
		}
		if (info.TryGetProperty(PropertyName._front, out var value3))
		{
			_front = value3.As<NDoomSubEmitterVfx>();
		}
		if (info.TryGetProperty(PropertyName._creatureVisuals, out var value4))
		{
			_creatureVisuals = value4.As<NCreatureVisuals>();
		}
		if (info.TryGetProperty(PropertyName._position, out var value5))
		{
			_position = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._size, out var value6))
		{
			_size = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._shouldDie, out var value7))
		{
			_shouldDie = value7.As<bool>();
		}
	}
}
