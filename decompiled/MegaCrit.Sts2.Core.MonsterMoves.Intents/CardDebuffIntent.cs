namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class CardDebuffIntent : AbstractIntent
{
	public override IntentType IntentType => IntentType.CardDebuff;

	protected override string IntentPrefix => "CARD_DEBUFF";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_card_debuff.tres";
}
