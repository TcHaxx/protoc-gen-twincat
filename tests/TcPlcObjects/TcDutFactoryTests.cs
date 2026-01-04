using System.Xml;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTcTests.VerifySetup;
using TcHaxx.ProtocGenTc.TcPlcObjects;
using Test.Prefix.Dut;

namespace TcHaxx.ProtocGenTcTests.TcPlcObjects;

public class TcDutFactoryTests : VerifyBase
{
    private readonly VerifySettings _localSettings;
    private const string TEST_PROTO = "test-tcdutfactory.proto";
    private readonly FileDescriptorProto? _sut;

    public TcDutFactoryTests() : base()
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
    public async Task ShouldCreateStructWithCorrectNameAndId()
    {
        Assert.NotNull(_sut);
        var md = _sut.MessageType.Single(m => m.Name == nameof(TestSimpleMessageDut));
        Assert.NotNull(md);
        var dut = TcDutFactory.CreateStruct(_sut, md, _sut.GetPrefixes());
        await Verify(dut, _localSettings);
    }

    [Fact]
    public async Task ShouldCreateStructWithCorrectNameAndIdFromNestedMessage()
    {
        Assert.NotNull(_sut);
        var md = _sut.MessageType.Single(m => m.Name == nameof(TestNestedMessageDut));
        Assert.NotNull(md);
        var dut = TcDutFactory.CreateStruct(_sut, md, _sut.GetPrefixes());
        await Verify(dut, _localSettings);
    }

    [Fact]
    public async Task ShouldCreateEnumWithCorrectNameAndId()
    {
        Assert.NotNull(_sut);
        var ed = _sut.EnumType.Single(m => m.Name == nameof(TestEnum));
        Assert.NotNull(ed);
        var dut = TcDutFactory.CreateEnum(_sut, ed, _sut.GetPrefixes());

        await Verify(dut, _localSettings);
    }
}
