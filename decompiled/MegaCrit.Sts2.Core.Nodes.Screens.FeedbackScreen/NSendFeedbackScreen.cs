using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;
using Sentry;

namespace MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;

[ScriptPath("res://src/Core/Nodes/Screens/FeedbackScreen/NSendFeedbackScreen.cs")]
public class NSendFeedbackScreen : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Relocalize = "Relocalize";

		public static readonly StringName OnDescriptionChanged = "OnDescriptionChanged";

		public static readonly StringName SetScreenshot = "SetScreenshot";

		public static readonly StringName EmojiButtonSelected = "EmojiButtonSelected";

		public static readonly StringName SendButtonFocused = "SendButtonFocused";

		public static readonly StringName SendButtonUnfocused = "SendButtonUnfocused";

		public static readonly StringName Open = "Open";

		public static readonly StringName Close = "Close";

		public static readonly StringName ClearInput = "ClearInput";

		public static readonly StringName SetSelectedEmoji = "SetSelectedEmoji";

		public static readonly StringName SendButtonSelected = "SendButtonSelected";

		public static readonly StringName WiggleCartoons1 = "WiggleCartoons1";

		public static readonly StringName WiggleCartoons2 = "WiggleCartoons2";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _backButton = "_backButton";

		public static readonly StringName _mainPanel = "_mainPanel";

		public static readonly StringName _descriptionInput = "_descriptionInput";

		public static readonly StringName _emojiLabel = "_emojiLabel";

		public static readonly StringName _sendButton = "_sendButton";

		public static readonly StringName _sendLabel = "_sendLabel";

		public static readonly StringName _categoryDropdown = "_categoryDropdown";

		public static readonly StringName _successBackstop = "_successBackstop";

		public static readonly StringName _successPanel = "_successPanel";

		public static readonly StringName _successLabel = "_successLabel";

		public static readonly StringName _flower = "_flower";

		public static readonly StringName _selectedEmoteButton = "_selectedEmoteButton";

		public static readonly StringName _screenshotBytes = "_screenshotBytes";

		public static readonly StringName _originalSuccessPosition = "_originalSuccessPosition";

		public static readonly StringName _lastClosedMsec = "_lastClosedMsec";

		public static readonly StringName _descriptionText = "_descriptionText";

		public static readonly StringName _descriptionCaretLine = "_descriptionCaretLine";

		public static readonly StringName _descriptionCaretColumn = "_descriptionCaretColumn";

		public static readonly StringName _wiggleTween = "_wiggleTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/feedback_screen/feedback_screen");

	private const float _superWiggleTime = 0.25f;

	private const string _defaultUrl = "https://feedback.sts2.megacrit.com/feedback";

	private static readonly string _url = System.Environment.GetEnvironmentVariable("STS2_FEEDBACK_URL") ?? "https://feedback.sts2.megacrit.com/feedback";

	private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

	private const int _maxDescriptionChars = 500;

	private NBackButton _backButton;

	private Control _mainPanel;

	private NMegaTextEdit _descriptionInput;

	private MegaLabel _emojiLabel;

	private NButton _sendButton;

	private MegaLabel _sendLabel;

	private NFeedbackCategoryDropdown _categoryDropdown;

	private Control _successBackstop;

	private Control _successPanel;

	private MegaLabel _successLabel;

	private List<NSendFeedbackCartoon> _cartoons = new List<NSendFeedbackCartoon>();

	private NSendFeedbackFlower _flower;

	private CancellationTokenSource? _cancelToken;

	private CancellationTokenSource? _sendCancelToken;

	private readonly List<NSendFeedbackEmojiButton> _emojiButtons = new List<NSendFeedbackEmojiButton>();

	private NSendFeedbackEmojiButton? _selectedEmoteButton;

	private byte[]? _screenshotBytes;

	private Vector2 _originalSuccessPosition;

	private ulong _lastClosedMsec;

	private string _descriptionText = string.Empty;

	private int _descriptionCaretLine;

	private int _descriptionCaretColumn;

	private Tween? _wiggleTween;

	public Control DefaultFocusedControl => _descriptionInput;

	public static NSendFeedbackScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NSendFeedbackScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_mainPanel = GetNode<Control>("%MainPanel");
		_descriptionInput = GetNode<NMegaTextEdit>("%DescriptionInput");
		_emojiLabel = GetNode<MegaLabel>("%EmojiLabel");
		_sendButton = GetNode<NButton>("%SendButton");
		_sendLabel = _sendButton.GetNode<MegaLabel>("Label");
		_categoryDropdown = GetNode<NFeedbackCategoryDropdown>("%CategoryDropdown");
		_successBackstop = GetNode<Control>("%SuccessBackstop");
		_successPanel = GetNode<Control>("%SuccessPanel");
		_successLabel = GetNode<MegaLabel>("%SuccessLabel");
		_backButton = GetNode<NBackButton>("BackButton");
		_originalSuccessPosition = _successPanel.Position;
		int num = 3;
		List<NSendFeedbackCartoon> list = new List<NSendFeedbackCartoon>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<NSendFeedbackCartoon> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = GetNode<NSendFeedbackCartoon>("Sun");
		num2++;
		span[num2] = GetNode<NSendFeedbackCartoon>("Cupcake");
		num2++;
		span[num2] = GetNode<NSendFeedbackCartoon>("FlowerContainer/Flower");
		_cartoons = list;
		_flower = GetNode<NSendFeedbackFlower>("FlowerContainer");
		foreach (Node child in GetNode("%EmojiButtonContainer").GetChildren())
		{
			if (child is NSendFeedbackEmojiButton nSendFeedbackEmojiButton)
			{
				_emojiButtons.Add(nSendFeedbackEmojiButton);
				nSendFeedbackEmojiButton.PivotOffset = nSendFeedbackEmojiButton.Size * 0.5f;
				nSendFeedbackEmojiButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(EmojiButtonSelected));
			}
		}
		_sendButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(SendButtonSelected));
		_sendButton.Connect(NClickableControl.SignalName.Focused, Callable.From<NClickableControl>(SendButtonFocused));
		_sendButton.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NClickableControl>(SendButtonUnfocused));
		_descriptionInput.Connect(TextEdit.SignalName.TextChanged, Callable.From(OnDescriptionChanged));
		_sendButton.FocusNeighborTop = _categoryDropdown.GetPath();
		_sendButton.FocusNeighborLeft = _emojiButtons.Last().GetPath();
		_sendButton.FocusNeighborBottom = _sendButton.GetPath();
		_sendButton.FocusNeighborRight = _sendButton.GetPath();
		_emojiButtons.Last().FocusNeighborRight = _sendButton.GetPath();
		foreach (NSendFeedbackEmojiButton emojiButton in _emojiButtons)
		{
			emojiButton.FocusNeighborTop = _categoryDropdown.GetPath();
			emojiButton.FocusNeighborBottom = emojiButton.GetPath();
		}
		_categoryDropdown.FocusNeighborRight = _sendButton.GetPath();
		_categoryDropdown.FocusNeighborBottom = _emojiButtons.First().GetPath();
		_categoryDropdown.FocusNeighborTop = _descriptionInput.GetPath();
		_descriptionInput.FocusNeighborTop = _descriptionInput.GetPath();
		_descriptionInput.FocusNeighborLeft = _descriptionInput.GetPath();
		_descriptionInput.FocusNeighborRight = _descriptionInput.GetPath();
		_descriptionInput.FocusNeighborBottom = _categoryDropdown.GetPath();
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			Close();
		}));
		base.Visible = false;
		base.MouseFilter = MouseFilterEnum.Ignore;
		_backButton.Disable();
	}

	public void Relocalize()
	{
		_descriptionInput.PlaceholderText = new LocString("settings_ui", "FEEDBACK_DESCRIPTION_PLACEHOLDER").GetFormattedText();
		_emojiLabel.SetTextAutoSize(new LocString("settings_ui", "FEEDBACK_EMOJI_LABEL").GetFormattedText());
		_sendLabel.SetTextAutoSize(new LocString("settings_ui", "FEEDBACK_SEND_BUTTON_LABEL").GetFormattedText());
		_descriptionInput.RefreshFont();
		_emojiLabel.RefreshFont();
		_sendLabel.RefreshFont();
	}

	private void OnDescriptionChanged()
	{
		if (_descriptionInput.Text.Length > 500)
		{
			_descriptionInput.Text = _descriptionText;
			_descriptionInput.SetCaretLine(_descriptionCaretLine);
			_descriptionInput.SetCaretColumn(_descriptionCaretColumn);
		}
		else
		{
			_descriptionText = _descriptionInput.Text;
			_descriptionCaretLine = _descriptionInput.GetCaretLine();
			_descriptionCaretColumn = _descriptionInput.GetCaretColumn();
		}
	}

	public void SetScreenshot(Image screenshot)
	{
		int width = screenshot.GetWidth();
		int height = screenshot.GetHeight();
		float num = (float)width / (float)height;
		if (width > 1280)
		{
			screenshot.Resize(1280, Mathf.RoundToInt(1280f / num), Image.Interpolation.Bilinear);
		}
		if (height > 720)
		{
			screenshot.Resize(Mathf.RoundToInt(720f * num), 720, Image.Interpolation.Bilinear);
		}
		_screenshotBytes = screenshot.SavePngToBuffer();
	}

	private void EmojiButtonSelected(NButton button)
	{
		SetSelectedEmoji((NSendFeedbackEmojiButton)button);
	}

	private void SendButtonFocused(NClickableControl _)
	{
		_flower.SetState(NSendFeedbackFlower.State.Anticipation);
	}

	private void SendButtonUnfocused(NClickableControl _)
	{
		if (_flower.MyState == NSendFeedbackFlower.State.Anticipation)
		{
			_flower.SetState(NSendFeedbackFlower.State.None);
		}
	}

	public void Open()
	{
		Log.Info("Feedback screen opened");
		if (Time.GetTicksMsec() - _lastClosedMsec > 60000)
		{
			ClearInput();
		}
		base.Visible = true;
		_flower.SetState(NSendFeedbackFlower.State.None);
		_successBackstop.Visible = false;
		base.MouseFilter = MouseFilterEnum.Stop;
		NHotkeyManager.Instance.AddBlockingScreen(this);
		ActiveScreenContext.Instance.Update();
		_backButton.Enable();
	}

	private void Close()
	{
		Log.Info("Feedback screen closed");
		_flower.SetState(NSendFeedbackFlower.State.None);
		_successBackstop.Visible = false;
		_mainPanel.Modulate = Colors.White;
		_wiggleTween?.Kill();
		base.Visible = false;
		_lastClosedMsec = Time.GetTicksMsec();
		_cancelToken?.Cancel();
		base.MouseFilter = MouseFilterEnum.Ignore;
		_backButton.Disable();
		NHotkeyManager.Instance.RemoveBlockingScreen(this);
		ActiveScreenContext.Instance.Update();
	}

	private void ClearInput()
	{
		_descriptionInput.Text = string.Empty;
		_descriptionText = string.Empty;
		SetSelectedEmoji(null);
	}

	private void SetSelectedEmoji(NSendFeedbackEmojiButton? button)
	{
		NSendFeedbackEmojiButton selectedEmoteButton = _selectedEmoteButton;
		_selectedEmoteButton?.SetSelected(isSelected: false);
		if (selectedEmoteButton != button)
		{
			_selectedEmoteButton = button;
			_selectedEmoteButton?.SetSelected(isSelected: true);
		}
	}

	private void SendButtonSelected(NButton _)
	{
		Log.Info("Beginning asynchronous feedback send at " + Log.Timestamp + ": " + _descriptionText);
		ReleaseInfo releaseInfo = ReleaseInfoManager.Instance.ReleaseInfo;
		string text = releaseInfo?.Commit ?? GitHelper.ShortCommitId;
		FeedbackData data = new FeedbackData
		{
			description = _descriptionText,
			category = _categoryDropdown.CurrentCategory,
			gameVersion = (releaseInfo?.Version ?? GitHelper.ShortCommitId ?? "unknown"),
			uniqueId = SaveManager.Instance.Progress.UniqueId,
			commit = (text ?? "unknown"),
			platformBranch = PlatformUtil.GetPlatformBranch()
		};
		byte[] screenshotBytes = _screenshotBytes;
		int currentProfileId = SaveManager.Instance.CurrentProfileId;
		_sendCancelToken?.Cancel();
		_sendCancelToken?.Dispose();
		_sendCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(SendFeedback(data, screenshotBytes, currentProfileId, _sendCancelToken.Token));
		ClearInput();
		_screenshotBytes = null;
		TaskHelper.RunSafely(OnFeedbackSuccess());
	}

	private static async Task SendFeedback(FeedbackData data, byte[] screenshotBytes, int profileId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(data.description))
		{
			return;
		}
		using MemoryStream logsMemoryStream = new MemoryStream();
		GetLogsConsoleCmd.ZipFeedbackLogs(logsMemoryStream, profileId);
		byte[] logsZipBytes = logsMemoryStream.ToArray();
		using MultipartFormDataContent formContent = BuildMultipartContent(data, screenshotBytes, logsZipBytes);
		byte[] bodyBytes = await formContent.ReadAsByteArrayAsync(cancellationToken);
		MediaTypeHeaderValue contentType = formContent.Headers.ContentType;
		int[] delaysMs = new int[3] { 1000, 2000, 4000 };
		string sentryMessage = null;
		for (int attempt = 0; attempt <= 3; attempt++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				using ByteArrayContent content = new ByteArrayContent(bodyBytes);
				content.Headers.ContentType = contentType;
				using HttpResponseMessage response = await _httpClient.PutAsync(_url, content, cancellationToken);
				if (response.IsSuccessStatusCode)
				{
					Log.Info("Feedback successfully posted!");
					return;
				}
				int statusCode = (int)response.StatusCode;
				if (statusCode >= 400 && statusCode < 500 && statusCode != 429)
				{
					string value = await response.Content.ReadAsStringAsync(cancellationToken);
					Log.Warn($"Feedback rejected ({response.StatusCode}): {value}");
					SentrySdk.CaptureMessage($"Feedback rejected: Response status code {response.StatusCode}");
					return;
				}
				sentryMessage = $"Response status code {response.StatusCode}";
				Log.Warn($"Feedback attempt {attempt + 1}/{4} failed: {response.StatusCode}");
			}
			catch (HttpRequestException ex)
			{
				string text = $"Feedback attempt {attempt + 1}/{4} network error: {ExceptionMessageWithInner(ex)} {ex.HttpRequestError}";
				if (ex.HttpRequestError != HttpRequestError.NameResolutionError)
				{
					sentryMessage = "HttpRequestException: " + ExceptionMessageWithInner(ex);
				}
				Log.Warn(text);
			}
			if (attempt < 3)
			{
				await Task.Delay(delaysMs[attempt], cancellationToken);
			}
		}
		Log.Warn("Feedback send failed after all retry attempts");
		if (sentryMessage != null)
		{
			SentrySdk.CaptureMessage("Feedback failed to send: " + sentryMessage);
		}
	}

	private static string ExceptionMessageWithInner(Exception ex)
	{
		if (ex.InnerException == null)
		{
			return ex.Message;
		}
		return ex.Message + " | " + ExceptionMessageWithInner(ex.InnerException);
	}

	private static MultipartFormDataContent BuildMultipartContent(FeedbackData data, byte[] screenshotBytes, byte[] logsZipBytes)
	{
		string content = JsonSerializer.Serialize(data, JsonSerializationUtility.GetTypeInfo<FeedbackData>());
		MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
		StringContent stringContent = new StringContent(content);
		stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		stringContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
		{
			Name = "payload_json"
		};
		multipartFormDataContent.Add(stringContent);
		ByteArrayContent byteArrayContent = new ByteArrayContent(screenshotBytes);
		byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
		byteArrayContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
		{
			Name = "screenshot"
		};
		multipartFormDataContent.Add(byteArrayContent);
		ByteArrayContent byteArrayContent2 = new ByteArrayContent(logsZipBytes);
		byteArrayContent2.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
		byteArrayContent2.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
		{
			Name = "logs"
		};
		multipartFormDataContent.Add(byteArrayContent2);
		return multipartFormDataContent;
	}

	private async Task OnFeedbackSuccess()
	{
		_successBackstop.Visible = true;
		_successPanel.Modulate = Colors.Transparent;
		Control successPanel = _successPanel;
		Vector2 position = _successPanel.Position;
		position.Y = _originalSuccessPosition.Y + 20f;
		successPanel.Position = position;
		_successLabel.SetTextAutoSize(new LocString("settings_ui", "FEEDBACK_SEND_SUCCESS_LABEL").GetFormattedText());
		_successLabel.Modulate = StsColors.green;
		Tween tween = GetTree().CreateTween().Parallel();
		tween.TweenProperty(_mainPanel, "modulate", new Color(0.1f, 0.1f, 0.1f), 0.15000000596046448);
		tween.TweenProperty(_successPanel, "modulate", Colors.White, 0.15000000596046448);
		tween.TweenProperty(_successPanel, "position:y", _originalSuccessPosition.Y, 0.15000000596046448).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
		tween.Chain().TweenProperty(_successLabel, "position:y", _successLabel.Position.Y - 10f, 0.10000000149011612).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Quad);
		tween.Chain().TweenProperty(_successLabel, "position:y", _successLabel.Position.Y, 0.10000000149011612).SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Quad);
		_wiggleTween?.Kill();
		_wiggleTween = CreateTween();
		_wiggleTween.TweenCallback(Callable.From(WiggleCartoons1));
		_wiggleTween.TweenInterval(0.25);
		_wiggleTween.TweenCallback(Callable.From(WiggleCartoons2));
		_wiggleTween.TweenInterval(0.25);
		_wiggleTween.SetLoops();
		string scenePath = SceneHelper.GetScenePath("vfx/vfx_dramatic_entrance_fullscreen");
		Node2D node2D = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		this.AddChildSafely(node2D);
		MoveChild(node2D, 1);
		node2D.GlobalPosition = NGame.Instance.GetViewportRect().Size * 0.5f;
		_flower.SetState(NSendFeedbackFlower.State.NoddingFast);
		_cancelToken = new CancellationTokenSource();
		await Task.Delay(2000, _cancelToken.Token);
		Close();
	}

	private void WiggleCartoons1()
	{
		foreach (NSendFeedbackCartoon cartoon in _cartoons)
		{
			if (_flower.MyState == NSendFeedbackFlower.State.None || cartoon != _flower.Cartoon)
			{
				cartoon.SetRotation1();
			}
		}
	}

	private void WiggleCartoons2()
	{
		foreach (NSendFeedbackCartoon cartoon in _cartoons)
		{
			if (_flower.MyState == NSendFeedbackFlower.State.None || cartoon != _flower.Cartoon)
			{
				cartoon.SetRotation2();
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(15);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Relocalize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDescriptionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetScreenshot, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "screenshot", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Image"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EmojiButtonSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SendButtonFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SendButtonUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetSelectedEmoji, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SendButtonSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.WiggleCartoons1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.WiggleCartoons2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NSendFeedbackScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Relocalize && args.Count == 0)
		{
			Relocalize();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDescriptionChanged && args.Count == 0)
		{
			OnDescriptionChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetScreenshot && args.Count == 1)
		{
			SetScreenshot(VariantUtils.ConvertTo<Image>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EmojiButtonSelected && args.Count == 1)
		{
			EmojiButtonSelected(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SendButtonFocused && args.Count == 1)
		{
			SendButtonFocused(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SendButtonUnfocused && args.Count == 1)
		{
			SendButtonUnfocused(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Open && args.Count == 0)
		{
			Open();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Close && args.Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearInput && args.Count == 0)
		{
			ClearInput();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSelectedEmoji && args.Count == 1)
		{
			SetSelectedEmoji(VariantUtils.ConvertTo<NSendFeedbackEmojiButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SendButtonSelected && args.Count == 1)
		{
			SendButtonSelected(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WiggleCartoons1 && args.Count == 0)
		{
			WiggleCartoons1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WiggleCartoons2 && args.Count == 0)
		{
			WiggleCartoons2();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NSendFeedbackScreen>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Relocalize)
		{
			return true;
		}
		if (method == MethodName.OnDescriptionChanged)
		{
			return true;
		}
		if (method == MethodName.SetScreenshot)
		{
			return true;
		}
		if (method == MethodName.EmojiButtonSelected)
		{
			return true;
		}
		if (method == MethodName.SendButtonFocused)
		{
			return true;
		}
		if (method == MethodName.SendButtonUnfocused)
		{
			return true;
		}
		if (method == MethodName.Open)
		{
			return true;
		}
		if (method == MethodName.Close)
		{
			return true;
		}
		if (method == MethodName.ClearInput)
		{
			return true;
		}
		if (method == MethodName.SetSelectedEmoji)
		{
			return true;
		}
		if (method == MethodName.SendButtonSelected)
		{
			return true;
		}
		if (method == MethodName.WiggleCartoons1)
		{
			return true;
		}
		if (method == MethodName.WiggleCartoons2)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._mainPanel)
		{
			_mainPanel = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._descriptionInput)
		{
			_descriptionInput = VariantUtils.ConvertTo<NMegaTextEdit>(in value);
			return true;
		}
		if (name == PropertyName._emojiLabel)
		{
			_emojiLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._sendButton)
		{
			_sendButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._sendLabel)
		{
			_sendLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._categoryDropdown)
		{
			_categoryDropdown = VariantUtils.ConvertTo<NFeedbackCategoryDropdown>(in value);
			return true;
		}
		if (name == PropertyName._successBackstop)
		{
			_successBackstop = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._successPanel)
		{
			_successPanel = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._successLabel)
		{
			_successLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._flower)
		{
			_flower = VariantUtils.ConvertTo<NSendFeedbackFlower>(in value);
			return true;
		}
		if (name == PropertyName._selectedEmoteButton)
		{
			_selectedEmoteButton = VariantUtils.ConvertTo<NSendFeedbackEmojiButton>(in value);
			return true;
		}
		if (name == PropertyName._screenshotBytes)
		{
			_screenshotBytes = VariantUtils.ConvertTo<byte[]>(in value);
			return true;
		}
		if (name == PropertyName._originalSuccessPosition)
		{
			_originalSuccessPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._lastClosedMsec)
		{
			_lastClosedMsec = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._descriptionText)
		{
			_descriptionText = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._descriptionCaretLine)
		{
			_descriptionCaretLine = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._descriptionCaretColumn)
		{
			_descriptionCaretColumn = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._wiggleTween)
		{
			_wiggleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._mainPanel)
		{
			value = VariantUtils.CreateFrom(in _mainPanel);
			return true;
		}
		if (name == PropertyName._descriptionInput)
		{
			value = VariantUtils.CreateFrom(in _descriptionInput);
			return true;
		}
		if (name == PropertyName._emojiLabel)
		{
			value = VariantUtils.CreateFrom(in _emojiLabel);
			return true;
		}
		if (name == PropertyName._sendButton)
		{
			value = VariantUtils.CreateFrom(in _sendButton);
			return true;
		}
		if (name == PropertyName._sendLabel)
		{
			value = VariantUtils.CreateFrom(in _sendLabel);
			return true;
		}
		if (name == PropertyName._categoryDropdown)
		{
			value = VariantUtils.CreateFrom(in _categoryDropdown);
			return true;
		}
		if (name == PropertyName._successBackstop)
		{
			value = VariantUtils.CreateFrom(in _successBackstop);
			return true;
		}
		if (name == PropertyName._successPanel)
		{
			value = VariantUtils.CreateFrom(in _successPanel);
			return true;
		}
		if (name == PropertyName._successLabel)
		{
			value = VariantUtils.CreateFrom(in _successLabel);
			return true;
		}
		if (name == PropertyName._flower)
		{
			value = VariantUtils.CreateFrom(in _flower);
			return true;
		}
		if (name == PropertyName._selectedEmoteButton)
		{
			value = VariantUtils.CreateFrom(in _selectedEmoteButton);
			return true;
		}
		if (name == PropertyName._screenshotBytes)
		{
			value = VariantUtils.CreateFrom(in _screenshotBytes);
			return true;
		}
		if (name == PropertyName._originalSuccessPosition)
		{
			value = VariantUtils.CreateFrom(in _originalSuccessPosition);
			return true;
		}
		if (name == PropertyName._lastClosedMsec)
		{
			value = VariantUtils.CreateFrom(in _lastClosedMsec);
			return true;
		}
		if (name == PropertyName._descriptionText)
		{
			value = VariantUtils.CreateFrom(in _descriptionText);
			return true;
		}
		if (name == PropertyName._descriptionCaretLine)
		{
			value = VariantUtils.CreateFrom(in _descriptionCaretLine);
			return true;
		}
		if (name == PropertyName._descriptionCaretColumn)
		{
			value = VariantUtils.CreateFrom(in _descriptionCaretColumn);
			return true;
		}
		if (name == PropertyName._wiggleTween)
		{
			value = VariantUtils.CreateFrom(in _wiggleTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mainPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._descriptionInput, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._emojiLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sendButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sendLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._categoryDropdown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._successBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._successPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._successLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flower, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedEmoteButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedByteArray, PropertyName._screenshotBytes, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalSuccessPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._lastClosedMsec, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._descriptionText, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._descriptionCaretLine, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._descriptionCaretColumn, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._wiggleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._mainPanel, Variant.From(in _mainPanel));
		info.AddProperty(PropertyName._descriptionInput, Variant.From(in _descriptionInput));
		info.AddProperty(PropertyName._emojiLabel, Variant.From(in _emojiLabel));
		info.AddProperty(PropertyName._sendButton, Variant.From(in _sendButton));
		info.AddProperty(PropertyName._sendLabel, Variant.From(in _sendLabel));
		info.AddProperty(PropertyName._categoryDropdown, Variant.From(in _categoryDropdown));
		info.AddProperty(PropertyName._successBackstop, Variant.From(in _successBackstop));
		info.AddProperty(PropertyName._successPanel, Variant.From(in _successPanel));
		info.AddProperty(PropertyName._successLabel, Variant.From(in _successLabel));
		info.AddProperty(PropertyName._flower, Variant.From(in _flower));
		info.AddProperty(PropertyName._selectedEmoteButton, Variant.From(in _selectedEmoteButton));
		info.AddProperty(PropertyName._screenshotBytes, Variant.From(in _screenshotBytes));
		info.AddProperty(PropertyName._originalSuccessPosition, Variant.From(in _originalSuccessPosition));
		info.AddProperty(PropertyName._lastClosedMsec, Variant.From(in _lastClosedMsec));
		info.AddProperty(PropertyName._descriptionText, Variant.From(in _descriptionText));
		info.AddProperty(PropertyName._descriptionCaretLine, Variant.From(in _descriptionCaretLine));
		info.AddProperty(PropertyName._descriptionCaretColumn, Variant.From(in _descriptionCaretColumn));
		info.AddProperty(PropertyName._wiggleTween, Variant.From(in _wiggleTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._backButton, out var value))
		{
			_backButton = value.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._mainPanel, out var value2))
		{
			_mainPanel = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._descriptionInput, out var value3))
		{
			_descriptionInput = value3.As<NMegaTextEdit>();
		}
		if (info.TryGetProperty(PropertyName._emojiLabel, out var value4))
		{
			_emojiLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._sendButton, out var value5))
		{
			_sendButton = value5.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._sendLabel, out var value6))
		{
			_sendLabel = value6.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._categoryDropdown, out var value7))
		{
			_categoryDropdown = value7.As<NFeedbackCategoryDropdown>();
		}
		if (info.TryGetProperty(PropertyName._successBackstop, out var value8))
		{
			_successBackstop = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._successPanel, out var value9))
		{
			_successPanel = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._successLabel, out var value10))
		{
			_successLabel = value10.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._flower, out var value11))
		{
			_flower = value11.As<NSendFeedbackFlower>();
		}
		if (info.TryGetProperty(PropertyName._selectedEmoteButton, out var value12))
		{
			_selectedEmoteButton = value12.As<NSendFeedbackEmojiButton>();
		}
		if (info.TryGetProperty(PropertyName._screenshotBytes, out var value13))
		{
			_screenshotBytes = value13.As<byte[]>();
		}
		if (info.TryGetProperty(PropertyName._originalSuccessPosition, out var value14))
		{
			_originalSuccessPosition = value14.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._lastClosedMsec, out var value15))
		{
			_lastClosedMsec = value15.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._descriptionText, out var value16))
		{
			_descriptionText = value16.As<string>();
		}
		if (info.TryGetProperty(PropertyName._descriptionCaretLine, out var value17))
		{
			_descriptionCaretLine = value17.As<int>();
		}
		if (info.TryGetProperty(PropertyName._descriptionCaretColumn, out var value18))
		{
			_descriptionCaretColumn = value18.As<int>();
		}
		if (info.TryGetProperty(PropertyName._wiggleTween, out var value19))
		{
			_wiggleTween = value19.As<Tween>();
		}
	}
}
