using Noise.Core.String;
using System;
using System.Text;
using Xunit;

namespace Noise.Core.Test.String
{
    public class Utf8ValueStringTests
    {
        [Fact]
        public void Should_Create_From_String()
        {
            var value = "Hello World!";

            _ = Utf8ValueString.FromString(value);
        }

        [Fact]
        public void Should_Create_From_Utf8Base64String()
        {
            var value = "Hello World!";
            var base64 = Utf8Base64String.FromString(value);

            _ = Utf8ValueString.FromBase64String(base64);
        }

        [Fact]
        public void Should_Tell_The_Value_Length()
        {
            var value = "Hello World!";
            var utf8 = Utf8ValueString.FromString(value);

            var expected = Encoding.UTF8.GetString(Encoding.Default.GetBytes(value)).Length;
            var actual = utf8.Length;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_Return_Correct_Value_On_ToString_From_String()
        {
            var value = "Hello World!";
            var utf8 = Utf8ValueString.FromString(value);

            var expectedToEqual = Encoding.UTF8.GetString(Encoding.Default.GetBytes(value));
            var expectedToNotEqual = Encoding.UTF32.GetString(Encoding.Default.GetBytes(value));
            var actual = utf8.ToString();

            Assert.Equal(expectedToEqual, actual);
            Assert.NotEqual(expectedToNotEqual, actual);
        }

        [Fact]
        public void Should_Return_Correct_Value_On_ToByteArray_From_String()
        {
            var value = "Hello World!";
            var utf8 = Utf8ValueString.FromString(value);

            var expectedToEqual = Encoding.UTF8.GetBytes(value);
            var expectedToNotEqual = Encoding.UTF32.GetBytes(value);
            var actual = utf8.ToByteArray();

            Assert.Equal(expectedToEqual, actual);
            Assert.NotEqual(expectedToNotEqual, actual);
        }

        [Fact]
        public void Should_Return_Correct_Value_On_ToString_From_Base64()
        {
            var value = "Hello World!";
            var base64 = Utf8Base64String.FromString(value);
            var utf8 = Utf8ValueString.FromBase64String(base64);

            var expectedToEqual = Encoding.UTF8.GetString(Encoding.Default.GetBytes(value));
            var expectedToNotEqual = Encoding.UTF32.GetString(Encoding.Default.GetBytes(value));
            var actual = utf8.ToString();

            Assert.Equal(expectedToEqual, actual);
            Assert.NotEqual(expectedToNotEqual, actual);
        }

        [Fact]
        public void Should_Return_Correct_Value_On_ToByteArray_From_Base64()
        {
            var value = "Hello World!";
            var base64 = Utf8Base64String.FromString(value);
            var utf8 = Utf8ValueString.FromBase64String(base64);

            var expectedToEqual = Encoding.UTF8.GetBytes(value);
            var expectedToNotEqual = Encoding.UTF32.GetBytes(value);
            var actual = utf8.ToByteArray();

            Assert.Equal(expectedToEqual, actual);
            Assert.NotEqual(expectedToNotEqual, actual);
        }

        [Fact]
        public void Should_Tell_If_Two_Instances_Are_Equal()
        {
            var value = "Hello World!";
            var firstUtf8 = Utf8ValueString.FromString(value);
            var secondUtf8 = Utf8ValueString.FromString(value);

            Assert.Equal(firstUtf8, secondUtf8);
        }

        [Fact]
        public void Should_Tell_If_Two_Instances_Are_Not_Equal()
        {
            var firstValue = "Hello World!";
            var firstUtf8 = Utf8ValueString.FromString(firstValue);

            var secondValue = "Hello other World!";
            var secondUtf8 = Utf8ValueString.FromString(secondValue);

            Assert.NotEqual(firstUtf8, secondUtf8);
        }
    }
}
