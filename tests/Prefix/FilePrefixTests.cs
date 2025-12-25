using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
//using TcHaxx.ProtocGenTcTests.VerifySetup;
using static TcHaxx.Extensions.v1.TchaxxExtensionsExtensions;
using Test.Prefix;

namespace TcHaxx.ProtocGenTcTests.Prefix;

public class FilePrefixTests : VerifyBase
{
    //private readonly VerifySettings _localSettings;
    private const string TEST_PROTO = "test-prefix.proto";
    private readonly FileDescriptorProto? _sut;

    public FilePrefixTests() : base()
    {
        //_localSettings = VerifyGlobalSettings.GetGlobalSettings();

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
    public async Task ShouldGenerateCorrectFilePrefix()
    {
        Assert.NotNull(_sut);
        Assert.True(_sut.Options.TryGetExtension(GlobalStPrefix, out var filePrefix));
        Assert.Equal("ST_FILE_", filePrefix);
        Assert.True(_sut.Options.TryGetExtension(GlobalFbPrefix, out filePrefix));
        Assert.Equal("FB_FILE_", filePrefix);
        Assert.True(_sut.Options.TryGetExtension(GlobalEnumPrefix, out filePrefix));
        Assert.Equal("E_FILE_", filePrefix);
    }

    [Fact]
    public async Task ShouldGenerateCorrectMessagePrefix()
    {
        Assert.NotNull(_sut);
        var md = _sut.MessageType.Single(m => m.Name == nameof(TestMessagePrefix));
        Assert.NotNull(md);
        Assert.True(md.Options.TryGetExtension(StPrefix, out var msgPrefix));
        Assert.Equal("ST_MSG_", msgPrefix);
        Assert.True(md.Options.TryGetExtension(FbPrefix, out msgPrefix));
        Assert.Equal("FB_MSG_", msgPrefix);
    }
}
