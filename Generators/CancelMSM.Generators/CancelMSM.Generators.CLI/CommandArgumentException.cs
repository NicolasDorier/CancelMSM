using System;
using System.Collections.Generic;
using System.Text;

namespace CancelMSM.Generators.CLI
{
	public class CommandArgumentException : Exception
	{
		public CommandArgumentException(string optionName, string error) : base($"{optionName} : {error}")
		{

		}
	}
}
