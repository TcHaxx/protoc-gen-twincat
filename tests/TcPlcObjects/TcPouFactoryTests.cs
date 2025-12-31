using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTcTests.VerifySetup;
using Test.Prefix;
using TcHaxx.ProtocGenTc.TcPlcObjects;

namespace TcHaxx.ProtocGenTcTests.TcPlcObjects;

public class TcPouFactoryTests : VerifyBase
{
    private readonly VerifySettings _localSettings;
    private const string TEST_PROTO = "test-tcpoufactory.proto";
    private readonly FileDescriptorProto? _sut;

    public TcPouFactoryTests() : base()
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
    public async Task ShouldCreateStructWithCorrectNameAndId()
    {
        Assert.NotNull(_sut);
        var md = _sut.MessageType.Single(m => m.Name == nameof(TestSimpleMessage));
        Assert.NotNull(md);
        var msgComment = CommentsProvider.GetComments(_sut, md);

        var pou = TcPouFactory.Create(md, msgComment, _sut.GetPrefixes());

        await Verify(pou, _localSettings);
    }
}
