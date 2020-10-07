using CancelMSM.Generators.CLI.Commands;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace CancelMSM.Generators.CLI
{
    class Program
    {
		static async Task Main(string[] args)
		{
			RootCommand root = CreateCommand();
			await root.InvokeAsync(args);
		}

		private static RootCommand CreateCommand()
		{
			RootCommand root = new RootCommand("This tool is used by this repository to generate various artifacts");
			root.AddOption(new Option<string>("--outputdir", "Output directory where artifacts will be generated")
			{
				Argument = new Argument<string>() { Arity =	ArgumentArity.ZeroOrOne },
				IsRequired = false
			});
			var selenium = new Command("selenium", "Selenium related commands");
			root.AddCommand(selenium);
			selenium.AddCommand(GenerateSeleniumFile.CreateCommand());
			return root;
		}

	}
}
