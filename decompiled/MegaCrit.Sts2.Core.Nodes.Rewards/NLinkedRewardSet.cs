using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Nodes.Rewards;

[ScriptPath("res://src/Core/Nodes/Rewards/NLinkedRewardSet.cs")]
public class NLinkedRewardSet : Control
{
	[Signal]
	public delegate void RewardClaimedEventHandler(NLinkedRewardSet linkedRewardSet);

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
		/// Cached name for the 'Reload' method.
		/// </summary>
		public static readonly StringName Reload = "Reload";

		/// <summary>
		/// Cached name for the 'GetReward' method.
		/// </summary>
		public static readonly StringName GetReward = "GetReward";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_rewardsScreen' field.
		/// </summary>
		public static readonly StringName _rewardsScreen = "_rewardsScreen";

		/// <summary>
		/// Cached name for the '_rewardContainer' field.
		/// </summary>
		public static readonly StringName _rewardContainer = "_rewardContainer";

		/// <summary>
		/// Cached name for the '_chainsContainer' field.
		/// </summary>
		public static readonly StringName _chainsContainer = "_chainsContainer";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
		/// <summary>
		/// Cached name for the 'RewardClaimed' signal.
		/// </summary>
		public static readonly StringName RewardClaimed = "RewardClaimed";
	}

	private NRewardsScreen _rewardsScreen;

	private Control _rewardContainer;

	private Control _chainsContainer;

	private RewardClaimedEventHandler backing_RewardClaimed;

	public LinkedRewardSet LinkedRewardSet { get; private set; }

	private static string ScenePath => SceneHelper.GetScenePath("/rewards/linked_reward_set");

	private static string ChainImagePath => ImageHelper.GetImagePath("/ui/reward_screen/reward_chain.png");

	public static IEnumerable<string> AssetPaths => new string[2] { ScenePath, ChainImagePath };

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Rewards.NLinkedRewardSet.RewardClaimedEventHandler" />
	public event RewardClaimedEventHandler RewardClaimed
	{
		add
		{
			backing_RewardClaimed = (RewardClaimedEventHandler)Delegate.Combine(backing_RewardClaimed, value);
		}
		remove
		{
			backing_RewardClaimed = (RewardClaimedEventHandler)Delegate.Remove(backing_RewardClaimed, value);
		}
	}

	public override void _Ready()
	{
		_rewardContainer = GetNode<Control>("%RewardContainer");
		_chainsContainer = GetNode<Control>("%ChainContainer");
		Reload();
	}

	public static NLinkedRewardSet Create(LinkedRewardSet linkedReward, NRewardsScreen screen)
	{
		NLinkedRewardSet nLinkedRewardSet = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NLinkedRewardSet>(PackedScene.GenEditState.Disabled);
		nLinkedRewardSet._rewardsScreen = screen;
		nLinkedRewardSet.SetReward(linkedReward);
		return nLinkedRewardSet;
	}

	private void SetReward(LinkedRewardSet linkedReward)
	{
		LinkedRewardSet = linkedReward;
		if (IsNodeReady())
		{
			Reload();
		}
	}

	private void Reload()
	{
		if (!IsNodeReady())
		{
			return;
		}
		for (int i = 0; i < LinkedRewardSet.Rewards.Count; i++)
		{
			Reward reward = LinkedRewardSet.Rewards[i];
			NRewardButton nRewardButton = NRewardButton.Create(reward, _rewardsScreen);
			nRewardButton.CustomMinimumSize -= Vector2.Right * 20f;
			_rewardContainer.AddChildSafely(nRewardButton);
			nRewardButton.Connect(NRewardButton.SignalName.RewardClaimed, Callable.From(GetReward));
			if (i < LinkedRewardSet.Rewards.Count - 1)
			{
				TextureRect textureRect = new TextureRect();
				textureRect.MouseFilter = MouseFilterEnum.Ignore;
				textureRect.Texture = PreloadManager.Cache.GetCompressedTexture2D(ChainImagePath);
				textureRect.Size = Vector2.One * 50f;
				_chainsContainer.AddChildSafely(textureRect);
				textureRect.GlobalPosition = _chainsContainer.GlobalPosition + Vector2.Down * i * (3f + nRewardButton.CustomMinimumSize.Y);
			}
		}
	}

	private void GetReward()
	{
		_rewardsScreen.RewardCollectedFrom(this);
		LinkedRewardSet.OnSkipped();
		EmitSignal(SignalName.RewardClaimed);
		this.QueueFreeSafely();
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
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetReward, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetReward && args.Count == 0)
		{
			GetReward();
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
		if (method == MethodName.Reload)
		{
			return true;
		}
		if (method == MethodName.GetReward)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._rewardsScreen)
		{
			_rewardsScreen = VariantUtils.ConvertTo<NRewardsScreen>(in value);
			return true;
		}
		if (name == PropertyName._rewardContainer)
		{
			_rewardContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._chainsContainer)
		{
			_chainsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._rewardsScreen)
		{
			value = VariantUtils.CreateFrom(in _rewardsScreen);
			return true;
		}
		if (name == PropertyName._rewardContainer)
		{
			value = VariantUtils.CreateFrom(in _rewardContainer);
			return true;
		}
		if (name == PropertyName._chainsContainer)
		{
			value = VariantUtils.CreateFrom(in _chainsContainer);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rewardsScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rewardContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chainsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._rewardsScreen, Variant.From(in _rewardsScreen));
		info.AddProperty(PropertyName._rewardContainer, Variant.From(in _rewardContainer));
		info.AddProperty(PropertyName._chainsContainer, Variant.From(in _chainsContainer));
		info.AddSignalEventDelegate(SignalName.RewardClaimed, backing_RewardClaimed);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._rewardsScreen, out var value))
		{
			_rewardsScreen = value.As<NRewardsScreen>();
		}
		if (info.TryGetProperty(PropertyName._rewardContainer, out var value2))
		{
			_rewardContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._chainsContainer, out var value3))
		{
			_chainsContainer = value3.As<Control>();
		}
		if (info.TryGetSignalEventDelegate<RewardClaimedEventHandler>(SignalName.RewardClaimed, out var value4))
		{
			backing_RewardClaimed = value4;
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
		list.Add(new MethodInfo(SignalName.RewardClaimed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "linkedRewardSet", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalRewardClaimed(NLinkedRewardSet linkedRewardSet)
	{
		EmitSignal(SignalName.RewardClaimed, linkedRewardSet);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.RewardClaimed && args.Count == 1)
		{
			backing_RewardClaimed?.Invoke(VariantUtils.ConvertTo<NLinkedRewardSet>(in args[0]));
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
		if (signal == SignalName.RewardClaimed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
