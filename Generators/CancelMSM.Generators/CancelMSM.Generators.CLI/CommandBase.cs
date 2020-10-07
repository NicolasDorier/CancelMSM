using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CancelMSM.Generators.CLI
{
	class CommandBase : ICommandHandler
	{
		public async Task<int> InvokeAsync(InvocationContext context)
		{
			OutputDirectory = context.ParseResult.RootCommandResult.ValueForOption<string>("outputdir") ?? Directory.GetCurrentDirectory();
			try
			{
				return await InvokeAsyncCore(context);
			}
			catch(CommandArgumentException ex)
			{
				context.Console.Error.WriteLine(ex.Message);
				return 1;
			}
		}

		protected virtual Task<int> InvokeAsyncCore(InvocationContext context)
		{
			return Task.FromResult(0);
		}

		public string OutputDirectory { get; set; }
	}
}
