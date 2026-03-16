using System;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;

public class CrystalSphereCell
{
	private bool _isHidden;

	private bool _isHighlighted;

	private bool _isHovered;

	public CrystalSphereItem? Item { get; private set; }

	public int X { get; private set; }

	public int Y { get; private set; }

	public bool IsHidden
	{
		get
		{
			return _isHidden;
		}
		set
		{
			_isHidden = value;
			this.FogUpdated?.Invoke();
		}
	}

	public bool IsHighlighted
	{
		get
		{
			return _isHighlighted;
		}
		set
		{
			_isHighlighted = value;
			this.HighlightUpdated?.Invoke();
		}
	}

	public bool IsHovered
	{
		get
		{
			return _isHovered;
		}
		set
		{
			_isHovered = value;
			this.HighlightUpdated?.Invoke();
		}
	}

	public event Action? FogUpdated;

	public event Action? HighlightUpdated;

	public CrystalSphereCell(int x, int y)
	{
		X = x;
		Y = y;
		IsHidden = true;
	}

	public void SetItem(CrystalSphereItem? item)
	{
		if (Item != null)
		{
			throw new InvalidOperationException("An item already occupies this cell");
		}
		Item = item;
	}
}
