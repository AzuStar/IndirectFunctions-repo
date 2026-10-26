using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace IndirectFunctions;

/// <summary>A durable 128-bit callback identity. Canonical bytes use network byte order.</summary>
public readonly struct NCodeId : IEquatable<NCodeId>, IComparable<NCodeId>
{
    public const int ByteLength = 16;
    private static readonly byte[] Domain = Encoding.ASCII.GetBytes("IndirectFunctions.CodeId.v1\0");
    private static readonly Encoding KeyEncoding = new UTF8Encoding(false, true);
    private readonly ulong _high;
    private readonly ulong _low;

    private NCodeId(ulong high, ulong low) => (_high, _low) = (high, low);

    /// <summary>First 16 SHA-256 bytes of ASCII domain "IndirectFunctions.CodeId.v1\0" followed by exact UTF-8 key bytes.</summary>
    public static NCodeId FromKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var keyBytes = KeyEncoding.GetBytes(key);
        var input = new byte[Domain.Length + keyBytes.Length];
        Domain.CopyTo(input, 0);
        keyBytes.CopyTo(input, Domain.Length);
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(input, hash);
        return FromBytes(hash[..ByteLength]);
    }

    public static NCodeId FromBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != ByteLength)
            throw new ArgumentException("A code identity must contain exactly 16 bytes.", nameof(bytes));
        return new(BinaryPrimitives.ReadUInt64BigEndian(bytes), BinaryPrimitives.ReadUInt64BigEndian(bytes[8..]));
    }

    public byte[] ToBytes()
    {
        var bytes = new byte[ByteLength];
        WriteBytes(bytes);
        return bytes;
    }

    public void WriteBytes(Span<byte> destination)
    {
        if (destination.Length < ByteLength)
            throw new ArgumentException("Destination must contain at least 16 bytes.", nameof(destination));
        BinaryPrimitives.WriteUInt64BigEndian(destination, _high);
        BinaryPrimitives.WriteUInt64BigEndian(destination[8..], _low);
    }

    public bool Equals(NCodeId other) => _high == other._high && _low == other._low;
    public override bool Equals(object? obj) => obj is NCodeId other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_high, _low);
    public int CompareTo(NCodeId other) => _high != other._high ? _high.CompareTo(other._high) : _low.CompareTo(other._low);
    public override string ToString() => $"{_high:x16}{_low:x16}";
    public static bool operator ==(NCodeId left, NCodeId right) => left.Equals(right);
    public static bool operator !=(NCodeId left, NCodeId right) => !left.Equals(right);
}
