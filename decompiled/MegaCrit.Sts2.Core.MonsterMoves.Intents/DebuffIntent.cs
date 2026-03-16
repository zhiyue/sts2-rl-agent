namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class DebuffIntent : AbstractIntent
{
	private readonly bool _strong;

	public override IntentType IntentType
	{
		get
		{
			if (!_strong)
			{
				return IntentType.Debuff;
			}
			return IntentType.DebuffStrong;
		}
	}

	protected override string IntentPrefix => "DEBUFF";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_debuff.tres";

	public DebuffIntent(bool strong = false)
	{
		_strong = strong;
	}
}
