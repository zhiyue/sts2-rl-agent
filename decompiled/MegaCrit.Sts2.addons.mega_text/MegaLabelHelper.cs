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

	public static void AssertThemeFontOverride(Control control, StringName fontOverrideName)
	{
		if (control.HasThemeFontOverride(fontOverrideName))
		{
			return;
		}
		throw new InvalidOperationException($"{control.GetType().Name} '{control.GetPath()}' has no theme font override. Please set one to avoid a Godot engine bug.");
	}

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
