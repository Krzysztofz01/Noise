using System;
using System.Collections;
using System.Text;

namespace Noise.Core.String
{
    public sealed class Utf8ValueString : IEnumerable, IEquatable<Utf8ValueString>, IComparable, IComparable<Utf8ValueString>
    {
        private readonly string _value;

        private Utf8ValueString(string value) => _value = value;

        public static Utf8ValueString FromString(string? value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            var defaultEncodingBytes = Encoding.Default.GetBytes(value);
            var utf8EncodingString = Encoding.UTF8.GetString(defaultEncodingBytes);

            return new Utf8ValueString(utf8EncodingString);
        }

        public static Utf8ValueString FromBase64String(Utf8Base64String? value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            var decodedBase64ToUtf8 = Convert.FromBase64String(value.ToString());
            var utf8EncodedString = Encoding.UTF8.GetString(decodedBase64ToUtf8);

            return new Utf8ValueString(utf8EncodedString);
        }

        public int Length => _value.Length;

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(_value);
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
            return Equals(obj as Utf8ValueString);
        }

        public bool Equals(Utf8ValueString? other)
        {
            if (other is null) return false;
            return _value == other._value;
        }

        public int CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (obj is not Utf8ValueString other)
                throw new ArgumentException("Object is not an instance of the Utf8ValueString.");

            return CompareTo(other);
        }

        public int CompareTo(Utf8ValueString? other)
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
