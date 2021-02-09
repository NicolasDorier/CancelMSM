using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CancelMSM.Generators.CLI.Commands
{
	class GenerateSeleniumFile : CommandBase
	{
		internal static Command CreateCommand()
		{
			var cmd = new Command("side", "Generate side file by extracting twitter handle present in a file");
			cmd.AddOption(new Option<string>(new[] { "--file", "-f" }, "A file containing twitter")
			{
				Argument = new Argument<string>() { Arity = ArgumentArity.ExactlyOne },
				IsRequired = true
			});
			cmd.Handler = new GenerateSeleniumFile();
			return cmd;
		}
		protected override Task<int> InvokeAsyncCore(InvocationContext context)
		{
			var file = context.ParseResult.CommandResult.ValueForOption<string>("file")?.Trim();
			if (!File.Exists(file))
				throw new CommandArgumentException("--file", "The file does not exists");

			var jobj = JObject.Parse(GetResource("TwitterBlockTemplate.side.json"));
			var testName = jobj.SelectToken($"$.tests[0].name").Value<string>();
			ReplaceIds(jobj);

			var model = (JObject)jobj.SelectToken($"$.tests[0]");
			model.Remove();
			foreach (var batch in GetBatches(GetTwitterHandles(File.ReadAllText(file)), batchSize: 100))
			{
				var tests = (JArray)jobj.SelectToken("$.tests");
				tests.Add(model.DeepClone());
				var arr = (JArray)jobj.SelectToken($"$.tests[{batch.BatchIndex}].commands");
				arr.Clear();
				foreach (var handle in batch.Batch)
				{
					var deletion = model["commands"].DeepClone();
					deletion[0]["target"] = new JValue($"/{handle}");
					foreach (var child in deletion)
					{
						ReplaceIds(child);
						arr.Add(child.DeepClone());
					}
				}
				arr.Parent.Parent["name"] = testName + "-" + batch.BatchIndex;
			}
			File.WriteAllText(Path.Combine(OutputDirectory, $"BlockTwitter.side"), jobj.ToString(Newtonsoft.Json.Formatting.Indented));
			return Task.FromResult(0);
		}

		private IEnumerable<(int BatchIndex, List<T> Batch)> GetBatches<T>(IEnumerable<T> list, int batchSize)
		{
			var enumerator = list.GetEnumerator();
			int batchIndex = 0;
			while (true)
			{
				var batch = new List<T>(batchSize);
				int count = 0;
				while (count != batchSize)
				{
					if (!enumerator.MoveNext())
						if (count == 0)
							yield break;
						else
						{
							yield return (batchIndex++, batch);
							yield break;
						}
					count++;
					batch.Add(enumerator.Current);
				}
				yield return (batchIndex++, batch);
			}
		}
		private IEnumerable<string> GetTwitterHandles(string content)
		{
			HashSet<string> handles = new HashSet<string>();
			var matches = Regex.Matches(content, @"@([a-zA-Z0-9]+)");
			foreach (var match in matches.OfType<Match>())
			{
				handles.Add(match.Groups[1].Value);
			}
			matches = Regex.Matches(content, "https://twitter\\.com/([a-zA-Z0-9]+)");
			foreach (var match in matches.OfType<Match>())
			{
				handles.Add(match.Groups[1].Value);
			}
			return handles.OrderByDescending(o => o).ToList();
		}

		private void ReplaceIds(JToken jobj)
		{
			foreach (var id in jobj.SelectTokens("$..id"))
			{
				((JProperty)id.Parent).Value = new JValue(NextGuid());
			}
		}

		Random random = new Random(0);
		private string NextGuid()
		{
			var id = new byte[16];
			random.NextBytes(id);
			return new Guid(id).ToString();
		}

		private static string GetResource(string resourceName)
		{
			var stream = typeof(GenerateSeleniumFile).Assembly.GetManifestResourceStream($"CancelMSM.Generators.CLI.Data.{resourceName}");
			var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}
	}
}
