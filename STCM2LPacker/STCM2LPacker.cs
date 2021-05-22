using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using static STCM2LPacker.STCM2L;

namespace STCM2LPacker
{
	static class STCM2LPacker
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
			// Drag & Drop
			if (args != null && args.Length > 0)
			{
				string mode = "export";
				string encoding = "shift-jis";
				string opcode = "opcode.txt";
				string table = "table.txt";
				foreach (var arg in args)
				{
					if (arg.StartsWith("--mode"))
					{
						mode = arg.Split('=')[1];
					}
					else if (arg.StartsWith("--encoding"))
					{
						encoding = arg.Split('=')[1];
					}
					else if (arg.StartsWith("--opcode"))
					{
						opcode = arg.Split('=')[1];
					}
					else if (arg.StartsWith("--table"))
					{
						table = arg.Split('=')[1];
					}
				}
				string opcodeFile = Path.Combine(args[0], opcode);
				string tableFile = Path.Combine(args[0], table);
				ReadFiles(args[0], opcodeFile, tableFile, mode, encoding);
			}
			// Run GUI
			else
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new STCM2LPackerGUI());
			}
		}

		public static void ReadFiles(string inputPath, string opcodeFile, string tableFile, string mode, string encoding)
		{
			// Get files
			FileAttributes attr = File.GetAttributes(inputPath);
			string[] inputFiles = attr.HasFlag(FileAttributes.Directory) ?
				Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories)
				: new string[] { inputPath };

			// Read opcode
			ReadOpCodeTable(opcodeFile);

			// Read files
			var stcm2lMap = new Dictionary<string, STCM2L>();
			foreach (var file in inputFiles)
			{
				var stcm2l = ReadFile(file, encoding);
				if (stcm2l != null)
				{
					stcm2lMap.Add(file, stcm2l);
				}
			}

			// Export
			if (mode.StartsWith("export"))
			{
				if (mode == "export-excel")
				{
					ExportExcel(stcm2lMap, inputPath + ".xlsx");
				}
				else
				{
					foreach (var file in stcm2lMap.Keys)
					{
						STCM2L stcm2l = null;
						stcm2lMap.TryGetValue(file, out stcm2l);
						if (stcm2l != null)
						{
							var outputFile = file + ".csv";
							Export(stcm2l, outputFile);
						}
					}
				}
			}

			// Import & Patch
			else if (mode == "import")
			{
				string outputPath = Path.Combine(Path.GetDirectoryName(inputPath), "/out");
				
			}
		}

		public static STCM2L ReadFile(string inputFile, string encoding = "shift-jis")
		{
			STCM2L stcm2l = null;
			// Build STCM2L file structure
			using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open)))
			{
				// Check magic
				var magic = Encoding.ASCII.GetString(b.ReadBytes(6));
				if (magic == "STCM2L")
				{
					b.BaseStream.Seek(0, SeekOrigin.Begin);
					stcm2l = new STCM2L();
					stcm2l.read(b, encoding);
				}
			}
			return stcm2l;
		}

		public static Dictionary<String, String> ReadOpCodeTable(string opcodeTableFile)
		{
			var opcodeTable = new Dictionary<String, String>();

			if (opcodeTableFile != "")
			{
				string[] opcodes = File.ReadAllLines(opcodeTableFile);
				foreach (string opcode in opcodes)
				{
					if (opcode.StartsWith("//")) continue;
					var parts = opcode.Split('=');
					if (parts.Length >= 2)
					{
						opcodeTable.Add(parts[0], parts[1]);
					}
				}
			}
			_opcodeTable = opcodeTable;
			return opcodeTable;
		}

		public static List<uint> GetPossibleTextOpcodes(STCM2L stcm2l)
		{
			var hiragana = "あぁかさたなはまやゃらわがざだばぱいぃきしちにひみりゐぎじぢびぴうぅくすつぬふむゆゅるぐずづぶぷえぇけせてねへめれゑげぜでべぺおぉこそとのほもよょろをごぞどぼぽゔっんーゝゞ、。";
			var katakana = "アァカサタナハマヤャラワガザダバパイィキシチニヒミリヰギジヂビピウゥクスツヌフムユュルグズヅブプエェケセテネヘメレヱゲゼデベペオォコソトノホモヨョロヲゴゾドボポヴッン・ーヽヾ、。";
			var alphabet = "";//"qwertyuiopasdfghjklzxcvbnmQEWRTYUIOPASDFGHJKLZXCVBNM";

			var opcodes = new List<uint>();
			foreach (var instruction in stcm2l.instructions)
			{
				if (instruction.extraData != null && !opcodes.Contains(instruction.opcode)
					&& instruction.lines != null && instruction.lines.Count > 0)
				{
					string text = Encoding.GetEncoding(_encoding).GetString(instruction.extraData);
					if (!text.Contains("_"))
					{
						var countMatch = 0;
						foreach (char textChar in text)
						{
							var charString = textChar.ToString();
							if (hiragana.Contains(charString) || katakana.Contains(charString) || alphabet.Contains(charString))
							{
								countMatch++;
							}
						}
						if (countMatch >= 5)
						{
							opcodes.Add(instruction.opcode);
						}
					}
				}
			}
			return opcodes;
		}

		public static void Export(STCM2L stcm2l, string outputFile, string header = "ID,Japanese,Vietnamese,Note")
		{
			if (stcm2l._textLineCount == 0) return;

			// Write text to csv
			using (StreamWriter sw = new StreamWriter(File.Open(outputFile, FileMode.Create), Encoding.UTF8))
			{
				sw.WriteLine(header);
				var no = 1;
				foreach (var instruction in stcm2l.instructions)
				{
					var opcodeHex = instruction.opcode.ToString("x").ToUpper();
					if (_opcodeTable != null && _opcodeTable.ContainsKey(opcodeHex) && instruction.lines != null && instruction.lines.Count > 0)
					{
						foreach (var line in instruction.lines)
						{
							var label = "";
							_opcodeTable.TryGetValue(opcodeHex, out label);

							var id = String.Format("{0}__{1}_{2}", no, line._paramValueOffset.ToString("x").ToUpper(), label);
							var text = line.text.Replace('"', '“').TrimEnd();

							sw.WriteLine("{0},\"{1}\",,", id, text);
							no++;
						}
					}
				}
			}
		}

		public static void Import(STCM2L stcm2l, string csvFile, string inputFile, string outputFile, string tableFile = "")
		{
			// Replace char table
			var csvText = File.ReadAllText(csvFile);
			if (tableFile != "")
			{
				var table = File.ReadAllLines(tableFile);
				foreach (var line in table)
				{
					var parts = line.Split('=');
					if (parts.Length == 2)
					{
						csvText = csvText.Replace(parts[0], parts[1]);
					}
				}
			}

			// Copy old file to new file and work on new file
			File.Copy(inputFile, outputFile, true);
			FileInfo fileInfo = new FileInfo(outputFile);
			fileInfo.IsReadOnly = false;

			using (BinaryWriter b = new BinaryWriter(File.Open(outputFile, FileMode.Open)))
			{
				var csvLines = csvText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

				// Make text data map
				var lineMap = new Dictionary<int, Line>();
				var count = 1;
				foreach (var instruction in stcm2l.instructions)
				{
					if (instruction.isText() && instruction.lines != null && instruction.lines.Count > 0)
					{
						foreach(var line in instruction.lines)
						{
							lineMap.Add(count, line);
							count++;
						}
					}
				}

				// Loop through lines in the CSV
				for (var i = 1; i < count; i++)
				{
					// exlcude header line
					var csvLine = csvLines[i];
					Line line;
					lineMap.TryGetValue(i, out line);

					// Parse new text
					b.Seek(0, SeekOrigin.End);
					var newOffset = Convert.ToInt32(b.BaseStream.Position);
					var parts = Regex.Split(csvLine, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
					var newText = parts[2].Replace("\"", string.Empty).Replace("\n", " ");
					byte[] textBytes = Encoding.GetEncoding(_encoding).GetBytes(newText);
					int newTextLen = textBytes.Length + (4 - textBytes.Length % 4);
					int newTextOpCode1 = newTextLen / 4;
					
					// Write new text to end of file
					b.Write(line.zero);
					b.Write(newTextOpCode1);
					b.Write(line.one);
					b.Write(newTextLen);
					b.Write(textBytes);

					// Pad zero byte to text so the length is divisible by 4
					byte[] zeros = new byte[newTextLen - textBytes.Length];
					b.Write(zeros);

					// Update param value that points to text offset
					var csvId = csvLine.Split(',')[0];
					var splitParts1 = Regex.Split(csvId, "___");
					var splitParts2 = Regex.Split(splitParts1[1], "_");
					var hexOffset = splitParts2[0];

					var offset = Convert.ToInt32(hexOffset, 16);
					b.Seek(offset, SeekOrigin.Begin);
					b.Write(newOffset);
				}
			}
		}

		public static void ExportExcel(Dictionary<string, STCM2L> stcm2lMap, string outputFile, string header = "ID,Japanese,Vietnamese,Note")
		{
			// Create spreadsheet
			SpreadsheetDocument spreadSheet = createExcel(outputFile);

			// Add TOC sheet
			var tocSheet = addSheet(spreadSheet, "TOC");
			addRow(tocSheet, new List<string> { "File", "Translator", "Progress", "Note" });
			var nameSheet = addSheet(spreadSheet, "TABLE_NAME");
			var tableSheet = addSheet(spreadSheet, "TABLE");

			var lineCount = 1;
			var listNames = new List<String>();
			foreach (var stcm2lFile in stcm2lMap.Keys)
			{
				var sheetName = Path.GetFileName(stcm2lFile);
				var sheet = addSheet(spreadSheet, sheetName);

				var headerCells = header.Split(',').ToList();
				addRow(sheet, headerCells);

				STCM2L stcm2l = null;
				stcm2lMap.TryGetValue(stcm2lFile, out stcm2l);

				foreach (Instruction instruction in stcm2l.instructions)
				{
					var opcodeHex = instruction.opcode.ToString("x").ToUpper();
					if (_opcodeTable != null && _opcodeTable.ContainsKey(opcodeHex) && instruction.lines != null && instruction.lines.Count > 0)
					{
						foreach (var line in instruction.lines)
						{
							var label = "";
							_opcodeTable.TryGetValue(opcodeHex, out label);

							var id = String.Format("{0}__{1}__{2}", lineCount, line._paramValueOffset, label);
							var text = line.text.TrimEnd().Replace("\0", string.Empty);

							if (label == "NAME")
							{
								var index = listNames.IndexOf(text);
								if (index == -1)
								{
									listNames.Add(text);
									addRow(nameSheet, new List<string>() { text, text });
								}
								index = listNames.IndexOf(text);
								text = "=TABLE_NAME!B" + (index + 1);
							}

							var cells = new List<String>();
							cells.Add(id);
							cells.Add(text);
							addRow(sheet, cells);

							lineCount++;
						}
					}
				}

				// Add to TOC
				var tocCells = new List<String>();
				tocCells.Add(String.Format("=HYPERLINK(\"#'{0}'!$A$1\",{0})", sheetName));
				tocCells.Add("");
				tocCells.Add(String.Format("=ROUND(COUNTA('{0}'!C:C)/COUNTA('{0}'!B:B)*100, 2) & \"%\"", sheetName));
				addRow(tocSheet, tocCells);
			}

			spreadSheet.Close();
		}

		private static SpreadsheetDocument createExcel(string outputFile)
		{
			// Create spreadsheet
			SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(outputFile, SpreadsheetDocumentType.Workbook);
			// Add a WorkbookPart to the document.
			WorkbookPart workbookpart = spreadSheet.AddWorkbookPart();
			workbookpart.Workbook = new Workbook();
			// Add Sheets to the Workbook.
			Sheets sheets = spreadSheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
			return spreadSheet;
		}

		private static WorksheetPart addSheet(SpreadsheetDocument spreadSheet, string sheetName)
		{
			// Add a blank WorksheetPart.  
			WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
			newWorksheetPart.Worksheet = new Worksheet(new SheetData());

			// Create a Sheets object in the Workbook.  
			Sheets sheets = spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
			string relationshipId = spreadSheet.WorkbookPart.GetIdOfPart(newWorksheetPart);

			// Create a unique ID for the new worksheet.  
			uint sheetId = 1;
			if (sheets.Elements<Sheet>().Count() > 0)
			{
				sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
			}

			// Append the new worksheet and associate it with the workbook.  
			Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
			sheets.Append(sheet);

			return newWorksheetPart;
		}

		private static Row addRow(WorksheetPart worksheetPart, List<String> cellTexts)
		{
			SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
			Row row = sheetData.AppendChild(new Row());

			foreach (var cellText in cellTexts)
			{
				// Add the cell to the cell table at A1.  
				Cell refCell = null;
				Cell newCell = new Cell();
				row.InsertBefore(newCell, refCell);

				// Set the cell value
				// Formula
				if (cellText.StartsWith("="))
				{
					CellFormula cellFormula = new CellFormula(cellText);
					newCell.Append(cellFormula);
				}
				// String
				else
				{
					newCell.CellValue = new CellValue(cellText);
					newCell.DataType = new EnumValue<CellValues>(CellValues.String);
				}
			}
			return row;
		}
	}
}
