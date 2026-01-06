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

public class ComplexTests : VerifyBase
{
    private readonly VerifySettings _localSettings;
    private const string TEST_PROTO = "test-complex.proto";
    private readonly FileDescriptorProto? _sut;

    public ComplexTests() : base()
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
    public async Task TestTcPouFactoryWithComplexMessages()
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
    public async Task TestTcDutFactoryStructWithComplexMessages()
    {
        Assert.NotNull(_sut);
        Assert.NotEmpty(_sut.MessageType);
        foreach (var message in _sut.MessageType.GetAllMessages())
        {
            var pou = TcDutFactory.CreateStruct(_sut, message, _sut.GetPrefixes());
            await Verify(pou, _localSettings).UseTypeName(message.Name);
        }
    }

    [Fact]
    public async Task TestTcDutFactoryEnumWithComplexMessages()
    {
        Assert.NotNull(_sut);
        Assert.NotEmpty(_sut.MessageType);
        foreach (var enumType in _sut.EnumType.Concat(_sut.MessageType.GetAllNestedEnums()))
        {
            var pou = TcDutFactory.CreateEnum(_sut, enumType, _sut.GetPrefixes());
            await Verify(pou, _localSettings).UseTypeName(enumType.Name);
        }
    }
}
