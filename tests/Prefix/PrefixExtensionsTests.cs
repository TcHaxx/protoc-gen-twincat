using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Prefix;
using Test.Prefix;

namespace TcHaxx.ProtocGenTcTests.Prefix;

public class PrefixExtensionsTests : VerifyBase
{

    private const string TEST_PROTO = "test-prefix.proto";
    private readonly FileDescriptorProto? _sut;

    public PrefixExtensionsTests() : base()
    {
        var extensionRegistry = ExtensionRegistryBuilder.Build();

        var descriptorSet = FileDescriptorSet.Parser.WithExtensionRegistry(extensionRegistry).ParseFrom(File.ReadAllBytes($"./.protobufs/{TEST_PROTO}.pb"));

        var request = new CodeGeneratorRequest
        {
            FileToGenerate = { TEST_PROTO },
            ProtoFile = { descriptorSet.File }
        };
        var codeGenRequest = CodeGeneratorRequest.Parser
            .WithExtensionRegistry(extensionRegistry)
            .ParseFrom(request.ToByteString());
        _sut = codeGenRequest.ProtoFile.Single(f => f.Name.EndsWith(TEST_PROTO));
    }

    [Fact]
    public void ShouldGetPrefixForFb()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var msg = _sut.MessageType.First(x => x.Name == nameof(TestMessagePrefix));
        Assert.True(prefixes.TryGetPrefixForFb(msg, out var prefix));
        Assert.Equal("FB_MSG_", prefix);
    }

    [Fact]
    public void ShouldGetPrefixForSt()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var msg = _sut.MessageType.First(x => x.Name == nameof(TestMessagePrefix));
        Assert.True(prefixes.TryGetPrefixForSt(msg, out var prefix));
        Assert.Equal("ST_MSG_", prefix);
    }

    [Fact]
    public void ShouldGetEnumPrefix()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var @enum = _sut.EnumType.First(x => x.Name == nameof(TestEnumPrefix));
        Assert.True(prefixes.TryGetEnumPrefix(@enum, out var prefix));
        Assert.Equal("E_ENUM_", prefix);
    }

    [Fact]
    public void ShouldGetGlobalPrefixForFb()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var msg = _sut.MessageType.First(x => x.Name == nameof(TestMessageNoPrefix));
        Assert.True(prefixes.TryGetPrefixForFb(msg, out var prefix));
        Assert.Equal("FB_FILE_", prefix);
    }

    [Fact]
    public void ShouldGetGlobalPrefixForSt()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var msg = _sut.MessageType.First(x => x.Name == nameof(TestMessageNoPrefix));
        Assert.True(prefixes.TryGetPrefixForSt(msg, out var prefix));
        Assert.Equal("ST_FILE_", prefix);
    }
    [Fact]
    public void ShouldGetGlobalPrefixedFbName()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var msg = _sut.MessageType.First(x => x.Name == nameof(TestMessageNoPrefix));
        Assert.Equal($"FB_FILE_{nameof(TestMessageNoPrefix)}", prefixes.GetFbNameWithPrefix(msg));
    }

    [Fact]
    public void ShouldGetPrefixedFbName()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var msg = _sut.MessageType.First(x => x.Name == nameof(TestMessagePrefix));
        Assert.Equal($"FB_MSG_{nameof(TestMessagePrefix)}", prefixes.GetFbNameWithPrefix(msg));
    }

    [Fact]
    public void ShouldGetGlobalPrefixedStName()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var msg = _sut.MessageType.First(x => x.Name == nameof(TestMessageNoPrefix));
        Assert.Equal($"ST_FILE_{nameof(TestMessageNoPrefix)}", prefixes.GetStNameWithPrefix(msg));
    }

    [Fact]
    public void ShouldGetPrefixedStName()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var msg = _sut.MessageType.First(x => x.Name == nameof(TestMessagePrefix));
        Assert.Equal($"ST_MSG_{nameof(TestMessagePrefix)}", prefixes.GetStNameWithPrefix(msg));
    }

    [Fact]
    public void ShouldGetGlobalEnumPrefix()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var @enum = _sut.EnumType.First(x => x.Name == nameof(TestEnumNoPrefix));
        Assert.True(prefixes.TryGetEnumPrefix(@enum, out var prefix));
        Assert.Equal("E_FILE_", prefix);
    }

    [Fact]
    public void ShouldGetGlobalPrefixedEnumName()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var @enum = _sut.EnumType.First(x => x.Name == nameof(TestEnumNoPrefix));
        Assert.Equal($"E_FILE_{nameof(TestEnumNoPrefix)}", prefixes.GetEnumNameWithPrefix(@enum));
    }

    [Fact]
    public void ShouldGetPrefixedEnumName()
    {
        Assert.NotNull(_sut);
        var prefixes = _sut.GetPrefixes();
        var @enum = _sut.EnumType.First(x => x.Name == nameof(TestEnumPrefix));
        Assert.Equal($"E_ENUM_{nameof(TestEnumPrefix)}", prefixes.GetEnumNameWithPrefix(@enum));
    }
}
