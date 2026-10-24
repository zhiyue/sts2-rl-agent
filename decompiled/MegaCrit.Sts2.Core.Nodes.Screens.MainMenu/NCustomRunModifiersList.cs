using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NCustomRunModifiersList.cs")]
public class NCustomRunModifiersList : Control
{
	[Signal]
	public delegate void ModifiersChangedEventHandler();

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
		/// Cached name for the 'Initialize' method.
		/// </summary>
		public static readonly StringName Initialize = "Initialize";

		/// <summary>
		/// Cached name for the 'UntickMutuallyExclusiveModifiersForTickbox' method.
		/// </summary>
		public static readonly StringName UntickMutuallyExclusiveModifiersForTickbox = "UntickMutuallyExclusiveModifiersForTickbox";

		/// <summary>
		/// Cached name for the 'AfterModifiersChanged' method.
		/// </summary>
		public static readonly StringName AfterModifiersChanged = "AfterModifiersChanged";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_container' field.
		/// </summary>
		public static readonly StringName _container = "_container";

		/// <summary>
		/// Cached name for the '_mode' field.
		/// </summary>
		public static readonly StringName _mode = "_mode";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
		/// <summary>
		/// Cached name for the 'ModifiersChanged' signal.
		/// </summary>
		public static readonly StringName ModifiersChanged = "ModifiersChanged";
	}

	private readonly List<NRunModifierTickbox> _modifierTickboxes = new List<NRunModifierTickbox>();

	private Control _container;

	private MultiplayerUiMode _mode;

	private ModifiersChangedEventHandler backing_ModifiersChanged;

	public Control? DefaultFocusedControl => _modifierTickboxes.FirstOrDefault();

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NCustomRunModifiersList.ModifiersChangedEventHandler" />
	public event ModifiersChangedEventHandler ModifiersChanged
	{
		add
		{
			backing_ModifiersChanged = (ModifiersChangedEventHandler)Delegate.Combine(backing_ModifiersChanged, value);
		}
		remove
		{
			backing_ModifiersChanged = (ModifiersChangedEventHandler)Delegate.Remove(backing_ModifiersChanged, value);
		}
	}

	public override void _Ready()
	{
		_container = GetNode<Control>("ScrollContainer/Mask/Content");
		foreach (Node child in _container.GetChildren())
		{
			child.QueueFreeSafely();
		}
		foreach (ModifierModel allModifier in GetAllModifiers())
		{
			NRunModifierTickbox nRunModifierTickbox = NRunModifierTickbox.Create(allModifier);
			_container.AddChildSafely(nRunModifierTickbox);
			_modifierTickboxes.Add(nRunModifierTickbox);
			nRunModifierTickbox.Connect(NTickbox.SignalName.Toggled, Callable.From<NRunModifierTickbox>(AfterModifiersChanged));
		}
	}

	/// <summary>
	/// Call this once when the menu opens with the appropriate mode for the screen we're displayed on.
	/// </summary>
	/// <param name="mode">See the documentation for the enumeration for the correct value to pass.</param>
	public void Initialize(MultiplayerUiMode mode)
	{
		_mode = mode;
		if ((uint)(mode - 3) > 1u)
		{
			return;
		}
		foreach (NRunModifierTickbox modifierTickbox in _modifierTickboxes)
		{
			modifierTickbox.Disable();
		}
	}

	/// <summary>
	/// Call this when:
	/// - We are in client mode and the host changes the modifiers that are selected
	/// - Once when the game is loaded from a multiplayer save
	/// </summary>
	/// <param name="modifiers">The modifiers to display as checked.</param>
	public void SyncModifierList(IReadOnlyList<ModifierModel> modifiers)
	{
		MultiplayerUiMode mode = _mode;
		if ((uint)(mode - 1) <= 1u)
		{
			throw new InvalidOperationException("This should only be called in client or load mode!");
		}
		foreach (NRunModifierTickbox tickbox in _modifierTickboxes)
		{
			tickbox.IsTicked = modifiers.FirstOrDefault((ModifierModel m) => m.IsEquivalent(tickbox.Modifier)) != null;
		}
	}

	private IEnumerable<ModifierModel> GetAllModifiers()
	{
		foreach (ModifierModel item in ModelDb.GoodModifiers.Concat(ModelDb.BadModifiers))
		{
			if (item is CharacterCards canonicalCharacterCardsModifier)
			{
				foreach (CharacterModel allCharacter in ModelDb.AllCharacters)
				{
					CharacterCards characterCards = (CharacterCards)canonicalCharacterCardsModifier.ToMutable();
					characterCards.CharacterModel = allCharacter.Id;
					yield return characterCards;
				}
			}
			else
			{
				yield return item.ToMutable();
			}
		}
	}

	private void UntickMutuallyExclusiveModifiersForTickbox(NRunModifierTickbox tickbox)
	{
		if (!tickbox.IsTicked)
		{
			return;
		}
		IReadOnlySet<ModifierModel> readOnlySet = ModelDb.MutuallyExclusiveModifiers.FirstOrDefault((IReadOnlySet<ModifierModel> s) => s.Any((ModifierModel m) => m.GetType() == tickbox.Modifier.GetType()));
		if (readOnlySet == null)
		{
			return;
		}
		foreach (NRunModifierTickbox otherTickbox in _modifierTickboxes)
		{
			if (!(otherTickbox.Modifier.GetType() == tickbox.Modifier.GetType()) && readOnlySet.Any((ModifierModel m) => m.GetType() == otherTickbox.Modifier.GetType()))
			{
				otherTickbox.IsTicked = false;
			}
		}
	}

	private void AfterModifiersChanged(NRunModifierTickbox tickbox)
	{
		UntickMutuallyExclusiveModifiersForTickbox(tickbox);
		EmitSignal(SignalName.ModifiersChanged);
	}

	/// <summary>
	/// Returns the list of modifiers that are ticked on.
	/// </summary>
	public List<ModifierModel> GetModifiersTickedOn()
	{
		return (from t in _modifierTickboxes
			where t.IsTicked
			select t.Modifier).ToList();
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
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "mode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UntickMutuallyExclusiveModifiersForTickbox, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterModifiersChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName.Initialize && args.Count == 1)
		{
			Initialize(VariantUtils.ConvertTo<MultiplayerUiMode>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UntickMutuallyExclusiveModifiersForTickbox && args.Count == 1)
		{
			UntickMutuallyExclusiveModifiersForTickbox(VariantUtils.ConvertTo<NRunModifierTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterModifiersChanged && args.Count == 1)
		{
			AfterModifiersChanged(VariantUtils.ConvertTo<NRunModifierTickbox>(in args[0]));
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
		if (method == MethodName.Initialize)
		{
			return true;
		}
		if (method == MethodName.UntickMutuallyExclusiveModifiersForTickbox)
		{
			return true;
		}
		if (method == MethodName.AfterModifiersChanged)
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
		if (name == PropertyName._mode)
		{
			_mode = VariantUtils.ConvertTo<MultiplayerUiMode>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._container)
		{
			value = VariantUtils.CreateFrom(in _container);
			return true;
		}
		if (name == PropertyName._mode)
		{
			value = VariantUtils.CreateFrom(in _mode);
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
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._mode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._container, Variant.From(in _container));
		info.AddProperty(PropertyName._mode, Variant.From(in _mode));
		info.AddSignalEventDelegate(SignalName.ModifiersChanged, backing_ModifiersChanged);
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
		if (info.TryGetProperty(PropertyName._mode, out var value2))
		{
			_mode = value2.As<MultiplayerUiMode>();
		}
		if (info.TryGetSignalEventDelegate<ModifiersChangedEventHandler>(SignalName.ModifiersChanged, out var value3))
		{
			backing_ModifiersChanged = value3;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.ModifiersChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalModifiersChanged()
	{
		EmitSignal(SignalName.ModifiersChanged);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.ModifiersChanged && args.Count == 0)
		{
			backing_ModifiersChanged?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.ModifiersChanged)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
