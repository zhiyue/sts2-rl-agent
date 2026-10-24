using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MegaCrit.Sts2.Core.Random;

/// <summary>
/// Xoshiro256** (xor, shift, rotate) pseudo-random number generator (PRNG).
/// The original algorithm's license is as follows.
///
/// ====================================================================
/// Written in 2018 by David Blackman and Sebastiano Vigna (vigna@acm.org)
///
/// To the extent possible under law, the author has dedicated all copyright
/// and related and neighboring rights to this software to the public domain
/// worldwide. This software is distributed without any warranty.
/// See <see href="http://creativecommons.org/publicdomain/zero/1.0/" />. */
/// =====================================================================
///
/// This particular implementation is copied from the Redzen library with heavy modifications:
/// https://github.com/colgreen/Redzen/blob/main/Redzen/Random/Xoshiro256StarStarRandom.cs
/// The original license of the Redzen library is as follows.
///
/// =====================================================================
/// Redzen code library.
///
/// Copyright 2015-2023 Colin D. Green (colin.green1@gmail.com)
///
/// This software is issued under the MIT License.
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
/// </summary>
public sealed class MegaRandom
{
	private const double _incrDouble = 1.1102230246251565E-16;

	private const float _incrFloat = 5.9604645E-08f;

	private ulong _s0;

	private ulong _s1;

	private ulong _s2;

	private ulong _s3;

	/// <summary>
	/// Initializes a new instance with a seed from the default seed source.
	/// </summary>
	public MegaRandom()
	{
		Reinitialise(0uL);
	}

	/// <summary>
	/// Initializes a new instance with the provided seed.
	/// </summary>
	/// <param name="seed">Seed value.</param>
	public MegaRandom(ulong seed)
	{
		Reinitialise(seed);
	}

	/// <summary>
	/// Splitmix64 PRNG.
	/// </summary>
	/// <param name="x">PRNG state. This can take any value, including zero.</param>
	/// <returns>A new random UInt64.</returns>
	public static ulong Splitmix64(ref ulong x)
	{
		ulong num = (x += 11400714819323198485uL);
		num = (num ^ (num >> 30)) * 13787848793156543929uL;
		num = (num ^ (num >> 27)) * 10723151780598845931uL;
		return num ^ (num >> 31);
	}

	/// <summary>
	/// Re-initialises the random number generator state using the provided seed value.
	/// </summary>
	/// <param name="seed">Seed value.</param>
	public void Reinitialise(ulong seed)
	{
		_s0 = Splitmix64(ref seed);
		_s1 = Splitmix64(ref seed);
		_s2 = Splitmix64(ref seed);
		_s3 = Splitmix64(ref seed);
	}

	/// <summary>
	/// Fills the provided span with random byte values, sampled from the uniform distribution with interval [0, 255].
	/// </summary>
	/// <param name="span">The byte span to fill with random samples.</param>
	public unsafe void NextBytes(Span<byte> span)
	{
		ulong num = _s0;
		ulong num2 = _s1;
		ulong num3 = _s2;
		ulong num4 = _s3;
		while (span.Length >= 8)
		{
			Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), BitOperations.RotateLeft(num2 * 5, 7) * 9);
			ulong num5 = num2 << 17;
			num3 ^= num;
			num4 ^= num2;
			num2 ^= num3;
			num ^= num4;
			num3 ^= num5;
			num4 = BitOperations.RotateLeft(num4, 45);
			span = span.Slice(8);
		}
		if (!span.IsEmpty)
		{
			ulong num6 = BitOperations.RotateLeft(num2 * 5, 7) * 9;
			byte* ptr = (byte*)(&num6);
			for (int i = 0; i < span.Length; i++)
			{
				span[i] = ptr[i];
			}
			ulong num7 = num2 << 17;
			num3 ^= num;
			num4 ^= num2;
			num2 ^= num3;
			num ^= num4;
			num3 ^= num7;
			num4 = BitOperations.RotateLeft(num4, 45);
		}
		_s0 = num;
		_s1 = num2;
		_s2 = num3;
		_s3 = num4;
	}

	/// <summary>
	/// Get the next 64 random bits from the underlying PRNG. This method forms the foundation for most of the methods of each
	/// Next* implementation, which take these 64 bits and manipulate them to provide random values of various
	/// data types, such as integers, byte arrays, floating point values, etc.
	/// </summary>
	/// <returns>A <see cref="T:System.UInt64" /> containing random bits from the underlying PRNG algorithm.</returns>
	private ulong NextULongInner()
	{
		ulong s = _s0;
		ulong s2 = _s1;
		ulong s3 = _s2;
		ulong s4 = _s3;
		ulong result = BitOperations.RotateLeft(s2 * 5, 7) * 9;
		ulong num = s2 << 17;
		s3 ^= s;
		s4 ^= s2;
		s2 ^= s3;
		s ^= s4;
		s3 ^= num;
		s4 = BitOperations.RotateLeft(s4, 45);
		_s0 = s;
		_s1 = s2;
		_s2 = s3;
		_s3 = s4;
		return result;
	}

	/// <summary>
	/// Returns a random integer sampled from the uniform distribution with interval [0, maxValue),
	/// i.e., exclusive of <paramref name="maxValue" />.
	/// </summary>
	/// <param name="maxValue">The maximum value to be sampled (exclusive).</param>
	/// <returns>A new random sample.</returns>
	public int Next(int maxValue)
	{
		if (maxValue < 1)
		{
			throw new ArgumentOutOfRangeException("maxValue", maxValue, "maxValue must be > 0");
		}
		return NextInner(maxValue);
	}

	/// <summary>
	/// Returns a random integer sampled from the uniform distribution with interval [minValue, maxValue),
	/// i.e., inclusive of <paramref name="minValue" /> and exclusive of <paramref name="maxValue" />.
	/// </summary>
	/// <param name="minValue">The minimum value to be sampled (inclusive).</param>
	/// <param name="maxValue">The maximum value to be sampled (exclusive).</param>
	/// <returns>A new random sample.</returns>
	/// <remarks>
	/// maxValue must be greater than minValue. minValue may be negative.
	/// </remarks>
	public int Next(int minValue, int maxValue)
	{
		if (minValue >= maxValue)
		{
			throw new ArgumentOutOfRangeException("maxValue", maxValue, "maxValue must be > minValue");
		}
		long num = (long)maxValue - (long)minValue;
		if (num <= int.MaxValue)
		{
			return NextInner((int)num) + minValue;
		}
		return (int)(NextInner(num) + minValue);
	}

	/// <summary>
	/// Returns a random <see cref="T:System.Double" /> sampled from the uniform distribution with interval [0, 1),
	/// i.e., inclusive of 0.0 and exclusive of 1.0.
	/// </summary>
	/// <returns>A new random sample, of type <see cref="T:System.Double" />.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double NextDouble()
	{
		return (double)(NextULongInner() >> 11) * 1.1102230246251565E-16;
	}

	/// <summary>
	/// Returns a random integer sampled from the uniform distribution with interval [0, int.MaxValue],
	/// i.e., <b>inclusive</b> of <see cref="F:System.Int32.MaxValue" />.
	/// </summary>
	/// <returns>A new random sample.</returns>
	/// <remarks>
	/// This method differs from <see cref="M:System.Random.Next" />, in the following way; the uniform distribution that
	/// is sampled from includes the value <see cref="F:System.Int32.MaxValue" />.
	/// </remarks>
	public int NextInt()
	{
		return (int)(NextULongInner() >> 33);
	}

	/// <summary>
	/// Returns a random <see cref="T:System.UInt32" /> sampled from the uniform distribution with interval [0, uint.MaxValue],
	/// i.e., over the full range of possible uint values.
	/// </summary>
	/// <returns>A new random sample.</returns>
	public uint NextUInt()
	{
		return (uint)NextULongInner();
	}

	/// <summary>
	/// Returns a random <see cref="T:System.UInt64" /> sampled from the uniform distribution with interval [0, ulong.MaxValue],
	/// i.e., over the full range of possible ulong values.
	/// </summary>
	/// <returns>A new random sample.</returns>
	public ulong NextULong()
	{
		return NextULongInner();
	}

	/// <summary>
	/// Returns a random boolean sampled from the uniform discrete distribution {false, true}, i.e., a fair coin flip.
	/// </summary>
	/// <returns>A new random sample.</returns>
	/// <remarks>
	/// Returns a sample the Bernoulli distribution with p = 0.5; also known as a a fair coin flip.
	/// </remarks>
	public bool NextBool()
	{
		return (NextULongInner() & 0x8000000000000000uL) != 0;
	}

	/// <summary>
	/// Returns a random <see cref="T:System.Single" /> sampled from the uniform distribution with interval [0, 1),
	/// i.e., inclusive of 0.0 and exclusive of 1.0.
	/// </summary>
	/// <returns>A new random sample, of type <see cref="T:System.Single" />.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float NextFloat()
	{
		return (float)(NextULongInner() >> 40) * 5.9604645E-08f;
	}

	private int NextInner(int maxValue)
	{
		return (int)(NextDouble() * (double)maxValue);
	}

	private long NextInner(long maxValue)
	{
		return (long)(NextDouble() * (double)maxValue);
	}
}
