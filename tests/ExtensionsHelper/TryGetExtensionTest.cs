using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.Extensions.v1;
using TcHaxx.ProtocGenTc;
using Test.Extension.Helper;
using static TcHaxx.Extensions.v1.TchaxxExtensionsExtensions;

namespace TcHaxx.ProtocGenTcTests.ExtensionsHelper;

public class TryGetExtensionTest
{
    private const string TEST_PROTO = "test-extensions-helper.proto";
    private readonly FileDescriptorProto? _sut;

    public TryGetExtensionTest()
    {
        var extensionRegistry = new ExtensionRegistry
        {
            AttributePackMode,
        };

        var descriptorSet = FileDescriptorSet.Parser.WithExtensionRegistry(extensionRegistry).ParseFrom(File.ReadAllBytes($"./.protobufs/proto/{TEST_PROTO}.pb"));

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
    public void ShouldRetrieveCorrectExtensionValue()
    {
        Assert.NotNull(_sut);

        var md = _sut.MessageType.Single(m => m.Name == nameof(MessageWithExtension));
        Assert.NotNull(md);

        Assert.True(md.Options.TryGetExtension(AttributePackMode, out var actualPackMode));
        Assert.Equal(EnumPackMode.EpmFourBytesAligned, actualPackMode);
    }

    [Fact]
    public void ShouldNotRetrieveAnyExtensionValue()
    {
        Assert.NotNull(_sut);

        var md = _sut.MessageType.Single(m => m.Name == nameof(MessageWithoutExtension));
        Assert.NotNull(md);

        Assert.False(md.Options.TryGetExtension(AttributePackMode, out var _));
    }
}
