namespace TournamentManager.Tests.RoundRobin;

internal struct CustomTestStruct : IEquatable<CustomTestStruct>
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }

    public readonly bool Equals(CustomTestStruct other)
    {
        return A == other.A && B == other.B && C == other.C;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is CustomTestStruct other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(A, B, C);
    }

    public static bool operator ==(CustomTestStruct left, CustomTestStruct right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CustomTestStruct left, CustomTestStruct right)
    {
        return !left.Equals(right);
    }

    public override readonly string ToString()
    {
        return $"{A}-{B}-{C}";
    }
}
