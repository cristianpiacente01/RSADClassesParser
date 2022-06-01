using System;
using CommandLine;


namespace RSADClassesParser
{
    internal class Program
    {
        public class Options
        {

            [Option('c', "collab", Required = false, HelpText = "Specify your RSAD's collaboration name to extract operations' calls (TODO).")]
            public string Collaboration { get; set; }

            [Option('i', "input", Required = false, HelpText = "Set the .emx path, by default it's \"Blank Package.emx\" in the current directory")]
            public string InputPath { get; set; }


            // TODO write logs and errors to file
            public static void Log(string msg)
            {
                Console.WriteLine("[Log] " + msg);
            }

            public static void Error(string msg)
            {
                Console.WriteLine("[ERROR] " + msg);
                Environment.Exit(0);
            }

            public string getEmxPath()
            {
                return (InputPath != null) ? InputPath : (Path.Combine(Directory.GetCurrentDirectory(), "Blank Package.emx"));
            }

        }

        static void pathChecks(string path)
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

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       string path = o.getEmxPath();

                       Options.Log("Checking if " + path + " is a valid .emx path...");
                       pathChecks(path);
                       Options.Log("OK! Parsing .emx...");

                       EmxParser parser = new EmxParser(path);
                       parser.parse();
                   });
        }
    }
}