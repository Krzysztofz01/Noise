using Noise.Core.String;
using System;
using System.Text;
using Xunit;

namespace Noise.Core.Test.String
{
    public class Utf8Base64StringTests
    {
        [Fact]
        public void Should_Create_From_String()
        {
            var value = "Hello World!";

            _ = Utf8Base64String.FromString(value);
        }

        [Fact]
        public void Should_Create_From_Utf8Base64String()
        {
            var value = "Hello World!";
            var utf8 = Utf8ValueString.FromString(value);

            _ = Utf8Base64String.FromUtf8String(utf8);
        }

        [Fact]
        public void Should_Tell_The_Value_Length()
        {
            var value = "Hello World!";
            var base64 = Utf8Base64String.FromString(value);

            var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).Length;
            var actual = base64.Length;

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Should_Return_Correct_Value_On_ToString_From_String()
        {
            var value = "Hello World!";
            var base64 = Utf8Base64String.FromString(value);

            var expectedToEqual = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            var expectedToNotEqual = Convert.ToBase64String(Encoding.UTF32.GetBytes(value));
            var actual = base64.ToString();

            Assert.Equal(expectedToEqual, actual);
            Assert.NotEqual(expectedToNotEqual, actual);
        }

        [Fact]
        public void Should_Return_Correct_Value_On_ToByteArray_From_String()
        {
            var value = "Hello World!";
            var base64 = Utf8Base64String.FromString(value);

            var expectedToEqual = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
            var expectedToNotEqual = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF32.GetBytes(value)));
            var actual = base64.ToByteArray();

            Assert.Equal(expectedToEqual, actual);
            Assert.NotEqual(expectedToNotEqual, actual);
        }

        [Fact]
        public void Should_Return_Correct_Value_On_ToString_From_Utf8()
        {
            var value = "Hello World!";
            var utf8 = Utf8ValueString.FromString(value);
            var base64 = Utf8Base64String.FromUtf8String(utf8);

            var expectedToEqual = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            var expectedToNotEqual = Convert.ToBase64String(Encoding.UTF32.GetBytes(value));
            var actual = base64.ToString();

            Assert.Equal(expectedToEqual, actual);
            Assert.NotEqual(expectedToNotEqual, actual);
        }

        [Fact]
        public void Should_Return_Correct_Value_On_ToByteArray_From_Base64()
        {
            var value = "Hello World!";
            var utf8 = Utf8ValueString.FromString(value);
            var base64 = Utf8Base64String.FromUtf8String(utf8);

            var expectedToEqual = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
            var expectedToNotEqual = Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF32.GetBytes(value)));
            var actual = base64.ToByteArray();

            Assert.Equal(expectedToEqual, actual);
            Assert.NotEqual(expectedToNotEqual, actual);
        }

        [Fact]
        public void Should_Tell_If_Two_Instances_Are_Equal()
        {
            var value = "Hello World!";
            var firstUtf8 = Utf8Base64String.FromString(value);
            var secondUtf8 = Utf8Base64String.FromString(value);

            Assert.Equal(firstUtf8, secondUtf8);
        }

        [Fact]
        public void Should_Tell_If_Two_Instances_Are_Not_Equal()
        {
            var firstValue = "Hello World!";
            var firstUtf8 = Utf8Base64String.FromString(firstValue);

            var secondValue = "Hello other World!";
            var secondUtf8 = Utf8Base64String.FromString(secondValue);

            Assert.NotEqual(firstUtf8, secondUtf8);
        }
    }
}
