using System.Xml;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Message;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTcTests.VerifySetup;
using TcHaxx.ProtocGenTc.TcPlcObjects;

namespace TcHaxx.ProtocGenTcTests.TcPlcObjects;

public class ScalarTypesTest : VerifyBase
{
    private readonly VerifySettings _localSettings;
    private const string TEST_PROTO = "test-scalars.proto";
    private readonly FileDescriptorProto? _sut;

    public ScalarTypesTest() : base()
    {
        _localSettings = VerifyGlobalSettings.GetGlobalSettings();
        _localSettings.AlwaysIncludeMembersWithType<XmlCDataSection>();

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
    public async Task TestTcPouFactoryWithScalarTypesMessages()
    {
        Assert.NotNull(_sut);
        Assert.NotEmpty(_sut.MessageType);
        foreach (var message in _sut.MessageType.GetAllMessages())
        {
            var pou = TcPouFactory.Create(_sut, message, _sut.GetPrefixes());
            await Verify(pou, _localSettings).UseTypeName(message.Name);
        }
    }

    [Fact]
    public async Task TestTcDutFactoryWithScalarTypesMessages()
    {
        Assert.NotNull(_sut);
        Assert.NotEmpty(_sut.MessageType);
        foreach (var message in _sut.MessageType.GetAllMessages())
        {
            var pou = TcDutFactory.CreateStruct(_sut, message, _sut.GetPrefixes());
            await Verify(pou, _localSettings).UseTypeName(message.Name).AutoVerify();
        }
    }

}
