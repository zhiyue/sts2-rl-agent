using System;
using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Holds progress data on a per Epoch basis.
/// If a SerializableEpoch is NOT in the list of epochs in SerializableProgress,
/// it means the player has not revealed that Epoch's Slot via Timeline Expansion.
/// </summary>
public class SerializableEpoch
{
	[JsonPropertyName("id")]
	public string Id { get; }

	[JsonPropertyName("state")]
	public EpochState State { get; set; }

	[JsonPropertyName("obtain_date")]
	public long ObtainDate { get; set; }

	public SerializableEpoch(string id, EpochState state)
	{
		Id = id;
		State = EpochState.NotObtained;
		if ((uint)(state - 3) <= 2u)
		{
			SetObtained(state);
		}
	}

	public void SetObtained(EpochState state)
	{
		if (State != EpochState.ObtainedNoSlot && State != EpochState.Obtained && State != EpochState.Revealed)
		{
			ObtainDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			State = state;
		}
	}
}
