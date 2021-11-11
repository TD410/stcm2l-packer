using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using static STCM2LPacker.STCM2L;

namespace STCM2LPacker
{
	static class STCM2LPacker
	{
		public static void Main(string[] args)
		{
			if (args != null && args.Length > 0)
			{
				string stcm2lPath = "";
				string csvPath = "";
				string outPath = "";
				string mode = "";
				string encoding = "shift-jis";
				string opcode = "opcode.txt";
				string table = "table.txt";

				foreach (var arg in args)
				{
					if (arg.StartsWith("--stcm2l"))
					{
						stcm2lPath = arg.Split('=')[1];
					}
					else if (arg.StartsWith("--csv"))
					{
						csvPath = arg.Split('=')[1];
					}

					else if (arg.StartsWith("--out"))
					{
						outPath = arg.Split('=')[1];
					}
					else if (arg.StartsWith("--mode"))
					{
						mode = arg.Split('=')[1];
					}
					else if (arg.StartsWith("--encoding"))
					{
						encoding = arg.Split('=')[1];
					}
				}
				string opcodeFile = Path.Combine(stcm2lPath, opcode);
				string tableFile = Path.Combine(stcm2lPath, table);

				Process(stcm2lPath, csvPath, outPath, opcodeFile, tableFile, mode, encoding);
			}
		}

		public static void Process(string stcm2lPath, string csvPath, string outPath, string opcodeFile, string tableFile, string mode, string encoding)
		{
			var stcm2lDict = ReadSTCM2LFiles(stcm2lPath, opcodeFile, encoding);

			// Export
			switch (mode)
			{
				case "unpack":
					break;
				case "repack":
					Repack(stcm2lDict, stcm2lPath, csvPath, outPath, tableFile);
					break;
			}
		}

		/** COMMON **/

		public static Dictionary<string, STCM2L> ReadSTCM2LFiles(string stcm2lPath, string opcodeFile, string encoding)
		{
			// Get files
			FileAttributes attr = File.GetAttributes(stcm2lPath);
			string[] stcm2lFiles = attr.HasFlag(FileAttributes.Directory) ?
				Directory.GetFiles(stcm2lPath, "*", SearchOption.AllDirectories)
				: new string[] { stcm2lPath };

			// Read opcode
			ReadOpCodeTable(opcodeFile);

			// Read files
			var stcm2lMap = new Dictionary<string, STCM2L>();
			foreach (var file in stcm2lFiles)
			{
				var stcm2l = ReadSTMC2LFile(file, encoding);
				if (stcm2l != null)
				{
					stcm2lMap.Add(file, stcm2l);
				}
			}

			return stcm2lMap;
		}

		public static STCM2L ReadSTMC2LFile(string inputFile, string encoding)
		{
			STCM2L stcm2l = null;
			// Build STCM2L file structure
			using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read)))
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
		
		/** UNPACK **/

		public static void UnpackCSV(STCM2L stcm2l, string outputFile, string header = "ID,Japanese,Vietnamese,Note")
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

		public static void UnpackExcel(Dictionary<string, STCM2L> stcm2lMap, string outputFile, string header = "ID,Japanese,Vietnamese,Note")
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

		/** REPACK **/

		public class CSVLine
		{
			public string ID { get; set; } // id
			public string Japanese { get; set; } // japanese
			public string Vietnamese { get; set; } // tranlated
		}

		public class CSVLineMap : ClassMap<CSVLine>
		{
			public CSVLineMap()
			{
				Map(m => m.ID).Name("\"ID\"", "ID");
				Map(m => m.Japanese).Name("English", "Japanese");
				Map(m => m.Vietnamese).Name("__EMPTY", "Vietnamese").Optional();
			}
		}

		public class ImportLine
		{
			public byte[] textBytes;
			public int offset;
		}

		public static void Repack(Dictionary<string, STCM2L> stcm2lDict, string stcm2lPath, string csvPath, string outPath, string tableFile)
		{
			// Get csv files
			FileAttributes attr = File.GetAttributes(csvPath);
			string[] csvFiles = attr.HasFlag(FileAttributes.Directory) ?
				Directory.GetFiles(csvPath, "*", SearchOption.AllDirectories)
				: new string[] { csvPath };

			// Read table
			var tableList = File.ReadAllLines(tableFile);

			// Loop through csv files
			foreach (var csvFile in csvFiles)
			{
				var importLines = ReadCSV(csvFile, tableList);

				var fileName = csvFile.Replace(csvPath, "")
					.Replace(".csv", string.Empty)
					.Replace(".dat", string.Empty);
				var inputSTMCFile = stcm2lPath + fileName;
				var outputSTMCFile = outPath + fileName;

				STCM2L stcm2l;
				stcm2lDict.TryGetValue(inputSTMCFile, out stcm2l);

				RepackSTCM2L(stcm2l, importLines, inputSTMCFile, outputSTMCFile);
			}
		}

		public static List<ImportLine> ReadCSV(string csvFile, string[] tableList)
		{
			var importLines = new List<ImportLine>();
			using (var reader = new StreamReader(csvFile))
			{
				using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
				{
					csv.Context.RegisterClassMap<CSVLineMap>();

					var csvLines = csv.GetRecords<CSVLine>().ToList();
					csvLines.ForEach(csvLine =>
					{
						if (csvLine.ID != string.Empty && csvLine.ID != null
						&& csvLine.Japanese != string.Empty && csvLine.Japanese != null
						&& csvLine.Vietnamese != string.Empty && csvLine.Vietnamese != null)
						{
							// Get text & replace with table
							var newText = csvLine.Vietnamese;
							foreach (var line in tableList)
							{
								var parts = line.Split('=');
								newText = newText.Replace(parts[0], parts[1]);
							}

							// Replace new line character
							newText = Regex.Replace(newText, "\n", "#n");

							// Get text bytes
							byte[] textBytes = Encoding.GetEncoding(_encoding).GetBytes(newText);

							// Get offset
							var csvId = csvLine.ID;
							var splitParts1 = Regex.Split(csvId, "___");
							var splitParts2 = Regex.Split(splitParts1[1], "_");
							var hexOffset = splitParts2[0];
							int offset = Convert.ToInt32(hexOffset, 16);

							var importLine = new ImportLine();
							importLine.textBytes = textBytes;
							importLine.offset = offset;
							importLines.Add(importLine);
						}
					});
				}
			}
			return importLines;
		}

		public static void RepackSTCM2L(STCM2L stcm2l, List<ImportLine> importLines, string inputSTMCFile, string outputSTMCFile)
		{
			// Copy old file to new file and work on new file
			File.Copy(inputSTMCFile, outputSTMCFile, true);
			FileInfo fileInfo = new FileInfo(outputSTMCFile);
			fileInfo.IsReadOnly = false;


			using (BinaryWriter b = new BinaryWriter(File.Open(outputSTMCFile, FileMode.Open)))
			{	
				// Loop through import lines
				for (var i = 0; i < importLines.Count; i++)
				{
					var importLine = importLines[i];
					// Parse new text
					b.Seek(0, SeekOrigin.End);
					var newOffset = Convert.ToInt32(b.BaseStream.Position);

					byte[] textBytes = importLine.textBytes;
					int newTextLen = textBytes.Length + (4 - textBytes.Length % 4);
					int newTextOpCode1 = newTextLen / 4;

					// Write new text to end of file
					var zero = new UInt32(); zero = 0;
					var one = new UInt32(); one = 1;
					b.Write(zero);
					b.Write(newTextOpCode1);
					b.Write(one);
					b.Write(newTextLen);
					b.Write(textBytes);

					// Pad zero byte to text so the length is divisible by 4
					byte[] zeros = new byte[newTextLen - textBytes.Length];
					b.Write(zeros);

					// Update param value that points to text offset			
					var offset = importLine.offset;
					b.Seek(offset, SeekOrigin.Begin);
					b.Write(newOffset);
				}
			}
		}
	}
}
