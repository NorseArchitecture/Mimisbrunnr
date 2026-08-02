using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Norse.Reference.Data.Primitives.Generator;

/// <summary>
/// Hand-rolled RFC 9562 version 5 UUID derivation: SHA-1 over the namespace GUID's bytes in true
/// RFC 4122 field order plus the UTF-8 name bytes, then version/variant bits set on the digest's
/// first 16 bytes. netstandard2.0-clean (<see cref="SHA1.Create()"/>, no span-based
/// <c>SHA1.HashData</c>; <c>Guid(byte[])</c>, no <c>Guid.TryWriteBytes(bigEndian:)</c> or
/// <c>new Guid(bytes, bigEndian:)</c> — none of those overloads exist on netstandard2.0) — a
/// generator runs inside the compiler process and cannot take a runtime dependency on
/// <c>Norse.Primitives.Identifiers.DeterministicGuid</c> (net11.0), so this must bit-for-bit
/// reproduce that type's algorithm by hand. Task 8's self-verification tests
/// (<c>Reference.Contracts.Tests</c>) recompute every emitted identifier independently via the
/// real <c>DeterministicGuid</c> at runtime to prove the two never drift apart (spec §6).
/// </summary>
static class Uuid5
{
	/// <summary>Derives a version 5 UUID from <paramref name="namespaceId"/> and <paramref name="name"/>.</summary>
	internal static Guid Compute(Guid namespaceId, string name)
	{
		var namespaceBytes = SwapRfcFieldOrder(namespaceId.ToByteArray());
		var nameBytes = Encoding.UTF8.GetBytes(name);

		var buffer = new byte[namespaceBytes.Length + nameBytes.Length];
		Buffer.BlockCopy(namespaceBytes, 0, buffer, 0, namespaceBytes.Length);
		Buffer.BlockCopy(nameBytes, 0, buffer, namespaceBytes.Length, nameBytes.Length);

		var digest = HashAndSetVersionBits(buffer);

		var head = new byte[16];
		Buffer.BlockCopy(digest, 0, head, 0, 16);

		// `head` carries the SHA-1 digest's raw bytes, which the RFC positions directly as the
		// big-endian/RFC-order field layout (time_low/time_mid/time_hi_and_version/clock_seq/node)
		// -- the same bytes `new Guid(head, bigEndian: true)` would consume on net11.0. Swapping
		// back to .NET's internal little-endian-first-three-fields layout lets the plain
		// `Guid(byte[])` constructor (netstandard2.0-available) build the correct value.
		return new Guid(SwapRfcFieldOrder(head));
	}

	[SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms",
		Justification = "RFC 9562 §A.4 mandates SHA-1 for UUIDv5 name-based identifiers; this is a specification requirement, not a security primitive.")]
	static byte[] HashAndSetVersionBits(byte[] input)
	{
		byte[] digest;
		using (var sha1 = SHA1.Create())
			digest = sha1.ComputeHash(input);

		digest[6] = (byte)((digest[6] & 0x0F) | (5 << 4));
		digest[8] = (byte)((digest[8] & 0x3F) | 0x80);
		return digest;
	}

	/// <summary>
	/// Swaps a 16-byte GUID between .NET's internal layout (<c>Data1</c>/<c>Data2</c>/<c>Data3</c>
	/// stored little-endian; <c>Data4</c>'s trailing 8 bytes already in the correct order either
	/// way) and the true RFC 4122/9562 big-endian field order. The transform is its own inverse —
	/// reversing the same three sub-ranges (4 bytes / 2 bytes / 2 bytes, leaving the trailing 8
	/// bytes untouched) converts in either direction, mirroring what
	/// <c>Guid.TryWriteBytes(bigEndian: true, ...)</c> and <c>new Guid(bytes, bigEndian: true)</c>
	/// do on net11.0 — neither of which is available on this project's netstandard2.0 target.
	/// </summary>
	static byte[] SwapRfcFieldOrder(byte[] guidBytes)
	{
		var result = (byte[])guidBytes.Clone();
		Array.Reverse(result, 0, 4);
		Array.Reverse(result, 4, 2);
		Array.Reverse(result, 6, 2);
		return result;
	}
}
