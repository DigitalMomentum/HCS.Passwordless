using HCS.Umbraco.Passwordless.Security;

namespace HCS.Umbraco.Passwordless.Tests.Common;

public class ConstantTimeTests
{
    [Fact]
    public void ByteEquals_ReturnsTrueForIdenticalArrays()
        => ConstantTime.Equals(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 }).Should().BeTrue();

    [Fact]
    public void ByteEquals_ReturnsTrueForEmptyArrays()
        => ConstantTime.Equals(Array.Empty<byte>(), Array.Empty<byte>()).Should().BeTrue();

    [Fact]
    public void ByteEquals_ReturnsFalseForDifferentValues()
        => ConstantTime.Equals(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 4 }).Should().BeFalse();

    [Fact]
    public void ByteEquals_ReturnsFalseForDifferentLengths()
        => ConstantTime.Equals(new byte[] { 1, 2 }, new byte[] { 1, 2, 3 }).Should().BeFalse();

    [Fact]
    public void StringEquals_ReturnsTrueForIdenticalStrings()
        => ConstantTime.Equals("hello", "hello").Should().BeTrue();

    [Fact]
    public void StringEquals_ReturnsTrueForEmptyStrings()
        => ConstantTime.Equals(string.Empty, string.Empty).Should().BeTrue();

    [Fact]
    public void StringEquals_ReturnsFalseForDifferentStrings()
        => ConstantTime.Equals("hello", "world").Should().BeFalse();

    [Fact]
    public void StringEquals_IsCaseSensitive()
        => ConstantTime.Equals("Hello", "hello").Should().BeFalse();

    [Fact]
    public void StringEquals_ReturnsFalseForDifferentLengths()
        => ConstantTime.Equals("ab", "abc").Should().BeFalse();
}
