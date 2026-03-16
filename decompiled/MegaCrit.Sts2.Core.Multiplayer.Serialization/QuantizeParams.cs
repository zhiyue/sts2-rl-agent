namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public struct QuantizeParams
{
	public float min;

	public float max;

	public int bits;

	public QuantizeParams(float min, float max, int bits)
	{
		this.min = min;
		this.max = max;
		this.bits = bits;
	}
}
