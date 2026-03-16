using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Entities.RestSite;

public sealed class MendRestSiteOption : RestSiteOption
{
	private const string _hasTargetKey = "HasTarget";

	private const string _playerNameKey = "Name";

	private readonly HealVar _healVar = new HealVar(0m);

	private LocString? _description;

	public override string OptionId => "MEND";

	public override LocString Description
	{
		get
		{
			if (_description == null)
			{
				_description = base.Description;
				_description.Add("HasTarget", variable: false);
				_description.Add("Name", "");
				_description.Add(_healVar);
			}
			return _description;
		}
	}

	public static decimal GetHealAmount(Player player)
	{
		return Hook.ModifyRestSiteHealAmount(player.RunState, player.Creature, HealRestSiteOption.GetBaseHealAmount(player.Creature));
	}

	public MendRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override async Task<bool> OnSelect()
	{
		uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(base.Owner);
		Player target = null;
		if (LocalContext.IsMe(base.Owner))
		{
			NRestSiteRoom.Instance.AnimateDescriptionDown();
			NRestSiteButton buttonForOption = NRestSiteRoom.Instance.GetButtonForOption(this);
			Vector2 startPosition = buttonForOption.GlobalPosition + buttonForOption.Size / 2f;
			bool usingController = NControllerManager.Instance.IsUsingController;
			NTargetManager targetManager = NTargetManager.Instance;
			targetManager.StartTargeting(TargetType.AnyPlayer, startPosition, usingController ? TargetMode.Controller : TargetMode.ClickMouseToTarget, ShouldCancelTargeting, AllowHoveringNode);
			if (usingController)
			{
				List<NRestSiteCharacter> list = NRestSiteRoom.Instance.characterAnims.Where((NRestSiteCharacter c) => c.Player != base.Owner).ToList();
				for (int num = 0; num < list.Count; num++)
				{
					list[num].Hitbox.SetFocusMode(Control.FocusModeEnum.All);
					list[num].Hitbox.FocusNeighborTop = list[num].Hitbox.GetPath();
					list[num].Hitbox.FocusNeighborBottom = list[num].Hitbox.GetPath();
					Control hitbox = list[num].Hitbox;
					NodePath path;
					if (num <= 0)
					{
						path = list[list.Count - 1].Hitbox.GetPath();
					}
					else
					{
						path = list[num - 1].Hitbox.GetPath();
					}
					hitbox.FocusNeighborLeft = path;
					list[num].Hitbox.FocusNeighborRight = ((num < list.Count - 1) ? list[num + 1].Hitbox.GetPath() : list[0].Hitbox.GetPath());
				}
				list.FirstOrDefault()?.Hitbox.TryGrabFocus();
			}
			targetManager.Connect(NTargetManager.SignalName.NodeHovered, Callable.From<Node>(OnNodeHovered));
			targetManager.Connect(NTargetManager.SignalName.NodeUnhovered, Callable.From<Node>(OnNodeUnhovered));
			try
			{
				target = NodeToPlayer(await targetManager.SelectionFinished());
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(base.Owner, choiceId, PlayerChoiceResult.FromPlayerId(target?.NetId));
			}
			finally
			{
				targetManager.Disconnect(NTargetManager.SignalName.NodeHovered, Callable.From<Node>(OnNodeHovered));
				targetManager.Disconnect(NTargetManager.SignalName.NodeUnhovered, Callable.From<Node>(OnNodeUnhovered));
				if (usingController)
				{
					foreach (NRestSiteCharacter item in NRestSiteRoom.Instance?.characterAnims ?? new List<NRestSiteCharacter>())
					{
						item.Hitbox.SetFocusMode(Control.FocusModeEnum.None);
					}
				}
			}
		}
		else
		{
			ulong? num2 = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(base.Owner, choiceId)).AsPlayerId();
			if (num2.HasValue)
			{
				target = base.Owner.RunState.GetPlayer(num2.Value);
			}
		}
		NRestSiteRoom.Instance?.AnimateDescriptionUp();
		Description.Add("HasTarget", variable: false);
		NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
		if (target != null)
		{
			await CreatureCmd.Heal(target.Creature, GetHealAmount(target));
			await Hook.AfterRestSiteHeal(target.RunState, target, isMimicked: false);
			if (TestMode.IsOff)
			{
				string scenePath = SceneHelper.GetScenePath("vfx/vfx_cross_heal");
				Node2D node2D = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
				(NRestSiteRoom.Instance?.GetCharacterForPlayer(target))?.AddChildSafely(node2D);
				node2D.Position = Vector2.Zero;
			}
			return true;
		}
		return false;
	}

	private void OnNodeHovered(Node node)
	{
		Player player = NodeToPlayer(node);
		if (player != null)
		{
			Description.Add("HasTarget", variable: true);
			Description.Add("Name", PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId));
			_healVar.BaseValue = HealRestSiteOption.GetBaseHealAmount(player.Creature);
			_healVar.PreviewValue = GetHealAmount(player);
			NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
		}
	}

	private void OnNodeUnhovered(Node _)
	{
		Description.Add("HasTarget", variable: false);
		NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
	}

	private Player? NodeToPlayer(Node? node)
	{
		if (node == null)
		{
			return null;
		}
		if (!(node is NMultiplayerPlayerState nMultiplayerPlayerState))
		{
			if (node is NRestSiteCharacter nRestSiteCharacter)
			{
				return nRestSiteCharacter.Player;
			}
			return null;
		}
		return nMultiplayerPlayerState.Player;
	}

	private bool ShouldCancelTargeting()
	{
		if (NOverlayStack.Instance.ScreenCount <= 0)
		{
			return NCapstoneContainer.Instance.InUse;
		}
		return true;
	}

	private bool AllowHoveringNode(Node node)
	{
		return !LocalContext.IsMe(NodeToPlayer(node));
	}
}
