using Arc4u.Serializer;
using FluentAssertions;
using System;
using System.Text;
using Xunit;

namespace Arc4u.Standard.UnitTest.Serialization.Json;

public class JsonSerializationTests
{
    [Fact]
    public void SerializingTimeSpan_ShouldReturn_UTF8EncodedTicks()
    {
        // Arrange
        const long ticks = 123456789;
        var timeSpan = new TimeSpan(ticks);
        var serializer = new JsonSerialization();

        // Act
        var serializedTimeSpan = serializer.Serialize(timeSpan);

        // Assert
        serializedTimeSpan.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(ticks.ToString()));
    }

    [Fact]
    public void SerializingNullTimeSpan_ShouldReturn_StringEqualsTonull()
    {
        // Arrange
        TimeSpan? nullTimeSpan = null;
        var serializer = new JsonSerialization();

        // Act
        var serializedNullTimeSpan = serializer.Serialize(nullTimeSpan);

        // Assert
        serializedNullTimeSpan.Should().Equal(Encoding.UTF8.GetBytes("null"));
    }

    [Fact]
    public void DeserializingNumberIntoTimeSpan_ShouldReturn_ATimeSpanWithItsTicksEqualToTheInputNumber()
    {
        // Arrange
        const string json = "123456789";
        var ticks = long.Parse(json);
        var serializer = new JsonSerialization();

        // Act
        var deserializedJsonAsTimeSpanWithGenericImpl = serializer.Deserialize<TimeSpan>(Encoding.UTF8.GetBytes(json));
        var deserializedJsonAsTimeSpanWithoutGenericImpl = (TimeSpan) serializer.Deserialize(Encoding.UTF8.GetBytes(json), typeof(TimeSpan));

        // Assert
        deserializedJsonAsTimeSpanWithGenericImpl.As<object>().Should().BeAssignableTo<TimeSpan>();
        deserializedJsonAsTimeSpanWithGenericImpl.Ticks.Should().Be(ticks);

        deserializedJsonAsTimeSpanWithoutGenericImpl.As<object>().Should().BeAssignableTo<TimeSpan>();
        deserializedJsonAsTimeSpanWithoutGenericImpl.Ticks.Should().Be(ticks);
    }

    [Fact]
    public void DeserializingnullStringValueToNullableTimeSpan_ShouldReturn_Null()
    {
        // Arrange
        byte[] encodedJson = Encoding.UTF8.GetBytes("null");
        var serializer = new JsonSerialization();

        // Act
        var timeSpan1 = serializer.Deserialize<TimeSpan?>(encodedJson);
        var timeSpan2 = serializer.Deserialize(encodedJson, typeof(TimeSpan?));

        // Assert
        timeSpan1.Should().BeNull();
        timeSpan2.Should().BeNull();
    }

    [Fact]
    public void DeserializingNullStringToTimeSpan_ShouldThrow()
    {
        // Arrange
        byte[] encodedJson = Encoding.UTF8.GetBytes("null");
        var serializer = new JsonSerialization();

        // Act
        var deserializeWithGeneric = () => serializer.Deserialize<TimeSpan>(encodedJson);
        var deseserializeWithoutGeneric = () => (TimeSpan) serializer.Deserialize(encodedJson, typeof(TimeSpan));

        // Assert
        deserializeWithGeneric.Should().Throw<Exception>();
        deseserializeWithoutGeneric.Should().Throw<Exception>();
    }

    [Fact]
    public void SerializingAndDeserializingAnObject_ShouldReturn_AnObjectWithSamePropertiesValuesThanTheOriginal()
    {
        // Arrange
        var serializer = new JsonSerialization();
        var operatingSystem = Environment.OSVersion;

        // Act
        var serializedObject = serializer.Serialize(operatingSystem);
        var deserializedOperatingSystem = serializer.Deserialize<OperatingSystem>(serializedObject);

        // Assert
        deserializedOperatingSystem.ServicePack.Should().Be(operatingSystem.ServicePack);
        deserializedOperatingSystem.Platform.Should().Be(operatingSystem.Platform);
        deserializedOperatingSystem.Version.Should().Be(operatingSystem.Version);
        deserializedOperatingSystem.VersionString.Should().Be(operatingSystem.VersionString);
    }

    [Fact]
    public void DeserializingAStringIntoAnIncorrectObject_ShouldThrow()
    {
        // Arrange
        var encodedJson = Encoding.UTF8.GetBytes("HELLO WORLD $");
        var serializer = new JsonSerialization();

        // Act
        var deserializationWithGenericAction = () => serializer.Deserialize<OperatingSystem>(encodedJson);
        var deserializationWithoutGenericAction = () => (OperatingSystem) serializer.Deserialize(encodedJson, typeof(OperatingSystem));

        // Assert
        deserializationWithGenericAction.Should().Throw<Exception>();
        deserializationWithoutGenericAction.Should().Throw<Exception>();
    }
}
