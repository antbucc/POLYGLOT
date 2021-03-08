using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Interactive.Commands;
using Polyglot.Interactive.Tests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Polyglot.Interactive.Tests
{
    public class ExtensionTests : LanguageKernelTestBase
    {
        public ExtensionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task kernel_extension_loads()
        {
            var extension = new KernelExtension();
            var kernel = CreateKernel();

            await extension.OnLoadAsync(kernel);

            KernelEvents.Should().NotContainErrors();

        }

        [Fact]
        public async Task adds_command_to_configure_game_engine()
        {
            var extension = new KernelExtension();
            var kernel = CreateKernel();
            await extension.OnLoadAsync(kernel);
            
            kernel.Directives.Should().ContainSingle(d => d.Name == "#!game-time");
        }

        [Fact]
        public async Task user_can_configure_game_engine()
        {
            var extension = new KernelExtension();
            var kernel = CreateKernel();
            await extension.OnLoadAsync(kernel);

            GameEngineClient.Current.Should().BeNull();
            await kernel.SendAsync(new SubmitCode("#!game-time --player-id test --game-id gameOne --game-token 123"), CancellationToken.None);
            KernelEvents.Should().NotContainErrors();

            GameEngineClient.Current.Should().NotBeNull();
        }

        [Fact]
        public async Task adds_middleware_to_intercept_code_submissions()
        {
            var extension = new KernelExtension();
            var kernel = CreateKernel();
            await extension.OnLoadAsync(kernel);
            await kernel.SendAsync(new SubmitCode("#!game-time --player-id my_user_id --game-id polyglot --game-token 123"), CancellationToken.None);

            await kernel.SendAsync(new SubmitCode("\"Hello World\""), CancellationToken.None);
            
            KernelEvents.Should().NotContainErrors();

        }

    }
}
