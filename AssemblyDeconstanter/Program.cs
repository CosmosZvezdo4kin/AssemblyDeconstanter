using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Options;

namespace AssemblyDeconstanter
{
    class Program
    {
		static bool automaticExit, help;

		static void Main(string[] args)
		{
			string defaultOutputDir = "deconstanted_assemblies";

			string input = string.Empty;
			string output = string.Empty;

			var options = new OptionSet
			{
				{ "i|input=", "Path (relative or absolute) to the input assembly", i => input = i },
				{ "o|output=", "Path/dir/filename for the output assembly", o => output = o },
				{ "e|exit", "Application should automatically exit", e => automaticExit = e != null},
				{ "h|help", "Show this message", h => help = h != null}
			};

			Console.WriteLine();

			List<string> extra;

			try
			{
				// parse the command line
				extra = options.Parse(args);

				if (help)
					ShowHelp(options);

				if (input == "" && extra.Count >= 1)
					input = extra[0];

				if (input == "")
					throw new OptionException();

				if (output == "" && extra.Count >= 2)
					output = extra[1];
			}
			catch (OptionException)
			{
				// output some error message
				Console.WriteLine("ERROR! Incorrect arguments. You need to provide the path to the assembly to deconstant.");
				Console.WriteLine("On Windows you can even drag and drop the assembly on the .exe.");
				Console.WriteLine("Try `--help' for more information.");
				Exit(10);
			}

			string inputFile = input;
			AssemblyDefinition assembly = null;
			string outputPath = string.Empty, outputName = string.Empty;

			if (string.IsNullOrEmpty(output))
			{
				try
				{
					outputPath = Path.GetDirectoryName(output);
					outputName = Path.GetFileName(output);
				}
				catch (Exception)
				{
					Console.WriteLine("ERROR! Invalid output argument.");
					Exit(20);
				}
			}


			if (!File.Exists(inputFile))
			{
				Console.WriteLine();
				Console.WriteLine("ERROR! File doesn't exist or you don't have sufficient permissions.");
				Exit(30);
			}

			try
			{
				assembly = AssemblyDefinition.ReadAssembly(inputFile);
			}
			catch (Exception)
			{
				Console.WriteLine();
				Console.WriteLine("ERROR! Cannot read the assembly. Please check your permissions.");
				Exit(40);
			}

			IEnumerable<TypeDefinition> allTypes = GetAllTypes(assembly.MainModule);
			IEnumerable<FieldDefinition> allFields = allTypes.SelectMany(t => t.Fields) ?? Array.Empty<FieldDefinition>();
			IEnumerable<PropertyDefinition> allProperties = allTypes.SelectMany(t => t.Properties) ?? Array.Empty<PropertyDefinition>();

			int count = 0;
			string reportString = "Changed {0} {1} to static.";

			foreach (FieldDefinition field in allFields)
			{
				if (field?.IsLiteral ?? true)
                {
					count++;
					field.IsLiteral = false;
                }
			}

			Console.WriteLine(reportString, count, "fields");

			if (string.IsNullOrEmpty(outputName))
			{
				outputName = Path.GetFileNameWithoutExtension(inputFile) + Path.GetExtension(inputFile);
				Console.WriteLine();
				Console.WriteLine(@"Info: Use default output name: ""{0}""", outputName);
			}

			if (string.IsNullOrEmpty(outputPath))
			{
				outputPath = defaultOutputDir;
				Console.WriteLine();
				Console.WriteLine(@"Info: Use default output dir: ""{0}""", outputPath);
			}

			Console.WriteLine("\nSaving a copy of the modified assembly ...");

			string outputFile = Path.Combine(outputPath, outputName);

			try
			{
				if (string.IsNullOrEmpty(outputPath) && !Directory.Exists(outputPath))
					Directory.CreateDirectory(outputPath);

				assembly.Write(outputFile);
			}
			catch (Exception)
			{
				Console.WriteLine();
				Console.WriteLine("ERROR! Cannot create/overwrite the new assembly. ");
				Console.WriteLine("Please check the path and its permissions " +
					"and in case of overwriting an existing file ensure that it isn't currently used.");
				Exit(50);
			}

			Console.WriteLine("Completed.");
			Console.WriteLine();
			Console.WriteLine("Use the deconstanted library as your main assembly and as reference.");
			Exit(0);
		}

		public static void Exit(int exitCode = 0)
		{
			if (!automaticExit)
			{
				Console.WriteLine();
				Console.WriteLine("Press any key to exit ...");
				Console.ReadKey();
			}
			Environment.Exit(exitCode);
		}

		private static void ShowHelp(OptionSet p)
		{
			Console.WriteLine("Usage: AssemblyInconstanter.exe [Options]+");
			Console.WriteLine("Creates a copy of an assembly in which all constant fields are static.");
			Console.WriteLine("An input path must be provided, the other options are optional.");
			Console.WriteLine("You can use it without the option identifiers;");
			Console.WriteLine("If so, the first argument is for input and the optional second one for output.");
			Console.WriteLine();
			Console.WriteLine("Options:");
			p.WriteOptionDescriptions(Console.Out);
			Exit(0);
		}

		private static IEnumerable<TypeDefinition> GetAllTypes(ModuleDefinition moduleDefinition)
		{
			return GetAllNestedTypes(moduleDefinition.Types);
		}

		private static IEnumerable<TypeDefinition> GetAllNestedTypes(IEnumerable<TypeDefinition> typeDefinitions)
		{
			if (typeDefinitions?.Count() == 0)
				return new List<TypeDefinition>();

			IEnumerable<TypeDefinition> result = typeDefinitions.Concat(GetAllNestedTypes(typeDefinitions.SelectMany(t => t.NestedTypes)));

			return result;
		}
	}
}
