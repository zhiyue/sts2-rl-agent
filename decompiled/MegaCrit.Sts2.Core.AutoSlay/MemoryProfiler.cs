using System;
using System.Linq;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes;

namespace MegaCrit.Sts2.Core.AutoSlay;

/// <summary>
/// Captures memory and resource snapshots during AutoSlay runs, logging deltas
/// from a baseline to detect memory/resource leaks.
/// </summary>
public static class MemoryProfiler
{
	private record struct MemorySnapshot(ulong StaticMemBytes, ulong VramBytes, long GcTotalMemory, int ObjectCount, int ResourceCount, int NodeCount, int OrphanNodeCount, int CachedAssets, int MissedCacheAssets, int RootViewportSignals, int GcGen0, int GcGen1, int GcGen2);

	private static MemorySnapshot? _baseline;

	private static MemorySnapshot? _previous;

	private static int _snapshotCount;

	public static void SetBaseline()
	{
		MemorySnapshot memorySnapshot = Capture();
		_baseline = memorySnapshot;
		_previous = memorySnapshot;
		LogLine("baseline", memorySnapshot, memorySnapshot);
	}

	public static void LogSnapshot(string context)
	{
		MemorySnapshot memorySnapshot = Capture();
		MemorySnapshot baseline = _baseline ?? memorySnapshot;
		LogLine(context, memorySnapshot, baseline);
		_previous = memorySnapshot;
	}

	public static void Reset()
	{
		_baseline = null;
		_previous = null;
		_snapshotCount = 0;
	}

	private static MemorySnapshot Capture()
	{
		Window window = NGame.Instance?.GetTree()?.Root;
		int rootViewportSignals = 0;
		if (window != null)
		{
			Array<Dictionary> signalConnectionList = window.GetSignalConnectionList(Viewport.SignalName.SizeChanged);
			rootViewportSignals = signalConnectionList.Count;
		}
		return new MemorySnapshot(OS.GetStaticMemoryUsage(), RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.VideoMemUsed), GC.GetTotalMemory(forceFullCollection: false), (int)Performance.GetMonitor(Performance.Monitor.ObjectCount), (int)Performance.GetMonitor(Performance.Monitor.ObjectResourceCount), (int)Performance.GetMonitor(Performance.Monitor.ObjectNodeCount), (int)Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount), PreloadManager.Cache.GetCacheKeys().Count(), PreloadManager.Cache.MissedCacheAssetCount, rootViewportSignals, GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
	}

	private static void LogLine(string context, MemorySnapshot current, MemorySnapshot baseline)
	{
		MemorySnapshot memorySnapshot = _previous ?? current;
		string text = $"[MemProfile] context={context} | StaticMem={Fmt(current.StaticMemBytes)}({Diff(current.StaticMemBytes, baseline.StaticMemBytes)}) VRAM={Fmt(current.VramBytes)}({Diff(current.VramBytes, baseline.VramBytes)}) GcMem={Fmt((ulong)current.GcTotalMemory)}({Diff((ulong)current.GcTotalMemory, (ulong)baseline.GcTotalMemory)}) Objects={current.ObjectCount}({Diff(current.ObjectCount, baseline.ObjectCount)}) Resources={current.ResourceCount}({Diff(current.ResourceCount, baseline.ResourceCount)}) Nodes={current.NodeCount}({Diff(current.NodeCount, baseline.NodeCount)}) Orphans={current.OrphanNodeCount}({Diff(current.OrphanNodeCount, baseline.OrphanNodeCount)}) CachedAssets={current.CachedAssets}({Diff(current.CachedAssets, baseline.CachedAssets)}) MissedCache={current.MissedCacheAssets}({Diff(current.MissedCacheAssets, baseline.MissedCacheAssets)}) RootSizeSignals={current.RootViewportSignals}({Diff(current.RootViewportSignals, baseline.RootViewportSignals)}) GC0={current.GcGen0}({Diff(current.GcGen0, baseline.GcGen0)}) GC1={current.GcGen1}({Diff(current.GcGen1, baseline.GcGen1)}) GC2={current.GcGen2}({Diff(current.GcGen2, baseline.GcGen2)})";
		if (_snapshotCount >= 2)
		{
			text += $" | room-delta: StaticMem={Diff(current.StaticMemBytes, memorySnapshot.StaticMemBytes)} VRAM={Diff(current.VramBytes, memorySnapshot.VramBytes)} GcMem={Diff((ulong)current.GcTotalMemory, (ulong)memorySnapshot.GcTotalMemory)} Objects={Diff(current.ObjectCount, memorySnapshot.ObjectCount)} Nodes={Diff(current.NodeCount, memorySnapshot.NodeCount)} Orphans={Diff(current.OrphanNodeCount, memorySnapshot.OrphanNodeCount)}";
		}
		_snapshotCount++;
		AutoSlayLog.Info(text);
	}

	private static string Fmt(ulong bytes)
	{
		return NGame.FormatBytes(bytes);
	}

	private static string Diff(ulong current, ulong baseline)
	{
		long num = (long)(current - baseline);
		if (num < 0)
		{
			return "-" + NGame.FormatBytes((ulong)(-num));
		}
		return "+" + NGame.FormatBytes((ulong)num);
	}

	private static string Diff(int current, int baseline)
	{
		int num = current - baseline;
		if (num >= 0)
		{
			return $"+{num}";
		}
		return $"{num}";
	}
}
