using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Nodes.Pooling;

public class NodePool
{
	private static System.Collections.Generic.Dictionary<Type, INodePool> _pools = new System.Collections.Generic.Dictionary<Type, INodePool>();

	public static NodePool<T> Init<T>(string scenePath, int prewarmCount) where T : Node, IPoolable
	{
		Type typeFromHandle = typeof(T);
		if (_pools.TryGetValue(typeFromHandle, out INodePool _))
		{
			throw new InvalidOperationException($"Tried to init NodePool for type {typeof(T)} but it's already initialized!");
		}
		NodePool<T> nodePool = new NodePool<T>(scenePath, prewarmCount);
		_pools[typeFromHandle] = nodePool;
		return nodePool;
	}

	public static IPoolable Get(Type type)
	{
		if (!_pools.TryGetValue(type, out INodePool value))
		{
			throw new InvalidOperationException($"Tried to get pool for type {type} before it was initialized!");
		}
		return value.Get();
	}

	public static void Free(IPoolable poolable)
	{
		Type type = poolable.GetType();
		if (!_pools.TryGetValue(type, out INodePool value))
		{
			throw new InvalidOperationException($"Tried to get pool for type {type} before it was initialized!");
		}
		value.Free(poolable);
	}

	public static T Get<T>() where T : Node, IPoolable
	{
		return (T)Get(typeof(T));
	}

	public static void Free<T>(T obj) where T : Node, IPoolable
	{
		Free((IPoolable)obj);
	}
}
public class NodePool<T> : INodePool where T : Node, IPoolable
{
	private static Variant _nameStr = Variant.CreateFrom("name");

	private static Variant _callableStr = Variant.CreateFrom("callable");

	private static Variant _signalStr = Variant.CreateFrom("signal");

	private string _scenePath;

	private readonly List<T> _freeObjects = new List<T>();

	private readonly HashSet<T> _usedObjects = new HashSet<T>();

	public IReadOnlyList<T> DebugFreeObjects => _freeObjects;

	public NodePool(string scenePath, int prewarmCount = 0)
	{
		_scenePath = scenePath;
		for (int i = 0; i < prewarmCount; i++)
		{
			_freeObjects.Add(Instantiate());
		}
	}

	IPoolable INodePool.Get()
	{
		return Get();
	}

	void INodePool.Free(IPoolable poolable)
	{
		Free((T)poolable);
	}

	public T Get()
	{
		T val;
		if (_freeObjects.Count > 0)
		{
			List<T> freeObjects = _freeObjects;
			val = freeObjects[freeObjects.Count - 1];
			_freeObjects.RemoveAt(_freeObjects.Count - 1);
		}
		else
		{
			val = Instantiate();
		}
		_usedObjects.Add(val);
		val.OnReturnedFromPool();
		return val;
	}

	public void Free(T obj)
	{
		if (!_usedObjects.Contains(obj))
		{
			if (_freeObjects.Contains(obj))
			{
				Log.Error($"Tried to free object {obj} ({obj.GetType()}) back to pool {typeof(NodePool<T>)} but it's already been freed!");
			}
			else
			{
				Log.Error($"Tried to free object {obj} ({obj.GetType()}) back to pool {typeof(NodePool<T>)} but it's not part of the pool!");
			}
			obj.QueueFreeSafelyNoPool();
		}
		else
		{
			DisconnectIncomingAndOutgoingSignals(obj);
			_usedObjects.Remove(obj);
			_freeObjects.Add(obj);
			obj.OnFreedToPool();
		}
	}

	private T Instantiate()
	{
		T val = PreloadManager.Cache.GetScene(_scenePath).Instantiate<T>(PackedScene.GenEditState.Disabled);
		val.OnInstantiated();
		return val;
	}

	private void DisconnectIncomingAndOutgoingSignals(Node obj)
	{
		foreach (Dictionary signal4 in obj.GetSignalList())
		{
			StringName signal = signal4[_nameStr].AsStringName();
			foreach (Dictionary signalConnection in obj.GetSignalConnectionList(signal))
			{
				Callable callable = signalConnection[_callableStr].AsCallable();
				Signal signal2 = signalConnection[_signalStr].AsSignal();
				DisconnectSignal(callable, signal2);
			}
		}
		foreach (Dictionary incomingConnection in obj.GetIncomingConnections())
		{
			Callable callable2 = incomingConnection[_callableStr].AsCallable();
			Signal signal3 = incomingConnection[_signalStr].AsSignal();
			DisconnectSignal(callable2, signal3);
		}
		for (int i = 0; i < obj.GetChildCount(); i++)
		{
			DisconnectIncomingAndOutgoingSignals(obj.GetChild(i));
		}
	}

	private void DisconnectSignal(Callable callable, Signal signal)
	{
		GodotObject target = callable.Target;
		if (target == null && callable.Method == null)
		{
			return;
		}
		StringName name = signal.Name;
		Node node = target as Node;
		if (node == null || node.IsInsideTree())
		{
			GodotObject owner = signal.Owner;
			Node node2 = owner as Node;
			if (node != null && node.HasSignal(name) && node.IsConnected(name, callable))
			{
				node.Disconnect(name, callable);
			}
			else if (node2 != null && node2.HasSignal(name) && node2.IsConnected(name, callable))
			{
				node2.Disconnect(name, callable);
			}
		}
	}
}
