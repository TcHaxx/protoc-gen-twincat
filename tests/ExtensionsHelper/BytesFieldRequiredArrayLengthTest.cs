using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using Test.Extension.Helper;
using static Test.Extensions.v1.ExtensionExtensions;

namespace TcHaxx.ProtocGenTcTests.ExtensionsHelper;

public class BytesFieldRequiredArrayLengthTest
{
    private const string TEST_PROTO = "test-extensions-helper.proto";
    private readonly FileDescriptorProto? _sut;

    public BytesFieldRequiredArrayLengthTest()
    {
        var extensionRegistry = new ExtensionRegistry
     {
         ArrayLength
     };

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
    public void ThrowsIfNoArrayLengthFieldOption()
    {
        Assert.NotNull(_sut);

        var md = _sut.MessageType.Single(m => m.Name == nameof(MessageTestFieldBytes));
        Assert.NotNull(md);

        var sut = md.Field.Single(f => f.Name == "invalid_throws");

        Assert.Throws<InvalidOperationException>(() => sut.GetArrayLengthWhenBytesFieldOrFail(out _));
    }

    [Theory]
    [InlineData("valid_len123", 122)]
    [InlineData("valid_len0", 0)]
    [InlineData("valid_len1", 0)]
    public void ShouldReturnArrayLength(string fieldName, uint expected)
    {
        Assert.NotNull(_sut);

        var md = _sut.MessageType.Single(m => m.Name == nameof(MessageTestFieldBytes));
        Assert.NotNull(md);

        var sut = md.Field.Single(f => f.Name == fieldName);

        sut.GetArrayLengthWhenBytesFieldOrFail(out var actual);
        Assert.Equal(expected, actual);
    }
}
