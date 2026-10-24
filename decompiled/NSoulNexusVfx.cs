using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NSoulNexusVfx.cs")]
public class NSoulNexusVfx : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnAnimationEvent' method.
		/// </summary>
		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		/// <summary>
		/// Cached name for the 'ShowFire' method.
		/// </summary>
		public static readonly StringName ShowFire = "ShowFire";

		/// <summary>
		/// Cached name for the 'StartPath1' method.
		/// </summary>
		public static readonly StringName StartPath1 = "StartPath1";

		/// <summary>
		/// Cached name for the 'EndPath1' method.
		/// </summary>
		public static readonly StringName EndPath1 = "EndPath1";

		/// <summary>
		/// Cached name for the 'StartPath2' method.
		/// </summary>
		public static readonly StringName StartPath2 = "StartPath2";

		/// <summary>
		/// Cached name for the 'EndPath2' method.
		/// </summary>
		public static readonly StringName EndPath2 = "EndPath2";

		/// <summary>
		/// Cached name for the 'StartPath3' method.
		/// </summary>
		public static readonly StringName StartPath3 = "StartPath3";

		/// <summary>
		/// Cached name for the 'EndPath3' method.
		/// </summary>
		public static readonly StringName EndPath3 = "EndPath3";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_trail1' field.
		/// </summary>
		public static readonly StringName _trail1 = "_trail1";

		/// <summary>
		/// Cached name for the '_trail2' field.
		/// </summary>
		public static readonly StringName _trail2 = "_trail2";

		/// <summary>
		/// Cached name for the '_trail3' field.
		/// </summary>
		public static readonly StringName _trail3 = "_trail3";

		/// <summary>
		/// Cached name for the '_fireTexture' field.
		/// </summary>
		public static readonly StringName _fireTexture = "_fireTexture";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private MegaSprite _megaSprite;

	private NBasicTrail _trail1;

	private NBasicTrail _trail2;

	private NBasicTrail _trail3;

	private TextureRect _fireTexture;

	public override void _Ready()
	{
		_megaSprite = new MegaSprite(GetParent<Node2D>());
		_fireTexture = GetNode<TextureRect>("../HeadFireSlot/FireTexture");
		_trail1 = GetNode<NBasicTrail>("../PathSlot1/Trail");
		_trail2 = GetNode<NBasicTrail>("../PathSlot2/Trail");
		_trail3 = GetNode<NBasicTrail>("../PathSlot3/Trail");
		_trail1.Visible = false;
		_trail2.Visible = false;
		_trail3.Visible = false;
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "hide_fire":
			ShowFire(show: false);
			break;
		case "show_fire":
			ShowFire(show: true);
			break;
		case "path_1_start":
			StartPath1();
			break;
		case "path_1_stop":
			EndPath1();
			break;
		case "path_2_start":
			StartPath2();
			break;
		case "path_2_stop":
			EndPath2();
			break;
		case "path_3_start":
			StartPath3();
			break;
		case "path_3_stop":
			EndPath3();
			break;
		}
	}

	private void ShowFire(bool show)
	{
		_fireTexture.Visible = show;
	}

	private void StartPath1()
	{
		_trail1.Visible = true;
		_trail1.ClearPoints();
	}

	private void EndPath1()
	{
		_trail1.Visible = false;
	}

	private void StartPath2()
	{
		_trail2.Visible = true;
		_trail2.ClearPoints();
	}

	private void EndPath2()
	{
		_trail2.Visible = false;
	}

	private void StartPath3()
	{
		_trail3.Visible = true;
		_trail3.ClearPoints();
	}

	private void EndPath3()
	{
		_trail3.Visible = false;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShowFire, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "show", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartPath1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndPath1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartPath2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndPath2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartPath3, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndPath3, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowFire && args.Count == 1)
		{
			ShowFire(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartPath1 && args.Count == 0)
		{
			StartPath1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndPath1 && args.Count == 0)
		{
			EndPath1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartPath2 && args.Count == 0)
		{
			StartPath2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndPath2 && args.Count == 0)
		{
			EndPath2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartPath3 && args.Count == 0)
		{
			StartPath3();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndPath3 && args.Count == 0)
		{
			EndPath3();
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
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName.ShowFire)
		{
			return true;
		}
		if (method == MethodName.StartPath1)
		{
			return true;
		}
		if (method == MethodName.EndPath1)
		{
			return true;
		}
		if (method == MethodName.StartPath2)
		{
			return true;
		}
		if (method == MethodName.EndPath2)
		{
			return true;
		}
		if (method == MethodName.StartPath3)
		{
			return true;
		}
		if (method == MethodName.EndPath3)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._trail1)
		{
			_trail1 = VariantUtils.ConvertTo<NBasicTrail>(in value);
			return true;
		}
		if (name == PropertyName._trail2)
		{
			_trail2 = VariantUtils.ConvertTo<NBasicTrail>(in value);
			return true;
		}
		if (name == PropertyName._trail3)
		{
			_trail3 = VariantUtils.ConvertTo<NBasicTrail>(in value);
			return true;
		}
		if (name == PropertyName._fireTexture)
		{
			_fireTexture = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._trail1)
		{
			value = VariantUtils.CreateFrom(in _trail1);
			return true;
		}
		if (name == PropertyName._trail2)
		{
			value = VariantUtils.CreateFrom(in _trail2);
			return true;
		}
		if (name == PropertyName._trail3)
		{
			value = VariantUtils.CreateFrom(in _trail3);
			return true;
		}
		if (name == PropertyName._fireTexture)
		{
			value = VariantUtils.CreateFrom(in _fireTexture);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._trail1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._trail2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._trail3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fireTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._trail1, Variant.From(in _trail1));
		info.AddProperty(PropertyName._trail2, Variant.From(in _trail2));
		info.AddProperty(PropertyName._trail3, Variant.From(in _trail3));
		info.AddProperty(PropertyName._fireTexture, Variant.From(in _fireTexture));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._trail1, out var value))
		{
			_trail1 = value.As<NBasicTrail>();
		}
		if (info.TryGetProperty(PropertyName._trail2, out var value2))
		{
			_trail2 = value2.As<NBasicTrail>();
		}
		if (info.TryGetProperty(PropertyName._trail3, out var value3))
		{
			_trail3 = value3.As<NBasicTrail>();
		}
		if (info.TryGetProperty(PropertyName._fireTexture, out var value4))
		{
			_fireTexture = value4.As<TextureRect>();
		}
	}
}
