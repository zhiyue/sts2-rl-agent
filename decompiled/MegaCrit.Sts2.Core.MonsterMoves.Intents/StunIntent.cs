using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class StunIntent : AbstractIntent
{
	private const string _intentPrefix = "STUN";

	private const string _spritePath = "atlases/intent_atlas.sprites/intent_stun.tres";

	public override IntentType IntentType => IntentType.Stun;

	protected override string IntentPrefix => "STUN";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_stun.tres";

	public static HoverTip GetStaticHoverTip()
	{
		string imagePath = ImageHelper.GetImagePath("atlases/intent_atlas.sprites/intent_stun.tres");
		Texture2D texture2D = PreloadManager.Cache.GetTexture2D(imagePath);
		LocString description = new LocString("intents", "STUN.description");
		return new HoverTip(new LocString("intents", "STUN.title"), description, texture2D);
	}
}
