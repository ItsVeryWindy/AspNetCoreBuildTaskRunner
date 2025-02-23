using CliWrap;
using CliWrap.Buffered;

namespace AspNetCoreBuildTaskRunner.Tests
{
    public class IntegrationTests
    {
        [Test]
        public async Task ShouldExecuteAspNetCoreBuilderTaskRunner()
        {
            WriteCSharpProject();

            var result = await Cli.Wrap("dotnet")
                .WithArguments("build -bl:msbuild.binlog project/web/web.csproj")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            Assert.That(result.IsSuccess, Is.True, () => result.StandardOutput);

            Assert.That(result.StandardOutput, Does.Contain("I did something").And.Contain("hello").And.Contain("world"));

            Console.WriteLine(result.StandardOutput);
        }

        private static void WriteCSharpProject()
        {
            Directory.Delete("project", true);

            var dir = new DirectoryInfo("Resources/project");

            var dirStack = new Stack<(DirectoryInfo dir, string dest)>();

            dirStack.Push((dir, "project"));

            while (dirStack.Count > 0)
            {
                (dir, var dest) = dirStack.Pop();

                Directory.CreateDirectory(dest);

                foreach (var file in dir.GetFiles())
                {
                    var targetFilePath = Path.Combine(dest, file.Name);
                    file.CopyTo(targetFilePath);
                }

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    dirStack.Push((subDir, Path.Combine(dest, subDir.Name)));
                }
            }
        }
    }
}