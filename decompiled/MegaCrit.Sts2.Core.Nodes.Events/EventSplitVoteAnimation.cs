using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Events;

public class EventSplitVoteAnimation
{
	private NEventLayout _eventLayout;

	private IRunState _runState;

	private int _ticks;

	private Player _winner;

	private readonly List<Player> _sortedPlayers = new List<Player>();

	private Player? _currentlyHighlightedPlayer;

	public EventSplitVoteAnimation(NEventLayout eventLayout, IRunState runState)
	{
		_eventLayout = eventLayout;
		_runState = runState;
	}

	public async Task TryPlay(NEventOptionButton chosenButton)
	{
		if (_eventLayout.OptionButtons.Count((NEventOptionButton b) => b.VoteContainer.Players.Any()) == 1)
		{
			return;
		}
		Rng rng = new Rng((uint)HashCode.Combine(_runState.Rng.Seed, _runState.ActFloor));
		_ticks = rng.NextInt(12, 18);
		float num = rng.NextFloat(0.05f, 0.3f);
		_winner = rng.NextItem(chosenButton.VoteContainer.Players);
		foreach (NEventOptionButton optionButton in _eventLayout.OptionButtons)
		{
			_sortedPlayers.AddRange(optionButton.VoteContainer.Players);
		}
		Tween tween = _eventLayout.CreateTween();
		tween.TweenMethod(Callable.From<float>(TickSplitVoteAnimation), 0f, 1f, 1.2000000476837158).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		tween.TweenInterval(num);
		await _eventLayout.ToSignal(tween, Tween.SignalName.Finished);
		chosenButton.VoteContainer.BouncePlayers();
	}

	private void TickSplitVoteAnimation(float value)
	{
		int num = Mathf.RoundToInt(value * (float)_ticks);
		int num2 = _sortedPlayers.IndexOf(_winner) - (_ticks - num);
		int index = (num2 % _sortedPlayers.Count + _sortedPlayers.Count) % _sortedPlayers.Count;
		Player player = _sortedPlayers[index];
		if (_currentlyHighlightedPlayer != player)
		{
			HighlightPlayer(_sortedPlayers[index]);
			NDebugAudioManager.Instance.Play("map_split_tick.mp3", 0.15f, PitchVariance.Small);
		}
	}

	private void HighlightPlayer(Player? player)
	{
		if (_currentlyHighlightedPlayer == player)
		{
			return;
		}
		if (_currentlyHighlightedPlayer != null)
		{
			foreach (NEventOptionButton optionButton in _eventLayout.OptionButtons)
			{
				if (optionButton.VoteContainer.Players.Contains<Player>(_currentlyHighlightedPlayer))
				{
					optionButton.VoteContainer.SetPlayerHighlighted(_currentlyHighlightedPlayer, isHighlighted: false);
				}
			}
		}
		_currentlyHighlightedPlayer = player;
		if (player == null)
		{
			return;
		}
		foreach (NEventOptionButton optionButton2 in _eventLayout.OptionButtons)
		{
			if (optionButton2.VoteContainer.Players.Contains<Player>(player))
			{
				optionButton2.VoteContainer.SetPlayerHighlighted(player, isHighlighted: true);
			}
		}
	}
}
