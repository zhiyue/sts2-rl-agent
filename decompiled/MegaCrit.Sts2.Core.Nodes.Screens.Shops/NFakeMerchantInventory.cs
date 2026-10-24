using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

/// <summary>
/// Manages the shop items for the fake merchant shop. We need this because we need to override the default controller navigation
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Shops/NFakeMerchantInventory.cs")]
public class NFakeMerchantInventory : NMerchantInventory
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NMerchantInventory.MethodName
	{
		/// <summary>
		/// Cached name for the 'UpdateNavigation' method.
		/// </summary>
		public new static readonly StringName UpdateNavigation = "UpdateNavigation";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NMerchantInventory.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NMerchantInventory.SignalName
	{
	}

	protected override void UpdateNavigation()
	{
		List<NMerchantSlot> list = _relicContainer?.GetChildren().OfType<NMerchantSlot>().ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> list2 = new List<NMerchantSlot>(new global::_003C_003Ez__ReadOnlyArray<NMerchantSlot>(new NMerchantSlot[2]
		{
			list[0],
			list[1]
		})).Where((NMerchantSlot r) => r.Entry.IsStocked).ToList();
		List<NMerchantSlot> list3 = new List<NMerchantSlot>(new global::_003C_003Ez__ReadOnlyArray<NMerchantSlot>(new NMerchantSlot[3]
		{
			list[2],
			list[3],
			list[4]
		})).Where((NMerchantSlot r) => r.Entry.IsStocked).ToList();
		List<NMerchantSlot> list4 = new List<NMerchantSlot>(new global::_003C_003Ez__ReadOnlySingleElementList<NMerchantSlot>(list[5])).Where((NMerchantSlot r) => r.Entry.IsStocked).ToList();
		List<List<NMerchantSlot>> list5 = new List<List<NMerchantSlot>>(new global::_003C_003Ez__ReadOnlyArray<List<NMerchantSlot>>(new List<NMerchantSlot>[3] { list2, list3, list4 })).Where((List<NMerchantSlot> r) => r.Count > 0).ToList();
		for (int num = 0; num < list5.Count; num++)
		{
			for (int num2 = 0; num2 < list5[num].Count; num2++)
			{
				list5[num][num2].FocusNeighborLeft = ((num2 > 0) ? list5[num][num2 - 1].GetPath() : list5[num][num2].GetPath());
				list5[num][num2].FocusNeighborRight = ((num2 < list5[num].Count - 1) ? list5[num][num2 + 1].GetPath() : list5[num][num2].GetPath());
				if (num > 0)
				{
					list5[num][num2].FocusNeighborTop = ((num2 < list5[num - 1].Count) ? list5[num - 1][num2].GetPath() : list5[num - 1][list5[num - 1].Count - 1].GetPath());
				}
				else
				{
					list5[num][num2].FocusNeighborTop = list5[num][num2].GetPath();
				}
				if (num < list5.Count - 1)
				{
					list5[num][num2].FocusNeighborBottom = ((num2 < list5[num + 1].Count) ? list5[num + 1][num2].GetPath() : list5[num + 1][list5[num + 1].Count - 1].GetPath());
				}
				else
				{
					list5[num][num2].FocusNeighborBottom = list5[num][num2].GetPath();
				}
			}
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.UpdateNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.UpdateNavigation && args.Count == 0)
		{
			UpdateNavigation();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.UpdateNavigation)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
