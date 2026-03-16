namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class EscapeIntent : AbstractIntent
{
	public override IntentType IntentType => IntentType.Escape;

	protected override string IntentPrefix => "ESCAPE";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_escape.tres";
}
