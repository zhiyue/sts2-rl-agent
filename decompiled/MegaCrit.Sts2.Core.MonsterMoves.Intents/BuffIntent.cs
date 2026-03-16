namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class BuffIntent : AbstractIntent
{
	public override IntentType IntentType => IntentType.Buff;

	protected override string IntentPrefix => "BUFF";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_buff.tres";
}
