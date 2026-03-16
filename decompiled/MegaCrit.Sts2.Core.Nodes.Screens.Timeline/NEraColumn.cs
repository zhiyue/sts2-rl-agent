using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NEraColumn.cs")]
public class NEraColumn : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SpawnIcon = "SpawnIcon";

		public static readonly StringName SetPredictedPosition = "SetPredictedPosition";

		public static readonly StringName RectChange = "RectChange";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _icon = "_icon";

		public static readonly StringName _name = "_name";

		public static readonly StringName _year = "_year";

		public static readonly StringName _iconTween = "_iconTween";

		public static readonly StringName _labelTween = "_labelTween";

		public static readonly StringName _labelSpawned = "_labelSpawned";

		public static readonly StringName era = "era";

		public static readonly StringName _prevLocalPos = "_prevLocalPos";

		public static readonly StringName _prevGlobalPos = "_prevGlobalPos";

		public static readonly StringName _predictedPosition = "_predictedPosition";

		public static readonly StringName _targetPosition = "_targetPosition";

		public static readonly StringName _isAnimated = "_isAnimated";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "res://scenes/timeline_screen/era_column.tscn";

	public static readonly IEnumerable<string> assetPaths;

	private TextureRect _icon;

	private MegaLabel _name;

	private MegaLabel _year;

	private Tween _iconTween;

	private Tween _labelTween;

	private bool _labelSpawned;

	public EpochEra era;

	private Vector2 _prevLocalPos;

	private Vector2 _prevGlobalPos;

	private Vector2 _predictedPosition;

	private Vector2 _targetPosition;

	private bool _isAnimated;

	private EpochSlotData _data;

	public static NEraColumn Create(EpochSlotData data)
	{
		NEraColumn nEraColumn = PreloadManager.Cache.GetScene("res://scenes/timeline_screen/era_column.tscn").Instantiate<NEraColumn>(PackedScene.GenEditState.Disabled);
		nEraColumn._data = data;
		return nEraColumn;
	}

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("%Icon");
		_name = GetNode<MegaLabel>("%Name");
		_year = GetNode<MegaLabel>("%Year");
		era = _data.Era;
		SetName(_data.Era.ToString());
		Init(_data);
		base.ItemRectChanged += RectChange;
	}

	public void Init(EpochSlotData epochSlot)
	{
		(Texture2D, string) eraIcon = NTimelineScreen.GetEraIcon(epochSlot.Era);
		_icon.Texture = eraIcon.Item1;
		_name.SetTextAutoSize(new LocString("eras", eraIcon.Item2 + ".name").GetFormattedText());
		_year.SetTextAutoSize(new LocString("eras", eraIcon.Item2 + ".year").GetFormattedText());
		AddSlot(epochSlot);
	}

	public void AddSlot(EpochSlotData epochSlotData)
	{
		NEpochSlot nEpochSlot = NEpochSlot.Create(epochSlotData);
		this.AddChildSafely(nEpochSlot);
		nEpochSlot.Name = $"Slot{epochSlotData.EraPosition}";
		MoveChild(nEpochSlot, 0);
	}

	public void SpawnIcon()
	{
		_iconTween = CreateTween().SetParallel();
		_iconTween.TweenProperty(_icon, "modulate:a", 1f, 0.5);
		_iconTween.TweenProperty(_icon, "scale", Vector2.One, 0.5).From(Vector2.One * 0.1f).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Back);
	}

	public async Task SpawnSlots(bool isAnimated)
	{
		foreach (Node child in GetChildren())
		{
			if (child is NEpochSlot { HasSpawned: false } nEpochSlot)
			{
				if (isAnimated)
				{
					await nEpochSlot.SpawnSlot();
				}
				else
				{
					TaskHelper.RunSafely(nEpochSlot.SpawnSlot());
				}
			}
		}
	}

	public async Task SpawnNameAndYear()
	{
		if (!_labelSpawned)
		{
			_labelSpawned = true;
			_labelTween = CreateTween().SetParallel();
			_name.SelfModulate = new Color(_name.SelfModulate.R, _name.SelfModulate.G, _name.SelfModulate.B, 0f);
			_year.Modulate = new Color(_year.Modulate.R, _year.Modulate.G, _year.Modulate.B, 0f);
			_labelTween.TweenProperty(_name, "self_modulate:a", 1f, 1.0);
			_labelTween.TweenProperty(_name, "position:y", 28f, 1.0).From(-36f).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Cubic);
			_labelTween.TweenProperty(_year, "modulate:a", 1f, 1.0).SetDelay(0.5);
			_labelTween.TweenProperty(_year, "position:y", 20f, 1.0).SetDelay(0.5).From(0f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Cubic);
			await ToSignal(_labelTween, Tween.SignalName.Finished);
			await Task.Delay(500);
		}
	}

	public async Task SaveBeforeAnimationPosition()
	{
		_isAnimated = true;
		_prevLocalPos = base.Position;
		_prevGlobalPos = base.GlobalPosition;
		await GetTree().ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_isAnimated = false;
		_targetPosition = _predictedPosition;
		base.GlobalPosition = _prevGlobalPos;
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(this, "position", _targetPosition, 2.0).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
	}

	public void SetPredictedPosition(Vector2 setPredictedPosition)
	{
		if (_isAnimated)
		{
			_predictedPosition = setPredictedPosition;
		}
	}

	private void RectChange()
	{
		if (_isAnimated)
		{
			base.GlobalPosition = _prevGlobalPos;
		}
	}

	public override void _ExitTree()
	{
		base.ItemRectChanged -= RectChange;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SpawnIcon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetPredictedPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "setPredictedPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RectChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SpawnIcon && args.Count == 0)
		{
			SpawnIcon();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPredictedPosition && args.Count == 1)
		{
			SetPredictedPosition(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RectChange && args.Count == 0)
		{
			RectChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.SpawnIcon)
		{
			return true;
		}
		if (method == MethodName.SetPredictedPosition)
		{
			return true;
		}
		if (method == MethodName.RectChange)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._name)
		{
			_name = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._year)
		{
			_year = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._iconTween)
		{
			_iconTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._labelTween)
		{
			_labelTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._labelSpawned)
		{
			_labelSpawned = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.era)
		{
			era = VariantUtils.ConvertTo<EpochEra>(in value);
			return true;
		}
		if (name == PropertyName._prevLocalPos)
		{
			_prevLocalPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._prevGlobalPos)
		{
			_prevGlobalPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._predictedPosition)
		{
			_predictedPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			_targetPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isAnimated)
		{
			_isAnimated = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._name)
		{
			value = VariantUtils.CreateFrom(in _name);
			return true;
		}
		if (name == PropertyName._year)
		{
			value = VariantUtils.CreateFrom(in _year);
			return true;
		}
		if (name == PropertyName._iconTween)
		{
			value = VariantUtils.CreateFrom(in _iconTween);
			return true;
		}
		if (name == PropertyName._labelTween)
		{
			value = VariantUtils.CreateFrom(in _labelTween);
			return true;
		}
		if (name == PropertyName._labelSpawned)
		{
			value = VariantUtils.CreateFrom(in _labelSpawned);
			return true;
		}
		if (name == PropertyName.era)
		{
			value = VariantUtils.CreateFrom(in era);
			return true;
		}
		if (name == PropertyName._prevLocalPos)
		{
			value = VariantUtils.CreateFrom(in _prevLocalPos);
			return true;
		}
		if (name == PropertyName._prevGlobalPos)
		{
			value = VariantUtils.CreateFrom(in _prevGlobalPos);
			return true;
		}
		if (name == PropertyName._predictedPosition)
		{
			value = VariantUtils.CreateFrom(in _predictedPosition);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			value = VariantUtils.CreateFrom(in _targetPosition);
			return true;
		}
		if (name == PropertyName._isAnimated)
		{
			value = VariantUtils.CreateFrom(in _isAnimated);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._name, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._year, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._labelTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._labelSpawned, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.era, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._prevLocalPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._prevGlobalPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._predictedPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isAnimated, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._name, Variant.From(in _name));
		info.AddProperty(PropertyName._year, Variant.From(in _year));
		info.AddProperty(PropertyName._iconTween, Variant.From(in _iconTween));
		info.AddProperty(PropertyName._labelTween, Variant.From(in _labelTween));
		info.AddProperty(PropertyName._labelSpawned, Variant.From(in _labelSpawned));
		info.AddProperty(PropertyName.era, Variant.From(in era));
		info.AddProperty(PropertyName._prevLocalPos, Variant.From(in _prevLocalPos));
		info.AddProperty(PropertyName._prevGlobalPos, Variant.From(in _prevGlobalPos));
		info.AddProperty(PropertyName._predictedPosition, Variant.From(in _predictedPosition));
		info.AddProperty(PropertyName._targetPosition, Variant.From(in _targetPosition));
		info.AddProperty(PropertyName._isAnimated, Variant.From(in _isAnimated));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._icon, out var value))
		{
			_icon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._name, out var value2))
		{
			_name = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._year, out var value3))
		{
			_year = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._iconTween, out var value4))
		{
			_iconTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._labelTween, out var value5))
		{
			_labelTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._labelSpawned, out var value6))
		{
			_labelSpawned = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.era, out var value7))
		{
			era = value7.As<EpochEra>();
		}
		if (info.TryGetProperty(PropertyName._prevLocalPos, out var value8))
		{
			_prevLocalPos = value8.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._prevGlobalPos, out var value9))
		{
			_prevGlobalPos = value9.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._predictedPosition, out var value10))
		{
			_predictedPosition = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetPosition, out var value11))
		{
			_targetPosition = value11.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isAnimated, out var value12))
		{
			_isAnimated = value12.As<bool>();
		}
	}

	static NEraColumn()
	{
		List<string> list = new List<string>();
		list.Add("res://scenes/timeline_screen/era_column.tscn");
		list.AddRange(NEpochSlot.assetPaths);
		assetPaths = new _003C_003Ez__ReadOnlyList<string>(list);
	}
}
