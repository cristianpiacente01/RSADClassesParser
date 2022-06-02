using System;
using CommandLine;


namespace RSADClassesParser
{
    internal class Program
    {
        public class Options
        {

            [Option('i', "input", Required = false, HelpText = "Sets the .emx path, by default it's \"Blank Package.emx\" in the current directory.")]
            public string InputPath { get; set; }

            [Option('o', "output", Required = false, HelpText = "Sets the output directory, by default it's the current directory (a folder named FileJava will be created). If it fails to create a directory in the specified path then it'll try to use the current directory and if it still fails it'll just print on the console.")]
            public string OutputPath { get; set; }


            public static String output = "console"; // this will be changed if it can successfully create a new directory

            public static void Log(string msg)
            {
                Console.WriteLine("[Log " + DateTime.Now + "] " + msg);
            }

            public static void Error(string msg)
            {
                Console.WriteLine("[ERROR " + DateTime.Now + "] " + msg);
                Environment.Exit(0);
            }

            public string GetEmxPath()
            {
                return (InputPath != null) ? InputPath : (Path.Combine(Directory.GetCurrentDirectory(), "Blank Package.emx"));
            }

            public string GetOutputPath()
            {
                return (OutputPath != null) ? (Path.Combine(OutputPath, "FileJava")) : (Path.Combine(Directory.GetCurrentDirectory(), "FileJava"));
            }

        }

        static void DoPathChecks(string path)
        {
            
            if (!File.Exists(path))
            {
                Options.Error("File not found!");
            }
            if (!Path.GetExtension(path).Equals(".emx"))
            {
                Options.Error("Not an .emx!");
            }
        }

        static void DoOutputPathChecks(string path, Options o, Boolean retry = true)
        {
            if (Directory.Exists(path))
            {
                Options.Log("Using " + path + " as the output path!");
                Options.output = path;
            }
            else
            {
                try
                {
                    DirectoryInfo d = Directory.CreateDirectory(path);
                    Options.Log("Successfully created a new directory, now using " + path);
                    Options.output = path;
                } 
                catch (Exception e)
                {
                    if (retry)
                    {
                        Options.Log("Failed to create a new directory, now trying in the current directory...");
                        o.OutputPath = null;
                        DoOutputPathChecks(o.GetOutputPath(), o, false);
                    }
                    else
                    {
                        Options.Log("Failed to create a new directory, the console will be used as the output");
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       string path = o.GetEmxPath();
                       string outputPath = o.GetOutputPath();

                       Options.Log("Checking if " + path + " is a valid .emx path...");
                       Program.DoPathChecks(path);

                       Options.Log("Checking if " + outputPath + " can be a valid output path...");
                       Program.DoOutputPathChecks(outputPath, o);

                       Options.Log("Done! Parsing .emx...");

                       EmxParser parser = new EmxParser(path);
                       parser.Parse();
                   });
        }
    }
}