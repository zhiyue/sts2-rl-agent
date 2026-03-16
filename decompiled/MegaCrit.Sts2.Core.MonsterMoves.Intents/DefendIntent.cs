namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class DefendIntent : AbstractIntent
{
	public override IntentType IntentType => IntentType.Defend;

	protected override string IntentPrefix => "DEFEND";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_defend.tres";
}
