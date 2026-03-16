using Godot;

namespace MegaCrit.Sts2.Core.ControllerInput;

public static class MegaInput
{
	public static readonly StringName up = "ui_up";

	public static readonly StringName down = "ui_down";

	public static readonly StringName left = "ui_left";

	public static readonly StringName right = "ui_right";

	public static readonly StringName accept = "ui_accept";

	public static readonly StringName select = "ui_select";

	public static readonly StringName cancel = "ui_cancel";

	public static readonly StringName selectCard1 = "mega_select_card_1";

	public static readonly StringName selectCard2 = "mega_select_card_2";

	public static readonly StringName selectCard3 = "mega_select_card_3";

	public static readonly StringName selectCard4 = "mega_select_card_4";

	public static readonly StringName selectCard5 = "mega_select_card_5";

	public static readonly StringName selectCard6 = "mega_select_card_6";

	public static readonly StringName selectCard7 = "mega_select_card_7";

	public static readonly StringName selectCard8 = "mega_select_card_8";

	public static readonly StringName selectCard9 = "mega_select_card_9";

	public static readonly StringName selectCard10 = "mega_select_card_10";

	public static readonly StringName releaseCard = "mega_release_card";

	public static readonly StringName topPanel = "mega_top_panel";

	public static readonly StringName viewDrawPile = "mega_view_draw_pile";

	public static readonly StringName viewDiscardPile = "mega_view_discard_pile";

	public static readonly StringName viewDeckAndTabLeft = "mega_view_deck_and_tab_left";

	public static readonly StringName viewExhaustPileAndTabRight = "mega_view_exhaust_pile_and_tab_right";

	public static readonly StringName viewMap = "mega_view_map";

	public static readonly StringName pauseAndBack = "mega_pause_and_back";

	public static readonly StringName back = "mega_back";

	public static readonly StringName peek = "mega_peek";

	public static string[] AllInputs => new string[15]
	{
		accept, cancel, down, left, pauseAndBack, peek, right, select, topPanel, up,
		viewDeckAndTabLeft, viewDiscardPile, viewDrawPile, viewExhaustPileAndTabRight, viewMap
	};
}
