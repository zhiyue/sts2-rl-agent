namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class SleepIntent : AbstractIntent
{
	public override IntentType IntentType => IntentType.Sleep;

	protected override string IntentPrefix => "SLEEP";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_sleep.tres";
}
