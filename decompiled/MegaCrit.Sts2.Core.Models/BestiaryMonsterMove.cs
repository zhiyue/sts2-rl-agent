using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Models;

public class BestiaryMonsterMove
{
	private static readonly LocString _attackMoveName = new LocString("bestiary", "ACTION_NAME.attack");

	private static readonly LocString _castMoveName = new LocString("bestiary", "ACTION_NAME.cast");

	private static readonly LocString _dieMoveName = new LocString("bestiary", "ACTION_NAME.die");

	private static readonly LocString _hurtMoveName = new LocString("bestiary", "ACTION_NAME.hurt");

	private static readonly LocString _reviveMoveName = new LocString("bestiary", "ACTION_NAME.revive");

	private static readonly LocString _stunMoveName = new LocString("bestiary", "ACTION_NAME.stun");

	public string moveName;

	public readonly string animId;

	public readonly string? sfx;

	public float sfxDelay;

	public BestiaryMonsterMove(string moveName, string animId, string sfx = "", float sfxDelay = 0f)
	{
		this.moveName = moveName;
		this.animId = animId;
		this.sfx = sfx;
		this.sfxDelay = sfxDelay;
		InitName();
	}

	public BestiaryMonsterMove(LocString moveName, string animId, string sfx = "", float sfxDelay = 0f)
	{
		this.moveName = moveName.GetRawText();
		this.animId = animId;
		this.sfx = sfx;
		this.sfxDelay = sfxDelay;
	}

	private void InitName()
	{
		string text = animId;
		string text2 = (animId.StartsWith("attack") ? _attackMoveName.GetRawText() : (text switch
		{
			"cast" => _castMoveName.GetRawText(), 
			"die" => _dieMoveName.GetRawText(), 
			"hurt" => _hurtMoveName.GetRawText(), 
			"revive" => _reviveMoveName.GetRawText(), 
			"stun" => _stunMoveName.GetRawText(), 
			_ => "MISSING_CASE", 
		}));
		moveName = text2;
	}
}
