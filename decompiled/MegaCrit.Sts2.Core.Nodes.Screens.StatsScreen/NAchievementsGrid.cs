using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;

[ScriptPath("res://src/Core/Nodes/Screens/StatsScreen/NAchievementsGrid.cs")]
public class NAchievementsGrid : Control
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
		/// Cached name for the 'OnAchievementsChanged' method.
		/// </summary>
		public static readonly StringName OnAchievementsChanged = "OnAchievementsChanged";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ScrollLimitBottom' property.
		/// </summary>
		public static readonly StringName ScrollLimitBottom = "ScrollLimitBottom";

		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_achievementsContainer' field.
		/// </summary>
		public static readonly StringName _achievementsContainer = "_achievementsContainer";

		/// <summary>
		/// Cached name for the '_scrollbarPressed' field.
		/// </summary>
		public static readonly StringName _scrollbarPressed = "_scrollbarPressed";

		/// <summary>
		/// Cached name for the '_startDragPos' field.
		/// </summary>
		public static readonly StringName _startDragPos = "_startDragPos";

		/// <summary>
		/// Cached name for the '_targetDragPos' field.
		/// </summary>
		public static readonly StringName _targetDragPos = "_targetDragPos";

		/// <summary>
		/// Cached name for the '_isDragging' field.
		/// </summary>
		public static readonly StringName _isDragging = "_isDragging";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private Control _achievementsContainer;

	private bool _scrollbarPressed;

	private Vector2 _startDragPos;

	private Vector2 _targetDragPos;

	private bool _isDragging;

	public static IEnumerable<string> AssetPaths => from a in Enum.GetValues<Achievement>()
		select NAchievementHolder.GetPathForAchievement(a);

	private float ScrollLimitBottom => 0f - _achievementsContainer.Size.Y + base.Size.Y;

	public Control DefaultFocusedControl => _achievementsContainer.GetChildren().OfType<NAchievementHolder>().First();

	public override void _Ready()
	{
		_achievementsContainer = GetNode<Control>("%AchievementsContainer");
		List<NAchievementHolder> list = new List<NAchievementHolder>();
		Achievement[] values = Enum.GetValues<Achievement>();
		foreach (Achievement achievement in values)
		{
			NAchievementHolder nAchievementHolder = NAchievementHolder.Create(achievement);
			if (nAchievementHolder.IsUnlocked)
			{
				_achievementsContainer.AddChildSafely(nAchievementHolder);
			}
			else
			{
				list.Add(nAchievementHolder);
			}
		}
		foreach (NAchievementHolder item in list)
		{
			_achievementsContainer.AddChildSafely(item);
		}
	}

	private void OnAchievementsChanged()
	{
		foreach (NAchievementHolder item in _achievementsContainer.GetChildren().OfType<NAchievementHolder>())
		{
			item.RefreshUnlocked();
		}
	}

	public override void _EnterTree()
	{
		AchievementsUtil.AchievementsChanged += OnAchievementsChanged;
	}

	public override void _ExitTree()
	{
		AchievementsUtil.AchievementsChanged -= OnAchievementsChanged;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAchievementsChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAchievementsChanged && args.Count == 0)
		{
			OnAchievementsChanged();
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
		if (method == MethodName.OnAchievementsChanged)
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
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._achievementsContainer)
		{
			_achievementsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._scrollbarPressed)
		{
			_scrollbarPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._startDragPos)
		{
			_startDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			_targetDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			_isDragging = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ScrollLimitBottom)
		{
			value = VariantUtils.CreateFrom<float>(ScrollLimitBottom);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._achievementsContainer)
		{
			value = VariantUtils.CreateFrom(in _achievementsContainer);
			return true;
		}
		if (name == PropertyName._scrollbarPressed)
		{
			value = VariantUtils.CreateFrom(in _scrollbarPressed);
			return true;
		}
		if (name == PropertyName._startDragPos)
		{
			value = VariantUtils.CreateFrom(in _startDragPos);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			value = VariantUtils.CreateFrom(in _targetDragPos);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			value = VariantUtils.CreateFrom(in _isDragging);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._achievementsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._scrollbarPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDragging, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.ScrollLimitBottom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._achievementsContainer, Variant.From(in _achievementsContainer));
		info.AddProperty(PropertyName._scrollbarPressed, Variant.From(in _scrollbarPressed));
		info.AddProperty(PropertyName._startDragPos, Variant.From(in _startDragPos));
		info.AddProperty(PropertyName._targetDragPos, Variant.From(in _targetDragPos));
		info.AddProperty(PropertyName._isDragging, Variant.From(in _isDragging));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._achievementsContainer, out var value))
		{
			_achievementsContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scrollbarPressed, out var value2))
		{
			_scrollbarPressed = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._startDragPos, out var value3))
		{
			_startDragPos = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetDragPos, out var value4))
		{
			_targetDragPos = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isDragging, out var value5))
		{
			_isDragging = value5.As<bool>();
		}
	}
}
