namespace MegaCrit.Sts2.Core.Nodes.Pooling;

public interface IPoolable
{
	/// <summary>
	/// Called the first time an object is instantiated for use in a pool.
	/// </summary>
	void OnInstantiated();

	/// <summary>
	/// Called when a pooled object is retrieved from a pool.
	/// </summary>
	void OnReturnedFromPool();

	/// <summary>
	/// Called when a pooled object is freed and put back into a pool for reuse later.
	/// </summary>
	void OnFreedToPool();
}
