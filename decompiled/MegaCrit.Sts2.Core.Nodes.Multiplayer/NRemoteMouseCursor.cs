using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NRemoteMouseCursor.cs")]
public class NRemoteMouseCursor : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetNextPosition = "SetNextPosition";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName UpdateImage = "UpdateImage";

		public static readonly StringName GetHotspot = "GetHotspot";

		public static readonly StringName GetTexture = "GetTexture";

		public static readonly StringName RefreshSize = "RefreshSize";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName PlayerId = "PlayerId";

		public static readonly StringName _textureRect = "_textureRect";

		public static readonly StringName _lastPositionUpdateMsec = "_lastPositionUpdateMsec";

		public static readonly StringName _defaultHotspot = "_defaultHotspot";

		public static readonly StringName _drawingHotspot = "_drawingHotspot";

		public static readonly StringName _erasingHotspot = "_erasingHotspot";

		public static readonly StringName _defaultCursorImage = "_defaultCursorImage";

		public static readonly StringName _tiltedCursorImage = "_tiltedCursorImage";

		public static readonly StringName _defaultDrawingImage = "_defaultDrawingImage";

		public static readonly StringName _tiltedDrawingImage = "_tiltedDrawingImage";

		public static readonly StringName _defaultErasingImage = "_defaultErasingImage";

		public static readonly StringName _tiltedErasingImage = "_tiltedErasingImage";

		public static readonly StringName _defaultCursorTexture = "_defaultCursorTexture";

		public static readonly StringName _tiltedCursorTexture = "_tiltedCursorTexture";

		public static readonly StringName _defaultDrawingTexture = "_defaultDrawingTexture";

		public static readonly StringName _tiltedDrawingTexture = "_tiltedDrawingTexture";

		public static readonly StringName _defaultErasingTexture = "_defaultErasingTexture";

		public static readonly StringName _tiltedErasingTexture = "_tiltedErasingTexture";

		public static readonly StringName _drawingMode = "_drawingMode";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "ui/multiplayer/remote_mouse_cursor";

	private TextureRect _textureRect;

	private Vector2? _previousPosition;

	private Vector2? _nextPosition;

	private ulong _lastPositionUpdateMsec;

	private Vector2 _defaultHotspot;

	private Vector2 _drawingHotspot;

	private Vector2 _erasingHotspot;

	[Export(PropertyHint.None, "")]
	private Image _defaultCursorImage;

	[Export(PropertyHint.None, "")]
	private Image _tiltedCursorImage;

	[Export(PropertyHint.None, "")]
	private Image _defaultDrawingImage;

	[Export(PropertyHint.None, "")]
	private Image _tiltedDrawingImage;

	[Export(PropertyHint.None, "")]
	private Image _defaultErasingImage;

	[Export(PropertyHint.None, "")]
	private Image _tiltedErasingImage;

	private ImageTexture _defaultCursorTexture;

	private ImageTexture _tiltedCursorTexture;

	private ImageTexture _defaultDrawingTexture;

	private ImageTexture _tiltedDrawingTexture;

	private ImageTexture _defaultErasingTexture;

	private ImageTexture _tiltedErasingTexture;

	private DrawingMode _drawingMode;

	public ulong PlayerId { get; private set; }

	public static NRemoteMouseCursor Create(ulong playerId)
	{
		NRemoteMouseCursor nRemoteMouseCursor = PreloadManager.Cache.GetAsset<PackedScene>(SceneHelper.GetScenePath("ui/multiplayer/remote_mouse_cursor")).Instantiate<NRemoteMouseCursor>(PackedScene.GenEditState.Disabled);
		nRemoteMouseCursor.PlayerId = playerId;
		return nRemoteMouseCursor;
	}

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		_defaultHotspot = -_textureRect.Position;
		_drawingHotspot = NMapDrawings.drawingCursorHotspot;
		_erasingHotspot = NMapDrawings.erasingCursorHotspot;
		base.ProcessMode = ProcessModeEnum.Disabled;
		_defaultCursorTexture = ImageTexture.CreateFromImage(_defaultCursorImage);
		_tiltedCursorTexture = ImageTexture.CreateFromImage(_tiltedCursorImage);
		_defaultDrawingTexture = ImageTexture.CreateFromImage(_defaultDrawingImage);
		_tiltedDrawingTexture = ImageTexture.CreateFromImage(_tiltedDrawingImage);
		_defaultErasingTexture = ImageTexture.CreateFromImage(_defaultErasingImage);
		_tiltedErasingTexture = ImageTexture.CreateFromImage(_tiltedErasingImage);
		GetViewport().Connect(Viewport.SignalName.SizeChanged, Callable.From(RefreshSize));
	}

	public void SetNextPosition(Vector2 position)
	{
		if (!_nextPosition.HasValue)
		{
			_nextPosition = position;
		}
		_previousPosition = _nextPosition;
		_nextPosition = position;
		_lastPositionUpdateMsec = Time.GetTicksMsec();
		base.ProcessMode = ProcessModeEnum.Inherit;
	}

	public override void _Process(double delta)
	{
		if (_previousPosition.HasValue && _nextPosition.HasValue)
		{
			float num = (float)(Time.GetTicksMsec() - _lastPositionUpdateMsec) / 50f;
			base.Position = _previousPosition.Value.Lerp(_nextPosition.Value, Mathf.Clamp(num, 0f, 1f));
			if (num >= 1f)
			{
				base.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
	}

	public void UpdateImage(bool isDown, DrawingMode drawingMode)
	{
		_textureRect.Texture = GetTexture(isDown, drawingMode);
		_drawingMode = drawingMode;
		RefreshSize();
	}

	private Vector2 GetHotspot(DrawingMode drawingMode)
	{
		switch (drawingMode)
		{
		case DrawingMode.None:
			return -_defaultHotspot;
		case DrawingMode.Drawing:
			return -_drawingHotspot;
		case DrawingMode.Erasing:
			return -_erasingHotspot;
		default:
		{
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(drawingMode);
			Vector2 result = default(Vector2);
			return result;
		}
		}
	}

	private Texture2D GetTexture(bool isDown, DrawingMode drawingMode)
	{
		switch (drawingMode)
		{
		case DrawingMode.None:
			return isDown ? _tiltedCursorTexture : _defaultCursorTexture;
		case DrawingMode.Drawing:
			return isDown ? _defaultDrawingTexture : _tiltedDrawingTexture;
		case DrawingMode.Erasing:
			return isDown ? _defaultErasingTexture : _tiltedErasingTexture;
		default:
		{
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(drawingMode);
			ImageTexture result = default(ImageTexture);
			return result;
		}
		}
	}

	public void RefreshSize()
	{
		if (OS.GetName() == "Windows")
		{
			int num = DisplayServer.ScreenGetDpi();
			float num2 = (float)num / 96f;
			Vector2 vector = GetViewport().GetStretchTransform().Scale.Inverse();
			Vector2 size = _textureRect.Texture.GetSize();
			Vector2 size2 = size * vector * num2;
			_textureRect.Size = size2;
			_textureRect.Position = GetHotspot(_drawingMode) * vector * num2;
		}
		else
		{
			_textureRect.Size = _textureRect.Texture.GetSize();
			_textureRect.Position = GetHotspot(_drawingMode);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetNextPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateImage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isDown", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "drawingMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetHotspot, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "drawingMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetTexture, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isDown", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "drawingMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NRemoteMouseCursor>(Create(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetNextPosition && args.Count == 1)
		{
			SetNextPosition(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateImage && args.Count == 2)
		{
			UpdateImage(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<DrawingMode>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetHotspot && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetHotspot(VariantUtils.ConvertTo<DrawingMode>(in args[0])));
			return true;
		}
		if (method == MethodName.GetTexture && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(GetTexture(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<DrawingMode>(in args[1])));
			return true;
		}
		if (method == MethodName.RefreshSize && args.Count == 0)
		{
			RefreshSize();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NRemoteMouseCursor>(Create(VariantUtils.ConvertTo<ulong>(in args[0])));
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
		if (method == MethodName.SetNextPosition)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateImage)
		{
			return true;
		}
		if (method == MethodName.GetHotspot)
		{
			return true;
		}
		if (method == MethodName.GetTexture)
		{
			return true;
		}
		if (method == MethodName.RefreshSize)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.PlayerId)
		{
			PlayerId = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._textureRect)
		{
			_textureRect = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._lastPositionUpdateMsec)
		{
			_lastPositionUpdateMsec = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._defaultHotspot)
		{
			_defaultHotspot = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._drawingHotspot)
		{
			_drawingHotspot = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._erasingHotspot)
		{
			_erasingHotspot = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._defaultCursorImage)
		{
			_defaultCursorImage = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._tiltedCursorImage)
		{
			_tiltedCursorImage = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._defaultDrawingImage)
		{
			_defaultDrawingImage = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._tiltedDrawingImage)
		{
			_tiltedDrawingImage = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._defaultErasingImage)
		{
			_defaultErasingImage = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._tiltedErasingImage)
		{
			_tiltedErasingImage = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._defaultCursorTexture)
		{
			_defaultCursorTexture = VariantUtils.ConvertTo<ImageTexture>(in value);
			return true;
		}
		if (name == PropertyName._tiltedCursorTexture)
		{
			_tiltedCursorTexture = VariantUtils.ConvertTo<ImageTexture>(in value);
			return true;
		}
		if (name == PropertyName._defaultDrawingTexture)
		{
			_defaultDrawingTexture = VariantUtils.ConvertTo<ImageTexture>(in value);
			return true;
		}
		if (name == PropertyName._tiltedDrawingTexture)
		{
			_tiltedDrawingTexture = VariantUtils.ConvertTo<ImageTexture>(in value);
			return true;
		}
		if (name == PropertyName._defaultErasingTexture)
		{
			_defaultErasingTexture = VariantUtils.ConvertTo<ImageTexture>(in value);
			return true;
		}
		if (name == PropertyName._tiltedErasingTexture)
		{
			_tiltedErasingTexture = VariantUtils.ConvertTo<ImageTexture>(in value);
			return true;
		}
		if (name == PropertyName._drawingMode)
		{
			_drawingMode = VariantUtils.ConvertTo<DrawingMode>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.PlayerId)
		{
			value = VariantUtils.CreateFrom<ulong>(PlayerId);
			return true;
		}
		if (name == PropertyName._textureRect)
		{
			value = VariantUtils.CreateFrom(in _textureRect);
			return true;
		}
		if (name == PropertyName._lastPositionUpdateMsec)
		{
			value = VariantUtils.CreateFrom(in _lastPositionUpdateMsec);
			return true;
		}
		if (name == PropertyName._defaultHotspot)
		{
			value = VariantUtils.CreateFrom(in _defaultHotspot);
			return true;
		}
		if (name == PropertyName._drawingHotspot)
		{
			value = VariantUtils.CreateFrom(in _drawingHotspot);
			return true;
		}
		if (name == PropertyName._erasingHotspot)
		{
			value = VariantUtils.CreateFrom(in _erasingHotspot);
			return true;
		}
		if (name == PropertyName._defaultCursorImage)
		{
			value = VariantUtils.CreateFrom(in _defaultCursorImage);
			return true;
		}
		if (name == PropertyName._tiltedCursorImage)
		{
			value = VariantUtils.CreateFrom(in _tiltedCursorImage);
			return true;
		}
		if (name == PropertyName._defaultDrawingImage)
		{
			value = VariantUtils.CreateFrom(in _defaultDrawingImage);
			return true;
		}
		if (name == PropertyName._tiltedDrawingImage)
		{
			value = VariantUtils.CreateFrom(in _tiltedDrawingImage);
			return true;
		}
		if (name == PropertyName._defaultErasingImage)
		{
			value = VariantUtils.CreateFrom(in _defaultErasingImage);
			return true;
		}
		if (name == PropertyName._tiltedErasingImage)
		{
			value = VariantUtils.CreateFrom(in _tiltedErasingImage);
			return true;
		}
		if (name == PropertyName._defaultCursorTexture)
		{
			value = VariantUtils.CreateFrom(in _defaultCursorTexture);
			return true;
		}
		if (name == PropertyName._tiltedCursorTexture)
		{
			value = VariantUtils.CreateFrom(in _tiltedCursorTexture);
			return true;
		}
		if (name == PropertyName._defaultDrawingTexture)
		{
			value = VariantUtils.CreateFrom(in _defaultDrawingTexture);
			return true;
		}
		if (name == PropertyName._tiltedDrawingTexture)
		{
			value = VariantUtils.CreateFrom(in _tiltedDrawingTexture);
			return true;
		}
		if (name == PropertyName._defaultErasingTexture)
		{
			value = VariantUtils.CreateFrom(in _defaultErasingTexture);
			return true;
		}
		if (name == PropertyName._tiltedErasingTexture)
		{
			value = VariantUtils.CreateFrom(in _tiltedErasingTexture);
			return true;
		}
		if (name == PropertyName._drawingMode)
		{
			value = VariantUtils.CreateFrom(in _drawingMode);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textureRect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.PlayerId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._lastPositionUpdateMsec, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._defaultHotspot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._drawingHotspot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._erasingHotspot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._defaultCursorImage, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tiltedCursorImage, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._defaultDrawingImage, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tiltedDrawingImage, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._defaultErasingImage, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tiltedErasingImage, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._defaultCursorTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tiltedCursorTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._defaultDrawingTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tiltedDrawingTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._defaultErasingTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tiltedErasingTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._drawingMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.PlayerId, Variant.From<ulong>(PlayerId));
		info.AddProperty(PropertyName._textureRect, Variant.From(in _textureRect));
		info.AddProperty(PropertyName._lastPositionUpdateMsec, Variant.From(in _lastPositionUpdateMsec));
		info.AddProperty(PropertyName._defaultHotspot, Variant.From(in _defaultHotspot));
		info.AddProperty(PropertyName._drawingHotspot, Variant.From(in _drawingHotspot));
		info.AddProperty(PropertyName._erasingHotspot, Variant.From(in _erasingHotspot));
		info.AddProperty(PropertyName._defaultCursorImage, Variant.From(in _defaultCursorImage));
		info.AddProperty(PropertyName._tiltedCursorImage, Variant.From(in _tiltedCursorImage));
		info.AddProperty(PropertyName._defaultDrawingImage, Variant.From(in _defaultDrawingImage));
		info.AddProperty(PropertyName._tiltedDrawingImage, Variant.From(in _tiltedDrawingImage));
		info.AddProperty(PropertyName._defaultErasingImage, Variant.From(in _defaultErasingImage));
		info.AddProperty(PropertyName._tiltedErasingImage, Variant.From(in _tiltedErasingImage));
		info.AddProperty(PropertyName._defaultCursorTexture, Variant.From(in _defaultCursorTexture));
		info.AddProperty(PropertyName._tiltedCursorTexture, Variant.From(in _tiltedCursorTexture));
		info.AddProperty(PropertyName._defaultDrawingTexture, Variant.From(in _defaultDrawingTexture));
		info.AddProperty(PropertyName._tiltedDrawingTexture, Variant.From(in _tiltedDrawingTexture));
		info.AddProperty(PropertyName._defaultErasingTexture, Variant.From(in _defaultErasingTexture));
		info.AddProperty(PropertyName._tiltedErasingTexture, Variant.From(in _tiltedErasingTexture));
		info.AddProperty(PropertyName._drawingMode, Variant.From(in _drawingMode));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.PlayerId, out var value))
		{
			PlayerId = value.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._textureRect, out var value2))
		{
			_textureRect = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._lastPositionUpdateMsec, out var value3))
		{
			_lastPositionUpdateMsec = value3.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._defaultHotspot, out var value4))
		{
			_defaultHotspot = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._drawingHotspot, out var value5))
		{
			_drawingHotspot = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._erasingHotspot, out var value6))
		{
			_erasingHotspot = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._defaultCursorImage, out var value7))
		{
			_defaultCursorImage = value7.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._tiltedCursorImage, out var value8))
		{
			_tiltedCursorImage = value8.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._defaultDrawingImage, out var value9))
		{
			_defaultDrawingImage = value9.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._tiltedDrawingImage, out var value10))
		{
			_tiltedDrawingImage = value10.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._defaultErasingImage, out var value11))
		{
			_defaultErasingImage = value11.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._tiltedErasingImage, out var value12))
		{
			_tiltedErasingImage = value12.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._defaultCursorTexture, out var value13))
		{
			_defaultCursorTexture = value13.As<ImageTexture>();
		}
		if (info.TryGetProperty(PropertyName._tiltedCursorTexture, out var value14))
		{
			_tiltedCursorTexture = value14.As<ImageTexture>();
		}
		if (info.TryGetProperty(PropertyName._defaultDrawingTexture, out var value15))
		{
			_defaultDrawingTexture = value15.As<ImageTexture>();
		}
		if (info.TryGetProperty(PropertyName._tiltedDrawingTexture, out var value16))
		{
			_tiltedDrawingTexture = value16.As<ImageTexture>();
		}
		if (info.TryGetProperty(PropertyName._defaultErasingTexture, out var value17))
		{
			_defaultErasingTexture = value17.As<ImageTexture>();
		}
		if (info.TryGetProperty(PropertyName._tiltedErasingTexture, out var value18))
		{
			_tiltedErasingTexture = value18.As<ImageTexture>();
		}
		if (info.TryGetProperty(PropertyName._drawingMode, out var value19))
		{
			_drawingMode = value19.As<DrawingMode>();
		}
	}
}
