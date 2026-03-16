namespace MegaCrit.Sts2.Core.Nodes.Pooling;

public interface INodePool
{
	IPoolable Get();

	void Free(IPoolable poolable);
}
