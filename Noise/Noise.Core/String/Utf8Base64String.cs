using System;
using System.Collections;
using System.Text;

namespace Noise.Core.String
{
    public sealed class Utf8Base64String : IEnumerable, IEquatable<Utf8Base64String>, IComparable, IComparable<Utf8Base64String>
    {
        private readonly string _value;

        private Utf8Base64String(string value) => _value = value;

        public static Utf8Base64String FromString(string? value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            var utf8EncodingBytes = Encoding.UTF8.GetBytes(value);
            var base64Utf8EncodingString = Convert.ToBase64String(utf8EncodingBytes);

            return new Utf8Base64String(base64Utf8EncodingString);
        }

        public static Utf8Base64String FromUtf8String(Utf8ValueString? value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            return FromString(value.ToString());
        }

        public int Length => _value.Length;

        public byte[] ToByteArray()
        {
            return Convert.FromBase64String(_value);
        }

        public override string ToString()
        {
            return _value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_value);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Utf8Base64String);
        }

        public bool Equals(Utf8Base64String? other)
        {
            if (other is null) return false;
            return _value == other._value;
        }

        public int CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (obj is not Utf8Base64String other)
                throw new ArgumentException("Object is not an instance of the Utf8Base64String.");

            return CompareTo(other);
        }

        public int CompareTo(Utf8Base64String? other)
        {
            if (other is null) return 1;
            return _value.CompareTo(other._value);
        }

        public IEnumerator GetEnumerator()
        {
            return _value.GetEnumerator();
        }
    }
}
