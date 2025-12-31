using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTcTests.VerifySetup;

namespace TcHaxx.ProtocGenTcTests.Prefix;

public class PrefixFactoryTests : VerifyBase
{
    private readonly VerifySettings _localSettings;
    private const string TEST_PROTO = "test-prefix.proto";
    private readonly FileDescriptorProto? _sut;

    public PrefixFactoryTests() : base()
    {
        _localSettings = VerifyGlobalSettings.GetGlobalSettings();

        var extensionRegistry = ExtensionRegistryBuilder.Build();

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
    public async Task ShouldGetAllPrefixes()
    {
        Assert.NotNull(_sut);
        await Verify(_sut.GetPrefixes(), _localSettings);
    }
}
