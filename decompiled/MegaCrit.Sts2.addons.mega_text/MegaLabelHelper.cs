using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Text;

namespace MegaCrit.Sts2.addons.mega_text;

public static class MegaLabelHelper
{
	private enum BbcodeParsingState
	{
		NotInTag,
		InTag,
		InEndTag,
		InTagEnvironment
	}

	private const float _defaultLineSpacing = -3f;

	private static readonly List<BbcodeObject> _cachedBbcodeObjects = new List<BbcodeObject>();

	/// <summary>
	/// This is the result of a very long investigation into what I believe is ultimately a Godot bug. Here's an
	/// explanation of the issue for future reference.
	///
	/// MegaLabel uses TextParagraph to auto-size. In order to use this method, MegaLabel must grab the current font
	/// whenever it auto-sizes. Auto-sizing happens in response to many different events, including when the game is
	/// quitting (this is caused by how Godot works, it's not intentional).
	///
	/// When we're auto-sizing in response to quitting and we try to grab the current font, if we have no font override
	/// (meaning the font is just defined in the theme, or nowhere at all), the game beach-balls on macOS. If we do have
	/// a font override (even if it's the exact same font as in the theme), it works perfectly.
	///
	/// This feels to me like either a reference counting bug, or an issue with the resource serialization lifecycle
	/// timing. I believe we can fix it with an engine patch, and once we do, we can remove this check. In the meantime
	/// though, this will keep us from creating a MegaLabel without a font override.
	/// </summary>
	public static void AssertThemeFontOverride(Control control, StringName fontOverrideName)
	{
		if (control.HasThemeFontOverride(fontOverrideName))
		{
			return;
		}
		throw new InvalidOperationException($"{control.GetType().Name} '{control.GetPath()}' has no theme font override. Please set one to avoid a Godot engine bug.");
	}

	/// <summary>
	/// This does a crude parsing of BBcode tags and text into a format usable by the EstimateTextSize function.
	/// The version of EsimateTextSize that takes a string can be used with RichTextLabel.GetParsedText. However, this
	/// will strip images and ignore their size, which is problematic when we are using energy icons in text.
	/// If EstimateTextSize is called multiple times, call this only once to minimize performance impact.
	/// Note that the return value of this is cached! We only expect this to be called once, for the results to be used,
	/// and then for it to be called again.
	/// </summary>
	/// <param name="bbcode">String with bbcode tags.</param>
	/// <returns>The parsed tags and text from the bbcode string.</returns>
	public static List<BbcodeObject> ParseBbcode(string bbcode)
	{
		_cachedBbcodeObjects.Clear();
		BbcodeParsingState bbcodeParsingState = BbcodeParsingState.NotInTag;
		Stack<string> stack = new Stack<string>();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < bbcode.Length; i++)
		{
			if (bbcode[i] == '[' && bbcodeParsingState == BbcodeParsingState.NotInTag)
			{
				if (stringBuilder.Length > 0)
				{
					_cachedBbcodeObjects.Add(new BbcodeObject
					{
						text = stringBuilder.ToString(),
						type = BbcodeObjectType.Text
					});
					stringBuilder.Clear();
				}
				bbcodeParsingState = BbcodeParsingState.InTag;
			}
			else if ((bbcode[i] == ' ' || bbcode[i] == '=') && bbcodeParsingState == BbcodeParsingState.InTag)
			{
				string text = stringBuilder.ToString();
				_cachedBbcodeObjects.Add(new BbcodeObject
				{
					tag = text,
					type = BbcodeObjectType.BeginTag
				});
				stack.Push(text);
				bbcodeParsingState = BbcodeParsingState.InTagEnvironment;
			}
			else if (bbcode[i] == '/' && bbcodeParsingState == BbcodeParsingState.InTag)
			{
				bbcodeParsingState = BbcodeParsingState.InEndTag;
			}
			else if (bbcode[i] == ']' && (bbcodeParsingState == BbcodeParsingState.InTag || bbcodeParsingState == BbcodeParsingState.InEndTag || bbcodeParsingState == BbcodeParsingState.InTagEnvironment))
			{
				if (bbcodeParsingState != BbcodeParsingState.InTagEnvironment)
				{
					string text2 = stringBuilder.ToString();
					if (text2 == "lb")
					{
						_cachedBbcodeObjects.Add(new BbcodeObject
						{
							text = "[",
							type = BbcodeObjectType.Text
						});
					}
					else if (text2 == "rb")
					{
						_cachedBbcodeObjects.Add(new BbcodeObject
						{
							text = "]",
							type = BbcodeObjectType.Text
						});
					}
					else
					{
						if (bbcodeParsingState == BbcodeParsingState.InEndTag)
						{
							if (stack.Count == 0)
							{
								throw new InvalidOperationException($"Found end tag {text2} with no tag on the stack. ({bbcode})");
							}
							if (stack.Peek() != text2)
							{
								throw new InvalidOperationException($"Found end tag {text2}, expected {stack.Peek()}. ({bbcode})");
							}
							stack.Pop();
						}
						else
						{
							stack.Push(text2);
						}
						_cachedBbcodeObjects.Add(new BbcodeObject
						{
							tag = text2,
							type = ((bbcodeParsingState == BbcodeParsingState.InTag) ? BbcodeObjectType.BeginTag : BbcodeObjectType.EndTag)
						});
					}
				}
				bbcodeParsingState = BbcodeParsingState.NotInTag;
				stringBuilder.Clear();
			}
			else if (bbcodeParsingState != BbcodeParsingState.InTagEnvironment)
			{
				stringBuilder.Append(bbcode[i]);
			}
		}
		if (bbcodeParsingState != BbcodeParsingState.NotInTag)
		{
			throw new InvalidOperationException("In tag at end of string");
		}
		if (stringBuilder.Length > 0)
		{
			_cachedBbcodeObjects.Add(new BbcodeObject
			{
				text = stringBuilder.ToString(),
				type = BbcodeObjectType.Text
			});
		}
		return _cachedBbcodeObjects;
	}

	/// <summary>
	/// Estimates the size of some text with bbcode in it.
	/// </summary>
	/// <param name="paragraph">Cached paragraph object that can be reused through multiple calls to this function.</param>
	/// <param name="objs">The parsed bbcode text obtained from ParseBbcode.</param>
	/// <param name="font">The font to use.</param>
	/// <param name="fontSize">The font size to use.</param>
	/// <param name="maxWidth">The maximum width of a line before wrapping.</param>
	/// <param name="lineSpacing">The spacing between lines.</param>
	/// <returns>Estimated size of the text.</returns>
	public static Vector2 EstimateTextSize(TextParagraph paragraph, List<BbcodeObject> objs, Font font, int fontSize, float maxWidth, float lineSpacing)
	{
		paragraph.Clear();
		paragraph.Direction = TextServer.Direction.Auto;
		paragraph.Orientation = TextServer.Orientation.Horizontal;
		Stack<string> stack = new Stack<string>();
		int num = 0;
		foreach (BbcodeObject obj in objs)
		{
			if (obj.type == BbcodeObjectType.BeginTag)
			{
				stack.Push(obj.tag);
			}
			else if (obj.type == BbcodeObjectType.EndTag)
			{
				stack.Pop();
			}
			else if (obj.type == BbcodeObjectType.Text)
			{
				if (stack.TryPeek(out var result) && result == "img")
				{
					string text = obj.text;
					Texture2D texture2D = PreloadManager.Cache.GetTexture2D(text);
					paragraph.AddObject(num, texture2D.GetSize(), InlineAlignment.Center);
					num++;
				}
				else
				{
					paragraph.AddString(obj.text, font, fontSize);
				}
			}
		}
		paragraph.Width = maxWidth;
		paragraph.BreakFlags = TextServer.LineBreakFlag.Mandatory | TextServer.LineBreakFlag.WordBound;
		paragraph.JustificationFlags = TextServer.JustificationFlag.Kashida | TextServer.JustificationFlag.WordBound;
		paragraph.TextOverrunBehavior = TextServer.OverrunBehavior.TrimChar;
		paragraph.Alignment = HorizontalAlignment.Center;
		paragraph.MaxLinesVisible = -1;
		int lineCount = paragraph.GetLineCount();
		return paragraph.GetSize() + Vector2.Down * (lineSpacing - -3f) * (lineCount - 1);
	}

	/// <summary>
	/// Checks if some text with bbcode in it is too big for the passed size.
	/// </summary>
	/// <param name="paragraph">Cached paragraph object that can be reused through multiple calls to this function.</param>
	/// <param name="objs">The parsed bbcode text obtained from ParseBbcode.</param>
	/// <param name="font">The font to use.</param>
	/// <param name="fontSize">The font size to use.</param>
	/// <param name="lineSpacing">The spacing between lines.</param>
	/// <param name="rectSize">The size to check against.</param>
	/// <param name="horizontallyBound">If false, allows the size to be greater than the X size of the rect.</param>
	/// <param name="verticallyBound">If false, allows the size to be greater than the Y size of the rect.</param>
	/// <returns>Whether or not the text would overrun rectSize when rendered.</returns>
	public static bool IsTooBig(TextParagraph paragraph, List<BbcodeObject> objs, Font font, int fontSize, float lineSpacing, Vector2 rectSize, bool horizontallyBound, bool verticallyBound)
	{
		Vector2 vector = EstimateTextSize(paragraph, objs, font, fontSize, rectSize.X, lineSpacing);
		float x = rectSize.X;
		float y = rectSize.Y;
		bool flag = vector.X > x;
		bool flag2 = vector.Y > y;
		if (!(flag && horizontallyBound))
		{
			return flag2 && verticallyBound;
		}
		return true;
	}

	/// <summary>
	/// Estimates the size of some text.
	/// Should be called only on text with no bbcode in it.
	/// </summary>
	/// <param name="paragraph">Cached paragraph object that can be reused through multiple calls to this function.</param>
	/// <param name="text">The text whose size will be estimated.</param>
	/// <param name="font">The font to use.</param>
	/// <param name="fontSize">The font size to use.</param>
	/// <param name="maxWidth">The maximum width of a line before wrapping.</param>
	/// <param name="lineSpacing">The spacing between lines.</param>
	/// <param name="wrap">If true, line wrapping is accounted for.</param>
	/// <returns>Estimated size of the text.</returns>
	public static Vector2 EstimateTextSize(TextParagraph paragraph, string text, Font font, int fontSize, float maxWidth, float lineSpacing, bool wrap = true)
	{
		paragraph.Clear();
		paragraph.Direction = TextServer.Direction.Auto;
		paragraph.Orientation = TextServer.Orientation.Horizontal;
		paragraph.AddString(text, font, fontSize);
		paragraph.Width = maxWidth;
		paragraph.BreakFlags = (TextServer.LineBreakFlag)(wrap ? 3 : 0);
		paragraph.JustificationFlags = TextServer.JustificationFlag.Kashida | TextServer.JustificationFlag.WordBound;
		paragraph.TextOverrunBehavior = TextServer.OverrunBehavior.NoTrimming;
		paragraph.Alignment = HorizontalAlignment.Center;
		paragraph.MaxLinesVisible = -1;
		int lineCount = paragraph.GetLineCount();
		return paragraph.GetSize() + Vector2.Down * (lineSpacing - -3f) * (lineCount - 1);
	}

	/// <summary>
	/// Checks if some text with no bbcode in it is too big for the passed size.
	/// </summary>
	/// <param name="paragraph">Cached paragraph object that can be reused through multiple calls to this function.</param>
	/// <param name="text">The text whose size will be estimated.</param>
	/// <param name="font">The font to use.</param>
	/// <param name="fontSize">The font size to use.</param>
	/// <param name="lineSpacing">The spacing between lines.</param>
	/// <param name="rectSize">The size to check against.</param>
	/// <param name="wrap">Whether or not we allow the text to be wrapped.</param>
	/// <returns>Whether or not the text would overrun rectSize when rendered.</returns>
	public static bool IsTooBig(TextParagraph paragraph, string text, Font font, int fontSize, float lineSpacing, bool wrap, Vector2 rectSize)
	{
		Vector2 vector = EstimateTextSize(paragraph, text, font, fontSize, rectSize.X, lineSpacing, wrap);
		float x = rectSize.X;
		float y = rectSize.Y;
		bool flag = vector.X > x;
		bool flag2 = vector.Y > y;
		return flag || flag2;
	}
}
