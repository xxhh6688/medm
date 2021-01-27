using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MatchSystem
{
    using System.IO;
    using System.Linq.Expressions;
    using DocumentFormat.OpenXml.Spreadsheet;
    using System.Data;

    public static class ExcelHelper
    {
        public static Stream CreateExcel(string Name, IEnumerable<DataTable> sheetTable)
        {
            var doc = new MemoryStream();
            var docu = SpreadsheetDocument.Create(doc, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);

            // 这些什么什么part 什么workbook 什么sheet 的创建顺序也是神坑的存在~~~
            //
            //WorkbookPart to the document.
            WorkbookPart workbookpart = docu.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();
            var sheets = docu.WorkbookPart.Workbook.
                    AppendChild<Sheets>(new Sheets());

            foreach (var table in sheetTable)
            {
                // Add a WorksheetPart to the WorkbookPart.
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();

                // Append a new worksheet and associate it with the workbook.
                Sheet sheet = new Sheet()
                {
                    Id = docu.WorkbookPart.
                    GetIdOfPart(worksheetPart),
                    SheetId = (uint)(sheets.Count() + 1),
                    Name = table.TableName
                };
                sheets.Append(sheet);

                worksheetPart.Worksheet = new Worksheet(new SheetData());
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                var firstRow = new Row();
                for(int i = 0; i < table.Columns.Count; i++)
                {
                    Cell cell = new Cell()
                    {
                        CellReference = ConvertIntToColumnString(i+1) + (1),
                        DataType = CellValues.String,
                        CellValue = new CellValue(table.Columns[i].ColumnName)
                    };
                    firstRow.Append(cell);
                }
                sheetData.Append(firstRow);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    var newRow = new Row();
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        Cell cell = new Cell()
                        {
                            CellReference = ConvertIntToColumnString(j+1) + (i+2),
                            DataType = CellValues.String,
                            CellValue = new CellValue(table.Rows[i][j].ToString())
                        };
                        newRow.Append(cell);
                    }

                    sheetData.Append(newRow);
                }
                 worksheetPart.Worksheet.Save();
            }

            workbookpart.Workbook.Save();

            // Close the document.
            docu.Close();

            // 这里绝对是个大坑~~~
            doc.Seek(0, SeekOrigin.Begin);
            return doc;
        }
            

        public static string GetColumnStr(string reference)
        {
            var index = reference.IndexOfAny("0123456789".ToArray());
            if (index == -1)
            {
                return reference;
            }

            var chars = reference.Substring(0, index);
            return chars;
        }

        /// <summary>
        /// get column index from a cell reference
        /// </summary>
        /// <param name="cellReference"></param>
        /// <returns></returns>
        public static int GetColumnIndex(string cellReference)
        {
            var columnName = GetColumnStr(cellReference);
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName");

            columnName = columnName.ToUpperInvariant();

            int sum = 0;

            for (int i = 0; i < columnName.Length; i++)
            {
                sum *= 26;
                sum += (columnName[i] - 'A' + 1);
            }

            return sum;
        }

        /// <summary>
        /// convert the int index to a excel column string
        /// </summary>
        /// <param name="columnNumber">int index, must be grater than 0</param>
        /// <returns></returns>
        public static string ConvertIntToColumnString(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public static int GetRowIndex(string cellReference)
        {
            var index = cellReference.IndexOfAny("0123456789".ToArray());

            int res;
            if (!int.TryParse(cellReference.Remove(0, index), out res))
            {
                throw new Exception($"错误的Position{cellReference}");
            }
            return res;
        }

        public static void OpenRead(string fileName, Action<SpreadsheetDocument, WorkbookPart> actions)
        {
            var spreadsheetDocument = SpreadsheetDocument.
                Open(fileName, true);

            var workbookPart = spreadsheetDocument.WorkbookPart;
            actions(spreadsheetDocument, workbookPart);
        }

        public static void OpenRead(Stream stream, Action<SpreadsheetDocument, WorkbookPart> actions)
        {
            var spreadsheetDocument = SpreadsheetDocument.
                Open(stream, true);

            var workbookPart = spreadsheetDocument.WorkbookPart;
            actions(spreadsheetDocument, workbookPart);
        }

        public static SheetData GetDataBySheetName(WorkbookPart workbookPart, string sheetName)
        {
            var relId = workbookPart.Workbook.Descendants<Sheet>().First(s => sheetName.Equals(s.Name)).Id;
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(relId);
            return worksheetPart.Worksheet.GetFirstChild<SheetData>();
        }

        public static Cell GetCellByReference(SheetData sheet, string reference)
        {
            var cells = sheet.Descendants<Cell>();
            return cells.FirstOrDefault(c => reference.Equals(c.CellReference.Value));
        }

        public static IEnumerable<Cell> SearchCell(SharedStringTable sst, SheetData sheet, string search)
        {
            return SearchCell(sst, sheet, val => search.Equals(val));
        }

        public static IEnumerable<Cell> SearchCell(SharedStringTable sst, SheetData sheet, Expression<Func<string, bool>> predicate)
        {
            var res = new List<Cell>();
            var cells = sheet.Descendants<Cell>();
            foreach (var cell in cells)
            {
                if (predicate.Compile()(GetCellData(sst, cell)))
                {
                    res.Add(cell);
                }
            }

            return res;
        }

        public static string GetCellData(SharedStringTable sst, Cell cell)
        {
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                int ssid = int.Parse(cell.CellValue.Text);
                string str = sst.ChildElements[ssid].InnerText;
                return str;
            }
            else if (cell.CellValue != null)
            {
                return cell.CellValue.Text;
            }

            return string.Empty;
        }
    }
}