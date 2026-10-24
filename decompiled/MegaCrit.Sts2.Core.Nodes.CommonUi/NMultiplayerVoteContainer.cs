using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NMultiplayerVoteContainer.cs")]
public class NMultiplayerVoteContainer : Control
{
	public delegate bool PlayerVotedDelegate(Player player);

	private record VoteIcon
	{
		public required Player player;

		public required TextureRect node;

		public Tween? tween;

		[CompilerGenerated]
		[SetsRequiredMembers]
		protected VoteIcon(VoteIcon original)
		{
			player = original.player;
			node = original.node;
			tween = original.tween;
		}

		public VoteIcon()
		{
		}
	}

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the 'RefreshPlayerVotes' method.
		/// </summary>
		public static readonly StringName RefreshPlayerVotes = "RefreshPlayerVotes";

		/// <summary>
		/// Cached name for the 'BouncePlayers' method.
		/// </summary>
		public static readonly StringName BouncePlayers = "BouncePlayers";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private const string _voteIconPath = "ui/multiplayer_vote_icon";

	private readonly List<VoteIcon> _votes = new List<VoteIcon>();

	private readonly List<VoteIcon> _iconsAnimatingOut = new List<VoteIcon>();

	private PlayerVotedDelegate _playerVotedDelegate;

	private readonly List<Player> _allPlayers = new List<Player>();

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(SceneHelper.GetScenePath("ui/multiplayer_vote_icon"));

	public IEnumerable<Player> Players => _votes.Select((VoteIcon v) => v.player);

	public void Initialize(PlayerVotedDelegate del, IReadOnlyList<Player> players)
	{
		_playerVotedDelegate = del;
		_allPlayers.AddRange(players);
	}

	/// <summary>
	/// Updates the player votes, animating them in or out based on their presence as reported by _playerVotedDelegate.
	/// </summary>
	/// <param name="animate">Pass false to this to immediately update the container, instead of animating. This is
	/// useful when, for example, the turn begins and we don't want characters popping in or out.</param>
	public void RefreshPlayerVotes(bool animate = true)
	{
		if (_allPlayers.Count == 1)
		{
			return;
		}
		for (int i = 0; i < _votes.Count; i++)
		{
			VoteIcon voteIcon = _votes[i];
			if (!_playerVotedDelegate(voteIcon.player))
			{
				AnimVoteOut(voteIcon, animate);
				_votes.RemoveAt(i);
				i--;
			}
		}
		foreach (Player player in _allPlayers)
		{
			if (_playerVotedDelegate(player))
			{
				int num = _votes.FindIndex((VoteIcon p) => p.player == player);
				if (num < 0)
				{
					VoteIcon voteIcon2 = new VoteIcon
					{
						player = player,
						node = SceneHelper.Instantiate<TextureRect>("ui/multiplayer_vote_icon")
					};
					voteIcon2.node.Texture = player.Character.IconTexture;
					voteIcon2.node.GetNode<TextureRect>("Outline").Texture = player.Character.IconOutlineTexture;
					_votes.Add(voteIcon2);
					this.AddChildSafely(voteIcon2.node);
					voteIcon2.node.PivotOffset = voteIcon2.node.Size * 0.5f;
					AnimVoteIn(voteIcon2, animate);
				}
			}
		}
	}

	/// <summary>
	/// Returns the index at which player is shown in this map vote container.
	/// </summary>
	public int GetVoteIndex(Player player)
	{
		return _votes.FindIndex((VoteIcon v) => v.player == player);
	}

	/// <summary>
	/// Highlights the player within the container.
	/// Used during the map vote randomization animation to show which player is currently being picked.
	/// </summary>
	public void SetPlayerHighlighted(Player player, bool isHighlighted)
	{
		VoteIcon voteIcon = _votes.FirstOrDefault((VoteIcon v) => v.player == player);
		if (voteIcon == null)
		{
			throw new InvalidOperationException();
		}
		voteIcon.tween?.FastForwardToCompletion();
		if (isHighlighted)
		{
			voteIcon.node.Scale = Vector2.One * 1.25f;
		}
		else
		{
			voteIcon.node.Scale = Vector2.One;
		}
	}

	/// <summary>
	/// Bounces all players within the container.
	/// Used during the map vote randomization animation to show which map coordinate has been chosen.
	/// </summary>
	public void BouncePlayers()
	{
		foreach (VoteIcon vote in _votes)
		{
			vote.tween?.FastForwardToCompletion();
			vote.tween = CreateTween().SetParallel();
			vote.tween.TweenProperty(vote.node, "scale", Vector2.One * 1.3f, 0.15).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
			vote.tween.TweenProperty(vote.node, "position:y", -10f, 0.15).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
			vote.tween.Chain().TweenProperty(vote.node, "scale", Vector2.One, 0.2).SetEase(Tween.EaseType.In)
				.SetTrans(Tween.TransitionType.Quad);
			vote.tween.TweenProperty(vote.node, "position:y", 0f, 0.3).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
		}
	}

	private void AnimVoteIn(VoteIcon vote, bool animate)
	{
		int num = _iconsAnimatingOut.FindIndex((VoteIcon i) => i.player == vote.player);
		if (num > 0)
		{
			_iconsAnimatingOut[num].tween?.Kill();
			_iconsAnimatingOut[num].node.QueueFreeSafely();
			_iconsAnimatingOut.RemoveAt(num);
		}
		if (animate)
		{
			vote.tween?.Kill();
			vote.tween = CreateTween().SetParallel();
			vote.tween.TweenProperty(vote.node, "modulate:a", 1f, 0.2).From(0f);
			vote.tween.TweenProperty(vote.node, "position:y", 0f, 0.3).From(20f).SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);
		}
	}

	private void AnimVoteOut(VoteIcon vote, bool animate)
	{
		_iconsAnimatingOut.Add(vote);
		if (animate)
		{
			vote.tween?.Kill();
			vote.tween = CreateTween().SetParallel();
			vote.tween.TweenProperty(vote.node, "modulate:a", 0f, 0.1).SetDelay(0.15000000596046448);
			vote.tween.TweenProperty(vote.node, "position:y", 20f, 0.25).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
			vote.tween.Chain().TweenCallback(Callable.From(delegate
			{
				RemoveVoteAfterAnimation(vote);
			}));
		}
		else
		{
			RemoveVoteAfterAnimation(vote);
		}
	}

	private void RemoveVoteAfterAnimation(VoteIcon vote)
	{
		_iconsAnimatingOut.Remove(vote);
		vote.node.QueueFreeSafely();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.RefreshPlayerVotes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "animate", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.BouncePlayers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.RefreshPlayerVotes && args.Count == 1)
		{
			RefreshPlayerVotes(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BouncePlayers && args.Count == 0)
		{
			BouncePlayers();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.RefreshPlayerVotes)
		{
			return true;
		}
		if (method == MethodName.BouncePlayers)
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
