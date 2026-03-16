using System;
using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Animation;

public class AnimState
{
	private struct Branch
	{
		public AnimState state;

		public Func<bool>? condition;
	}

	public const string attackAnim = "attack";

	public const string castAnim = "cast";

	public const string dieAnim = "die";

	public const string hurtAnim = "hurt";

	public const string idleAnim = "idle_loop";

	public const string reviveAnim = "revive";

	public const string stunAnim = "stun";

	private readonly Dictionary<string, List<Branch>> _branchedStates;

	public string Id { get; }

	public bool IsLooping { get; }

	public bool HasLooped { get; private set; }

	public AnimState? NextState { get; set; }

	public string? BoundsContainer { get; init; }

	public AnimState(string id, bool isLooping = false)
	{
		Id = id;
		IsLooping = isLooping;
		_branchedStates = new Dictionary<string, List<Branch>>();
	}

	public void AddBranch(string trigger, AnimState state, Func<bool>? condition = null)
	{
		Branch item = new Branch
		{
			state = state,
			condition = condition
		};
		if (!_branchedStates.TryGetValue(trigger, out List<Branch> value))
		{
			value = new List<Branch>();
			_branchedStates[trigger] = value;
		}
		value.Add(item);
	}

	public AnimState? CallTrigger(string trigger)
	{
		if (_branchedStates.TryGetValue(trigger, out List<Branch> value))
		{
			foreach (Branch item in value)
			{
				Func<bool>? condition = item.condition;
				if (condition == null || condition())
				{
					return item.state;
				}
			}
		}
		return null;
	}

	public bool HasTrigger(string trigger)
	{
		return _branchedStates.ContainsKey(trigger);
	}

	public void MarkHasLooped()
	{
		HasLooped = true;
	}
}
