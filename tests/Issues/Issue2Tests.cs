using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using Issue2;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTc.TcPlcObjects;
using TcHaxx.ProtocGenTcTests.VerifySetup;

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
    public async Task ShoulCreatePou()
    {
        Assert.NotNull(_sut);
        var md = _sut.MessageType.Single(m => m.Name == nameof(ExtendedStruct));
        Assert.NotNull(md);
        var pou = TcPouFactory.Create(_sut, md, _sut.GetPrefixes());
        await Verify(pou, _localSettings);
    }

    [Fact]
    public async Task ShouldCreateDut()
    {
        Assert.NotNull(_sut);
        var md = _sut.MessageType.Single(m => m.Name == nameof(ExtendedStruct));
        Assert.NotNull(md);
        var dut = TcDutFactory.CreateStruct(_sut, md, _sut.GetPrefixes());
        await Verify(dut, _localSettings);
    }
}
