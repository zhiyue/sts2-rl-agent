namespace MegaCrit.Sts2.Core.Nodes.Pooling;

public interface IPoolable
{
	void OnInstantiated();

	void OnReturnedFromPool();

	void OnFreedToPool();
}
