using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

[ScriptPath("res://src/Core/Nodes/Screens/TreasureRoomRelic/NHandImage.cs")]
public class NHandImage : Control
{
	private enum State
	{
		None,
		Frozen,
		GrabbingRelic
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetIsInFight = "SetIsInFight";

		public static readonly StringName SetFrozenForRelicAwards = "SetFrozenForRelicAwards";

		public static readonly StringName DoFightMove = "DoFightMove";

		public static readonly StringName SetTextureToFightMove = "SetTextureToFightMove";

		public static readonly StringName SetPointingPosition = "SetPointingPosition";

		public static readonly StringName AnimateAway = "AnimateAway";

		public static readonly StringName AnimateIn = "AnimateIn";

		public static readonly StringName SetIsDown = "SetIsDown";

		public static readonly StringName SetAnimateInProgress = "SetAnimateInProgress";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Index = "Index";

		public static readonly StringName IsDown = "IsDown";

		public static readonly StringName _grabMarker = "_grabMarker";

		public static readonly StringName _textureRect = "_textureRect";

		public static readonly StringName _currentVelocity = "_currentVelocity";

		public static readonly StringName _desiredPosition = "_desiredPosition";

		public static readonly StringName _downTween = "_downTween";

		public static readonly StringName _state = "_state";

		public static readonly StringName _isInFight = "_isInFight";

		public static readonly StringName _originalPosition = "_originalPosition";

		public static readonly StringName _handAnimateInProgress = "_handAnimateInProgress";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/hand_image");

	private static readonly Vector2 _pointingPivot = new Vector2(163f, 10f);

	private static readonly Vector2 _fightingPivot = new Vector2(197f, 600f);

	private Marker2D _grabMarker;

	private TextureRect _textureRect;

	private Vector2 _currentVelocity;

	private Vector2 _desiredPosition;

	private Tween? _downTween;

	private State _state;

	private bool _isInFight;

	private Vector2 _originalPosition;

	private float _handAnimateInProgress;

	public Player Player { get; private set; }

	public int Index { get; private set; }

	public bool IsDown { get; private set; }

	public static NHandImage Create(Player player, int slotIndex)
	{
		NHandImage nHandImage = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NHandImage>(PackedScene.GenEditState.Disabled);
		nHandImage.Player = player;
		nHandImage.Index = slotIndex;
		return nHandImage;
	}

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		_grabMarker = GetNode<Marker2D>("GrabMarker");
		_originalPosition = _textureRect.Position;
		base.Rotation = (Index % 4) switch
		{
			0 => 0f, 
			1 => (float)Math.PI / 2f, 
			2 => -(float)Math.PI / 2f, 
			3 => (float)Math.PI, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (!LocalContext.IsMe(Player))
		{
			base.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		}
		_textureRect.Texture = Player.Character.ArmPointingTexture;
	}

	public void SetIsInFight(bool inFight)
	{
		_isInFight = inFight;
		if (_isInFight)
		{
			_textureRect.PivotOffset = _fightingPivot;
			base.Modulate = Colors.White;
			base.ZIndex = 1;
			return;
		}
		_textureRect.PivotOffset = _pointingPivot;
		if (!LocalContext.IsMe(Player))
		{
			base.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		}
		base.ZIndex = 0;
	}

	public void SetFrozenForRelicAwards(bool frozenForRelicAwards)
	{
		if (frozenForRelicAwards)
		{
			_state = State.Frozen;
			Rect2 viewportRect = GetViewportRect();
			Vector2 vector = Vector2.Down.Rotated(base.Rotation);
			_desiredPosition = viewportRect.Size / 2f + viewportRect.Size * vector * 0.1667f;
		}
		else
		{
			_state = State.None;
		}
	}

	public Tween DoFightMove(RelicPickingFightMove move, float duration)
	{
		float num = 0.666f * duration / 3f;
		float num2 = 0.333f * duration / 3f;
		int num3 = 6;
		List<float> list = new List<float>(num3);
		CollectionsMarshal.SetCount(list, num3);
		Span<float> span = CollectionsMarshal.AsSpan(list);
		int num4 = 0;
		span[num4] = num;
		num4++;
		span[num4] = num2;
		num4++;
		span[num4] = num;
		num4++;
		span[num4] = num2;
		num4++;
		span[num4] = num;
		num4++;
		span[num4] = num2;
		List<float> list2 = list;
		for (int i = 0; i < list2.Count - 1; i++)
		{
			float num5 = Rng.Chaotic.NextFloat((0f - duration) / 25f, duration / 25f);
			list2[i] += num5;
			list2[i + 1] -= num5;
		}
		SetTextureToFightMove(RelicPickingFightMove.Rock);
		Tween tween = CreateTween();
		tween.Chain().TweenProperty(_textureRect, "rotation", -(float)Math.PI / 10f + Rng.Chaotic.NextFloat(-0.05f, 0.05f), list2[0]).SetTrans(Tween.TransitionType.Linear)
			.SetEase(Tween.EaseType.In);
		tween.Chain().TweenProperty(_textureRect, "rotation", Rng.Chaotic.NextFloat(-0.02f, 0.02f), list2[1]).SetTrans(Tween.TransitionType.Expo)
			.SetEase(Tween.EaseType.In);
		tween.Chain().TweenProperty(_textureRect, "rotation", -(float)Math.PI / 10f + Rng.Chaotic.NextFloat(-0.05f, 0.05f), list2[2]).SetTrans(Tween.TransitionType.Linear)
			.SetEase(Tween.EaseType.In);
		tween.Chain().TweenProperty(_textureRect, "rotation", Rng.Chaotic.NextFloat(-0.02f, 0.02f), list2[3]).SetTrans(Tween.TransitionType.Expo)
			.SetEase(Tween.EaseType.In);
		tween.Chain().TweenProperty(_textureRect, "rotation", -(float)Math.PI / 10f + Rng.Chaotic.NextFloat(-0.05f, 0.05f), list2[4]).SetTrans(Tween.TransitionType.Linear)
			.SetEase(Tween.EaseType.In);
		tween.Chain().TweenProperty(_textureRect, "rotation", Rng.Chaotic.NextFloat(-0.02f, 0.02f), list2[5]).SetTrans(Tween.TransitionType.Expo)
			.SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(delegate
		{
			SetTextureToFightMove(move);
		}));
		return tween;
	}

	private void SetTextureToFightMove(RelicPickingFightMove move)
	{
		TextureRect textureRect = _textureRect;
		textureRect.Texture = move switch
		{
			RelicPickingFightMove.Rock => Player.Character.ArmRockTexture, 
			RelicPickingFightMove.Paper => Player.Character.ArmPaperTexture, 
			RelicPickingFightMove.Scissors => Player.Character.ArmScissorsTexture, 
			_ => throw new ArgumentOutOfRangeException("move", move, null), 
		};
	}

	public void SetPointingPosition(Vector2 position)
	{
		if (_state == State.None)
		{
			_desiredPosition = position;
		}
	}

	public void AnimateAway()
	{
		Rect2 viewportRect = GetViewportRect();
		Vector2 vector = Vector2.Down.Rotated(base.Rotation);
		_desiredPosition = viewportRect.Size / 2f + viewportRect.Size * vector * 0.8f;
	}

	public void AnimateIn()
	{
		Tween tween = CreateTween();
		_handAnimateInProgress = 0f;
		tween.TweenMethod(Callable.From(delegate(float v)
		{
			_handAnimateInProgress = v;
		}), 0f, 1f, 0.6000000238418579).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
	}

	public void SetIsDown(bool isDown)
	{
		if (IsDown != isDown)
		{
			IsDown = isDown;
			_downTween?.Kill();
			if (isDown)
			{
				_textureRect.Scale = Vector2.One * 0.98f;
				return;
			}
			_downTween = CreateTween();
			_downTween.TweenProperty(_textureRect, "scale", Vector2.One, 0.20000000298023224).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
		}
	}

	public async Task DoLoseShake(float duration)
	{
		CreateTween().TweenProperty(this, "modulate", new Color(0.5f, 0.5f, 0.5f, 0.5f), duration * 0.333f).SetDelay(duration * 0.667f);
		ScreenRumbleInstance rumble = new ScreenRumbleInstance(100f, duration, 5f, RumbleStyle.Rumble);
		while (!rumble.IsDone)
		{
			Vector2 vector = rumble.Update(GetProcessDeltaTime());
			_textureRect.Position = _originalPosition + vector;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		_textureRect.Position = _originalPosition;
	}

	public async Task GrabRelic(NTreasureRoomRelicHolder holder)
	{
		State oldState = _state;
		_state = State.GrabbingRelic;
		SetTextureToFightMove(RelicPickingFightMove.Paper);
		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", holder.GlobalPosition - _grabMarker.Position.Rotated(base.Rotation) + holder.Size * 0.5f, 0.5).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		await ToSignal(tween, Tween.SignalName.Finished);
		SetTextureToFightMove(RelicPickingFightMove.Rock);
		holder.Reparent(this);
		holder.Rotation = 0f - base.Rotation;
		holder.Position = _grabMarker.Position - holder.Size * 0.5f;
		tween = CreateTween();
		tween.TweenProperty(this, "global_position", _desiredPosition, 0.5).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		await ToSignal(tween, Tween.SignalName.Finished);
		_state = oldState;
	}

	public void SetAnimateInProgress(float animateInProgress)
	{
		_handAnimateInProgress = animateInProgress;
	}

	public override void _Process(double delta)
	{
		Vector2 size = GetViewportRect().Size;
		int num = Index % 4;
		if (_state != State.GrabbingRelic)
		{
			Vector2 vector;
			if ((num == 0 || num == 3) ? true : false)
			{
				float num2 = ((num == 0) ? 1 : (-1));
				vector = num2 * Vector2.Down;
			}
			else
			{
				float num3 = ((num == 1) ? 1 : (-1));
				vector = num3 * Vector2.Left;
			}
			Rect2 viewportRect = GetViewportRect();
			Vector2 vector2 = viewportRect.Size / 2f + viewportRect.Size * vector;
			float smoothTime = ((_state == State.Frozen) ? 0.25f : ((!LocalContext.IsMe(Player)) ? 0.07f : 0.01f));
			Vector2 target = vector2.Lerp(_desiredPosition, _handAnimateInProgress);
			base.GlobalPosition = MathHelper.SmoothDamp(base.GlobalPosition, target, ref _currentVelocity, smoothTime, (float)delta);
		}
		if (_state == State.None)
		{
			if ((num == 0 || num == 3) ? true : false)
			{
				float num4 = ((num == 0) ? 1 : (-1));
				_textureRect.Rotation = num4 * (base.GlobalPosition.X - size.X / 2f) / 2000f;
			}
			else
			{
				float num5 = ((num == 1) ? 1 : (-1));
				_textureRect.Rotation = num5 * (base.GlobalPosition.Y - size.Y / 2f) / 1000f;
			}
		}
		else
		{
			_textureRect.Rotation = 0f;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetIsInFight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "inFight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetFrozenForRelicAwards, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "frozenForRelicAwards", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DoFightMove, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Tween"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "move", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTextureToFightMove, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "move", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetPointingPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimateAway, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetIsDown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isDown", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetAnimateInProgress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "animateInProgress", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.SetIsInFight && args.Count == 1)
		{
			SetIsInFight(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetFrozenForRelicAwards && args.Count == 1)
		{
			SetFrozenForRelicAwards(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoFightMove && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<Tween>(DoFightMove(VariantUtils.ConvertTo<RelicPickingFightMove>(in args[0]), VariantUtils.ConvertTo<float>(in args[1])));
			return true;
		}
		if (method == MethodName.SetTextureToFightMove && args.Count == 1)
		{
			SetTextureToFightMove(VariantUtils.ConvertTo<RelicPickingFightMove>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPointingPosition && args.Count == 1)
		{
			SetPointingPosition(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateAway && args.Count == 0)
		{
			AnimateAway();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateIn && args.Count == 0)
		{
			AnimateIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetIsDown && args.Count == 1)
		{
			SetIsDown(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAnimateInProgress && args.Count == 1)
		{
			SetAnimateInProgress(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.SetIsInFight)
		{
			return true;
		}
		if (method == MethodName.SetFrozenForRelicAwards)
		{
			return true;
		}
		if (method == MethodName.DoFightMove)
		{
			return true;
		}
		if (method == MethodName.SetTextureToFightMove)
		{
			return true;
		}
		if (method == MethodName.SetPointingPosition)
		{
			return true;
		}
		if (method == MethodName.AnimateAway)
		{
			return true;
		}
		if (method == MethodName.AnimateIn)
		{
			return true;
		}
		if (method == MethodName.SetIsDown)
		{
			return true;
		}
		if (method == MethodName.SetAnimateInProgress)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Index)
		{
			Index = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.IsDown)
		{
			IsDown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._grabMarker)
		{
			_grabMarker = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName._textureRect)
		{
			_textureRect = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._currentVelocity)
		{
			_currentVelocity = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._desiredPosition)
		{
			_desiredPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._downTween)
		{
			_downTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._state)
		{
			_state = VariantUtils.ConvertTo<State>(in value);
			return true;
		}
		if (name == PropertyName._isInFight)
		{
			_isInFight = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._originalPosition)
		{
			_originalPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._handAnimateInProgress)
		{
			_handAnimateInProgress = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Index)
		{
			value = VariantUtils.CreateFrom<int>(Index);
			return true;
		}
		if (name == PropertyName.IsDown)
		{
			value = VariantUtils.CreateFrom<bool>(IsDown);
			return true;
		}
		if (name == PropertyName._grabMarker)
		{
			value = VariantUtils.CreateFrom(in _grabMarker);
			return true;
		}
		if (name == PropertyName._textureRect)
		{
			value = VariantUtils.CreateFrom(in _textureRect);
			return true;
		}
		if (name == PropertyName._currentVelocity)
		{
			value = VariantUtils.CreateFrom(in _currentVelocity);
			return true;
		}
		if (name == PropertyName._desiredPosition)
		{
			value = VariantUtils.CreateFrom(in _desiredPosition);
			return true;
		}
		if (name == PropertyName._downTween)
		{
			value = VariantUtils.CreateFrom(in _downTween);
			return true;
		}
		if (name == PropertyName._state)
		{
			value = VariantUtils.CreateFrom(in _state);
			return true;
		}
		if (name == PropertyName._isInFight)
		{
			value = VariantUtils.CreateFrom(in _isInFight);
			return true;
		}
		if (name == PropertyName._originalPosition)
		{
			value = VariantUtils.CreateFrom(in _originalPosition);
			return true;
		}
		if (name == PropertyName._handAnimateInProgress)
		{
			value = VariantUtils.CreateFrom(in _handAnimateInProgress);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._grabMarker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textureRect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._currentVelocity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._desiredPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._downTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._state, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isInFight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._handAnimateInProgress, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Index, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Index, Variant.From<int>(Index));
		info.AddProperty(PropertyName.IsDown, Variant.From<bool>(IsDown));
		info.AddProperty(PropertyName._grabMarker, Variant.From(in _grabMarker));
		info.AddProperty(PropertyName._textureRect, Variant.From(in _textureRect));
		info.AddProperty(PropertyName._currentVelocity, Variant.From(in _currentVelocity));
		info.AddProperty(PropertyName._desiredPosition, Variant.From(in _desiredPosition));
		info.AddProperty(PropertyName._downTween, Variant.From(in _downTween));
		info.AddProperty(PropertyName._state, Variant.From(in _state));
		info.AddProperty(PropertyName._isInFight, Variant.From(in _isInFight));
		info.AddProperty(PropertyName._originalPosition, Variant.From(in _originalPosition));
		info.AddProperty(PropertyName._handAnimateInProgress, Variant.From(in _handAnimateInProgress));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Index, out var value))
		{
			Index = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName.IsDown, out var value2))
		{
			IsDown = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._grabMarker, out var value3))
		{
			_grabMarker = value3.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName._textureRect, out var value4))
		{
			_textureRect = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._currentVelocity, out var value5))
		{
			_currentVelocity = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._desiredPosition, out var value6))
		{
			_desiredPosition = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._downTween, out var value7))
		{
			_downTween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._state, out var value8))
		{
			_state = value8.As<State>();
		}
		if (info.TryGetProperty(PropertyName._isInFight, out var value9))
		{
			_isInFight = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._originalPosition, out var value10))
		{
			_originalPosition = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._handAnimateInProgress, out var value11))
		{
			_handAnimateInProgress = value11.As<float>();
		}
	}
}
