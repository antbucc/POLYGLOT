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
            
            kernel.Directives.Should().ContainSingle(d => d.Name == "#!start-game");
        }

        [Fact]
        public async Task user_can_configure_game_engine()
        {
            var extension = new KernelExtension();
            var kernel = CreateKernel();
            await extension.OnLoadAsync(kernel);


            GameEngineClient.Current.Should().BeNull();
            await kernel.SendAsync(new SubmitCode("#!start-game --player-id papyrus --game-id 603fced708813b0001baa2cc --game-token papyrus0704!"), CancellationToken.None);
            KernelEvents.Should().NotContainErrors();

            GameEngineClient.Current.Should().NotBeNull();
        }

        [Fact]
        public async Task adds_middleware_to_intercept_code_submissions()
        {
            var extension = new KernelExtension();
            var kernel = CreateKernel();
            await extension.OnLoadAsync(kernel);
            
            await kernel.SendAsync(new SubmitCode("#!start-game --player-id papyrus --game-id 603fced708813b0001baa2cc --game-token papyrus0704!"), CancellationToken.None);

            await kernel.SendAsync(new SubmitCode("\"Hello World\""), CancellationToken.None);
            
            KernelEvents.Should().NotContainErrors();
        }

        [Fact]
        public async Task intercepts_errors()
        {
            var extension = new KernelExtension();
            var kernel = CreateKernel();
            await extension.OnLoadAsync(kernel);

            await kernel.SendAsync(new SubmitCode("#!start-game --player-id papyrus --game-id 603fced708813b0001baa2cc --game-token papyrus0704!"), CancellationToken.None);

            await kernel.SendAsync(new SubmitCode("valid at all"), CancellationToken.None);

            var report = await GameEngineClient.Current.GetReportAsync();

            report.Score.Should().Be(0);
        }
    }
}
