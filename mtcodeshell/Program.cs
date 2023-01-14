﻿using Mattodev.MattoScript.Builder;
using static Mattodev.MattoScript.Engine.CoreEng;

namespace Mattodev.Codeshell
{
	public class CdShInfo
	{
		public static string
			version = "0.1.1";
	}

	internal class Program
	{
		static bool canExit = false;
		static void reportErr(string msg)
		{
			Console.WriteLine("!" + msg);
		}
		static string exec(Dictionary<int, string> lns)
			=> Runner.runFromCode(
				lns
				.Select((pair) => pair.Value)
				.ToArray(), "Codeshell").cont;
		static string exec(string ln)
			=> Runner.runFromCode(ln, "Codeshell").cont;

		static void processCmd(string cmd, ref Dictionary<int, string> code)
		{
			string[] c = cmd.Split(' ');

			if (int.TryParse(c[0], out int lnNum))
				if (lnNum >= 1 && lnNum <= 10000)
					code[lnNum] = string.Join(" ", c[1..]);
				else
					reportErr("InvalidLineNum:" + c[0]);
			else
			{
				switch (c[0].ToLower())
				{
					case "run":
						string o = exec(code);
						Console.Write(o + (string.IsNullOrWhiteSpace(o) ? "" : "\n"));
						break;
					case "new":
						code = new();
						break;
					case "list":
						code.Where(ln => !string.IsNullOrWhiteSpace(ln.Value))
							.Select(pair => $"{pair.Key} {pair.Value}")
							.ToList()
							.ForEach(Console.WriteLine);

						Console.WriteLine();
						break;
					case "exit":
						canExit = true;
						break;
					default:
						string o2 = exec(cmd);
						Console.Write(o2 + (string.IsNullOrWhiteSpace(o2) ? "" : "\n"));
						break;
				}
			}
		}

		static void Main(string[] args)
		{
			Console.BackgroundColor = ConsoleColor.Blue;
			Console.ForegroundColor = ConsoleColor.Yellow;
			bool bw = false;
			if (args.Length > 0)
			{
				if (args[0] == "-bw" || args[0] == "--grayscale")
					bw = true;
			}

			if (bw) Console.ResetColor();
			
			Console.Clear();
			Dictionary<int, string> code = new();

			Console.WriteLine($"Codeshell v{CdShInfo.version} for MattoScript v{MTSInfo.mtsVer} (engine v{MTSInfo.engVer})");
			
			while (true)
			{
				Console.Write("]");
				string? prompt = Console.ReadLine();
				if (!string.IsNullOrEmpty(prompt))
				{
					try
					{
						processCmd(prompt, ref code);
					}
					catch (Exception e)
					{
						reportErr("Internal:" + e.Message);
					}
				}
				Console.BackgroundColor = ConsoleColor.Blue;
				Console.ForegroundColor = ConsoleColor.Yellow;
				if (bw) Console.ResetColor();

				if (canExit) return;
			}
		}
	}
}