using Mattodev.MattoScript.Builder;
using static Mattodev.MattoScript.Engine.CoreEng;

namespace Mattodev.Codeshell
{
	public class CdShInfo
	{
		public static string
			version = "0.1.2";
	}

	internal class Program
	{
		static bool canExit = false;
		static void reportErr(string msg)
		{
			Console.WriteLine("!" + msg);
		}
		static T[] FillArr<T>(int size, T defaultValue)
		{
			T[] arr = new T[size];
			for (int i = 0; i < size; i++)
				arr[i] = defaultValue;
			return arr;
		}
		static TArr[] FillWithDictionary<TArr>(TArr[] arrIn, Dictionary<int, TArr> dict, int offset = 0)
		{
			TArr[] arr = arrIn;
			for (int i = 0; i < dict.Max(p => p.Key); i++)
				arr[i] = dict.GetValueOrDefault(i + offset, arrIn[i]);
			return arr;
		}
		static MTSConsole exec(Dictionary<int, string> lns)
			=> Runner.runFromCode(
				FillWithDictionary(
					FillArr(lns.Max(pair => pair.Key), "nop"), lns, 1),
				"Codeshell");
		static MTSConsole exec(string ln)
			=> Runner.runFromCode(ln, "Codeshell");

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
						string o = exec(code).cont;
						Console.Write(o + (string.IsNullOrWhiteSpace(o) ? "" : "\n"));
						break;
					case "new":
						code = new();
						break;
					case "list":
						int min, max;
						switch (c.Length - 1) {
							case 0:
								code.Where(ln => !string.IsNullOrWhiteSpace(ln.Value))
									.Select(pair => $"{pair.Key} {pair.Value}")
									.ToList()
									.ForEach(Console.WriteLine);
								break;
							case 1:
								min = int.Parse(c[1]);
								code.Where(ln => !string.IsNullOrWhiteSpace(ln.Value) && ln.Key == min)
									.Select(pair => $"{pair.Key} {pair.Value}")
									.ToList()
									.ForEach(Console.WriteLine);
								break;
							default:
								min = int.Parse(c[1]);
								max = int.Parse(c[2]);
								code.Where(ln => !string.IsNullOrWhiteSpace(ln.Value) && ln.Key >= min && ln.Key <= max)
									.Select(pair => $"{pair.Key} {pair.Value}")
									.ToList()
									.ForEach(Console.WriteLine);
								break;
						}

						Console.WriteLine();
						break;
					case "exit":
						canExit = true;
						break;
					case "clear":
						Console.Clear();
						break;
					case "save":
						if (c.Length < 1)
							reportErr("NoArgsGiven");
						else
							File.WriteAllLines(c[1] + ".cdsh",
								code.OrderBy(p => p.Key)
									.Where(p => p.Value != "nop")
									.Select(p => $"{p.Key} {p.Value}")
							);
						break;
					case "load":
						if (c.Length < 1)
							reportErr("NoArgsGiven");
						else
						{
							code = new();
							var cd = code;
							File.ReadAllLines(c[1] + ".cdsh")
								.ToList()
								.ForEach(ln => cd.Add(
									int.Parse(ln.Split(' ')[0]),
									string.Join(" ", ln.Split(' ')[1..]))
								);
							code = cd;
						}
						break;
					default:
						string o2 = exec(cmd).cont;
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
				switch (args[0])
				{
					case "-bw" or "--grayscale":
						bw = true;
						break;
				}
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