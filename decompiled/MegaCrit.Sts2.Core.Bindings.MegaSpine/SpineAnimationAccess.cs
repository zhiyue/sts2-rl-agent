namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

/// <summary>
/// Null-safe wrapper for spine animation operations. No-ops when the underlying MegaSprite is null
/// (e.g. when skeleton data failed to load). This eliminates the need for null-guards at every call site.
/// </summary>
public readonly struct SpineAnimationAccess
{
	private readonly MegaSprite? _sprite;

	public bool IsValid => _sprite != null;

	public SpineAnimationAccess(MegaSprite? sprite)
	{
		_sprite = sprite;
	}

	public MegaTrackEntry? SetAnimation(string name, bool loop = true, int track = 0)
	{
		return _sprite?.GetAnimationState().SetAnimation(name, loop, track);
	}

	public MegaTrackEntry? AddAnimation(string name, float delay = 0f, bool loop = true, int track = 0)
	{
		return _sprite?.GetAnimationState().AddAnimation(name, delay, loop, track);
	}

	public MegaTrackEntry? GetCurrentTrack(int track = 0)
	{
		return _sprite?.GetAnimationState().GetCurrent(track);
	}

	/// <summary>
	/// Name of the animation currently playing on the given track, or null if the sprite is null or no
	/// track is active. See MegaAnimationState.GetCurrentAnimationName (PRG-6985).
	/// </summary>
	public string? GetCurrentAnimationName(int track = 0)
	{
		return _sprite?.GetAnimationState().GetCurrentAnimationName(track);
	}

	/// <summary>
	/// Duration of the animation currently playing on the given track, or null if the sprite is null or
	/// no track is active. See <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.SpineAnimationAccess.GetCurrentAnimationName(System.Int32)" />.
	/// </summary>
	public float? GetCurrentAnimationDuration(int track = 0)
	{
		return _sprite?.GetAnimationState().GetCurrentAnimationDuration(track);
	}

	public void SetTimeScale(float scale)
	{
		_sprite?.GetAnimationState().SetTimeScale(scale);
	}

	public MegaAnimationState? GetAnimationState()
	{
		return _sprite?.GetAnimationState();
	}
}
