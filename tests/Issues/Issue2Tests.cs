using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTc.TcPlcObjects;
using TcHaxx.ProtocGenTcTests.VerifySetup;
using Issue2;

namespace TcHaxx.ProtocGenTcTests.Issues;

public class Issue2Tests : VerifyBase
{
    private readonly VerifySettings _localSettings;
    private const string TEST_PROTO = "issues/issue#2.proto";
    private readonly FileDescriptorProto? _sut;

    public Issue2Tests() : base()
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
        var md = _sut.MessageType.Single(m => m.Name == nameof(ExtendedStruct));
        Assert.NotNull(md);
        var pou = TcPouFactory.Create(_sut, md, _sut.GetPrefixes());
        await Verify(pou, _localSettings);
    }
}
