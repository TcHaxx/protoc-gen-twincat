using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Fields;
using TcHaxx.ProtocGenTcTests.VerifySetup;
using Test.Bool;

namespace TcHaxx.ProtocGenTcTests.FieldProviders;

public class BooleanFieldProviderTests : VerifyBase
{
    private readonly VerifySettings _localSettings;
    private const string TEST_PROTO = "test-bool.proto";
    private readonly FileDescriptorProto? _sut;

    public BooleanFieldProviderTests() : base()
    {
        _localSettings = VerifyGlobalSettings.GetGlobalSettings();

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
    public void ThrowsIfNoArrayLengthFieldOption()
    {
        Assert.NotNull(_sut);

        var md = _sut.MessageType.Single(m => m.Name == nameof(TestBoolInvalid));
        Assert.NotNull(md);

        var sut = md.Field.Single(f => f.Name == "repeated_flags_no_attribute");

        Assert.Throws<InvalidOperationException>(() => sut.GetArrayLengthWhenBytesFieldOrFail(out _));
    }

    [Theory]
    [InlineData("flag")]
    [InlineData("repeated_flags_attribute")]
    public async Task ShouldSuccessfullyProcessField(string field)
    {
        Assert.NotNull(_sut);
        var md = _sut.MessageType.Single(m => m.Name == nameof(TestBoolValid));
        Assert.NotNull(md);
        var sut = md.Field.Single(f => f.Name == field);
        var msgComment = CommentsProvider.GetComments(_sut, md, sut);

        var actual = BooleanFieldProvider.ProcessField(sut, msgComment);

        await Verify(actual, _localSettings).UseTextForParameters(field);
    }
}
