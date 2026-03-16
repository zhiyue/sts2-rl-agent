namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class SummonIntent : AbstractIntent
{
	public override IntentType IntentType => IntentType.Summon;

	protected override string IntentPrefix => "SUMMON";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_summon.tres";
}
