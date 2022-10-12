using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Style;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Services
{
    public interface IExcelTemplateGenerator
    {
        Task<(MemoryStream, string, string)> GenerateTemplate(BulkImportType importType, Guid? companyId);
    }

    public class ExcelTemplateGenerator : IExcelTemplateGenerator
    {
        private readonly SpineContext _context;

        public ExcelTemplateGenerator(SpineContext context)
        {
            _context = context;
        }

        public async Task<(MemoryStream, string, string)> GenerateTemplate(BulkImportType importType, Guid? companyId)
        {
            //var companyName = await _context.Companies.First(x => x.Id == companyId).Name;
            var stream = new MemoryStream();

            using (var excelPackage = new ExcelPackage())
            {
                var workSheet = excelPackage.Workbook.Worksheets.Add("Data Sheet");

                //needed for using macros via VBACode
                excelPackage.Workbook.CreateVBAProject();

                #region Headers

                //workSheet.Cells["A1"].Value = "Company Name: " + companyName + Environment.NewLine
                //                              + "Date Created: " + DateTime.Today.ToLongDateString() + Environment.NewLine;

                //workSheet.Cells["A1"].Style.Font.Size = 12;
                //workSheet.Cells["A1"].Style.WrapText = true;
                //workSheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                //workSheet.Row(1).Height = 53.1;

                workSheet.Cells["A2"].Value = importType.GetDescription() + " Upload Template";
                workSheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Row(2).Height = 46.5;
                workSheet.Cells["A2"].Style.Font.Size = 16;
                workSheet.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells["A2"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                workSheet.Cells["2:2"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                workSheet.Row(3).Height = 28.8;

                #endregion

                var headers = GenerateColumnHeaders(workSheet, importType);
                GenerateDropDownsAndRelatedData(excelPackage, headers, _context, companyId);

                workSheet.Protection.IsProtected = false;
                workSheet.Protection.AllowSelectLockedCells = false;
                excelPackage.Workbook.Properties.Title = "ImportTemplate";

                workSheet.Cells.AutoFitColumns();
                excelPackage.SaveAs(stream);
            }

            stream.Position = 0;
            return (stream, "application/octet-stream", $"{importType}_Template.xlsm");
        }



        private List<Header> GenerateColumnHeaders(ExcelWorksheet workSheet, BulkImportType type)
        {
            List<Header> currentHeaders;
            switch (type)
            {
                case BulkImportType.Customer:
                    currentHeaders = CustomerHeaders;
                    break;
                case BulkImportType.BankTransaction:
                    currentHeaders = TransactionHeaders;
                    break;
                case BulkImportType.Product:
                    currentHeaders = ProductHeaders;
                    break;
                case BulkImportType.Services:
                    currentHeaders = ServicesHeaders;
                    break;
                case BulkImportType.PurchaseOrder:
                    currentHeaders = PurchaseOrderHeaders;
                    break;
                case BulkImportType.Subscriber:
                    currentHeaders = SubcriberHeaders;
                    break;
                case BulkImportType.IndividualVendor:
                    currentHeaders = IndividualVendorHeaders;
                    break;
                case BulkImportType.BusinessVendor:
                    currentHeaders = BusinessVendorHeaders;
                    break;
                case BulkImportType.Journal:
                    currentHeaders = JournalHeaders;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            workSheet.Cells[1, 1, 1, currentHeaders.Count].Merge = true;
            workSheet.Cells[2, 1, 2, currentHeaders.Count].Merge = true;

            for (var i = 1; i < currentHeaders.Count + 1; i++)
            {
                workSheet.Column(i).BestFit = true;
                workSheet.Cells[3, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                workSheet.Cells[3, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(i).AutoFit();

                //add a red * to required columns
                if (i - 1 < currentHeaders.Count && currentHeaders[i - 1].IsRequired)
                {
                    workSheet.Cells[3, i].IsRichText = true;
                    workSheet.Cells[3, i].RichText.Add(currentHeaders[i - 1].Name);
                    var asterisk = workSheet.Cells[3, i].RichText.Add("*");
                    asterisk.Color = Color.Red;

                }
                else
                {
                    workSheet.Cells[3, i].Value = currentHeaders[i - 1].Name;
                }
            }

            return currentHeaders;
        }

        private void GenerateDropDownsAndRelatedData(ExcelPackage excelPackage, List<Header> headers,
            SpineContext context, Guid? companyId)
        {
            var ddList = excelPackage.Workbook.Worksheets.Add("DropDownData");
            ddList.Cells["1:1"].Style.Font.Bold = true;

            GenerateDropdowns(excelPackage, headers, context, companyId);

            ddList.Hidden = eWorkSheetHidden.Hidden;
        }

        private static void GenerateDropdowns(ExcelPackage excelPackage, List<Header> headers, SpineContext context,
            Guid? companyId)
        {
            var primarySheet = excelPackage.Workbook.Worksheets[0];
            var dropDownData = excelPackage.Workbook.Worksheets[1];

            var headersWithDropdown = headers.Where(h => h.HasDropdown).OrderBy(h => h.Index).ToList();

            var columnIndex = 1;

            var macroStart = $"Private Sub Worksheet_Change(ByVal Target As Range)\r\n    ";
            var macroEnd = "Else: Exit Sub\r\n    End If\r\nEnd Sub";
            var macroCode = string.Empty;

            foreach (var header in headersWithDropdown)
            {
                var excelColumnLetters = GetColumnLetters(columnIndex);

                dropDownData.Cells[$"{excelColumnLetters}1"].Value = $"{header.Name} Options";

                MethodInfo methodInfo, genericMethod;
                object[] parameters;
                if (header.TableName.IsNullOrEmpty())
                {
                    methodInfo = typeof(ExcelTemplateGenerator).GetMethod("AddDropDownData", new[]
                    {
                        typeof(ExcelWorksheet), typeof(int)
                    });
                    genericMethod = methodInfo.MakeGenericMethod(header.DropDownDataType);
                    parameters = new object[]
                    {
                        dropDownData, columnIndex
                    };
                }
                else
                {
                    methodInfo = typeof(ExcelTemplateGenerator).GetMethod("AddTableDropDownData", new[]
                    {
                        typeof(ExcelWorksheet), typeof(int), typeof(SpineContext), typeof(Guid), typeof(string)
                    });

                    genericMethod = methodInfo; // ..MakeGenericMethod();
                    parameters = new object[]
                    {
                        dropDownData, columnIndex, context, companyId, header.TableName
                    };
                }

                var result = Convert.ToInt32(genericMethod.Invoke(genericMethod, parameters)) + 1;

                var excelFormula = $"DropDownData!${excelColumnLetters}$2:${excelColumnLetters}${result}";

                for (var i = 0; i < 500; i++)
                {
                    var row = 4 + i;
                    CreateValidation(primarySheet, header, excelFormula, row);
                }

                var macroColumnLetters = GetColumnLetters(header.Index);
                if (!macroCode.IsNullOrEmpty())
                {
                    macroCode += "    Else";
                }

                macroCode +=
                    $"If Not Intersect(Target, Range(\"{macroColumnLetters}:{macroColumnLetters}\")) Is Nothing Then\r\n    Cells(1, \"{macroColumnLetters}\").EntireColumn.AutoFit\r\n";

                columnIndex++;
            }

            //        primarySheet.CodeModule.Code = macroStart + macroCode + macroEnd;

        }


        //this method needs to be public for Type.GetMethod to work the way it is called in GenerateDropdowns
        public static int AddDropDownData<TEnum>(ExcelWorksheet dropDownData, int column) where TEnum : struct, Enum
        {
            var enums = Enum.GetValues(typeof(TEnum));
            for (var i = 0; i < enums.Length; i++)
            {
                var value = ((TEnum) enums.GetValue(i)).GetDescription();
                if (value.IsNullOrEmpty())
                {
                    value = ((TEnum) enums.GetValue(i)).ToString();
                }

                dropDownData.Cells[i + 2, column].Value = value;
            }

            return enums.Length;
        }

        //this method needs to be public for Type.GetMethod to work the way it is called in GenerateDropdowns
        public static int AddTableDropDownData(ExcelWorksheet dropDownData, int column, SpineContext context,
            Guid? companyId, string tableName)
        {
            var dataLength = 0;
            switch (tableName)
            {
                case "ProductCategory":
                {
                    var data = context.ProductCategories
                        .Where(x => x.CompanyId == companyId && !x.IsServiceCategory && !x.IsDeleted)
                        .Select(x => new TableModel
                        {
                            //   Id = x.Id,
                            Item = x.Name
                        }).ToList();

                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "Inventory":
                {
                    var data = context.Inventories.Where(x => x.CompanyId == companyId
                                                              && x.InventoryType == InventoryType.Product 
                                                              && x.Status == InventoryStatus.Active
                                                              && !x.IsDeleted)
                        .Select(x => new TableModel
                        {
                            //   Id = x.Id,
                            Item = x.Name
                        }).ToList();

                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "Gender":
                {
                    var data = new List<TableModel>
                    {
                        new TableModel {Item = "Male"},
                        new TableModel {Item = "Female"},
                    };

                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "OperatingSector":
                {
                    var data = context.OperatingSectors
                        .Select(x => new TableModel
                        {
                            //   Id = x.Id,
                            Item = x.Sector
                        }).ToList();

                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "BusinessTypes":
                {
                    var data = context.BusinessTypes
                        .Select(x => new TableModel
                        {
                            //   Id = x.Id,
                            Item = x.Type
                        }).ToList();

                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "Vendor":
                {
                    var data = context.Vendors
                        .Where(x=>x.CompanyId == companyId && !x.IsDeleted && x.Status == Status.Active)
                        .Select(x => new TableModel
                        {
                            //   Id = x.Id,
                            Item = x.Name + " - " + x.DisplayName
                        }).ToList();

                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "Role":
                {
                    var data = context.Roles
                        .Where(x =>  (x.CompanyId == companyId || x.IsSystemDefined)
                                     && !x.IsDeleted)
                        .Select(x => new TableModel
                        {
                            //   Id = x.Id,
                            Item = x.Name
                        }).ToList();

                    for (var i = 0; i < data.Count; i++)
                    {
                        data[i].Item = data[i].Item.GetFirstPart();
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "Bank":
                {
                    var data = context.Banks
                        .Select(x => new TableModel
                        {
                            //   Id = x.Id,
                            Item = x.BankName
                        }).ToList();

                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "LedgerAccount":
                {
                    var data = context.LedgerAccounts
                        .Select(x => new TableModel
                        {
                            //   Id = x.Id,
                            Item = x.AccountName
                        }).ToList();

                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }
                case "YesNo":
                {
                    var data = new List<TableModel>
                    {
                        new TableModel {Item = "True"},
                        new TableModel {Item = "False"}
                    };
                    for (var i = 0; i < data.Count; i++)
                    {
                        dropDownData.Cells[i + 2, column].Value = data[i].Item;
                    }

                    dataLength = data.Count;
                    break;
                }

            }
            
            return dataLength;
        }

        public static string GetColumnLetters(int index)
        {
            //65 = ascii code for letter A, first letter of the excel worksheet
            var dividend = index;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (int) ((dividend - modulo) / 26);
            }

            return columnName;
        }

        private static void CreateValidation(ExcelWorksheet worksheet, Header header, string excelFormula, int row)
        {
            var columnIndex = header.Index;
            var cellAddress = worksheet.Cells[row, columnIndex].Address;
            var validation = worksheet.DataValidations.AddListValidation(cellAddress);
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
            validation.ErrorTitle = "An invalid value was entered";
            validation.Error = "Select a value from the list";
            validation.Formula.ExcelFormula = excelFormula;
        }

        private static readonly List<Header> CustomerHeaders = new()
        {
            new Header("Customer Name", 1, true),
            new Header("Email Address", 2, true),
            new Header("Phone Number Country Code", 3, true),
            new Header("Phone Number", 4, true),
            new Header("Gender", 5, true, tableName: "Gender"),
            new Header("Tax Identification Number", 6),
            new Header("Business Name", 7, true),
            new Header("Operating Sector", 8, true, tableName: "OperatingSector"),
            new Header("Billing Address Line 1", 9),
            new Header("Billing Address Line 2", 10),
            new Header("Billing State", 11),
            new Header("Billing Country", 12),
            new Header("Billing Postal Code", 13),
            new Header("Shipping Address Line 1", 14),
            new Header("Shipping Address Line 2", 15),
            new Header("Shipping State", 16),
            new Header("Shipping Country", 17),
            new Header("Shipping Postal Code", 18)
        };
        
        private static readonly List<Header> IndividualVendorHeaders = new()
        {
            new Header("Full Name", 1, true),
            new Header("Display Name", 2, true),
            new Header("Email Address", 3, true),
            new Header("Phone Number Country Code", 4, true),
            new Header("Phone Number", 5, true),
            new Header("Operating Sector", 6, true, tableName: "OperatingSector"),
            new Header("Billing Address Line 1", 7),
            new Header("Billing Address Line 2", 8),
            new Header("Billing State", 9),
            new Header("Billing Country", 10),
            new Header("Billing Postal Code", 11),
            new Header("Shipping Address Line 1", 12),
            new Header("Shipping Address Line 2", 13),
            new Header("Shipping State", 14),
            new Header("Shipping Country", 15),
            new Header("Shipping Postal Code", 16),
            new Header("Contact Person Name", 17),
            new Header("Contact Person Role", 18),
            new Header("Contact Person Email", 19),
            new Header("Contact Person Phone Country Code", 20),
            new Header("Contact Person Phone", 21),
            new Header("Bank Name", 22, tableName: "Bank"),
            new Header("Account Name", 23),
            new Header("Account Number", 24)
        };
        
        private static readonly List<Header> BusinessVendorHeaders = new()
        {
            new Header("Full Name", 1, true),
            new Header("Display Name", 2, true),
            new Header("Email Address", 3, true),
            new Header("Phone Number Country Code", 4, true),
            new Header("Phone Number", 5, true),
            new Header("Operating Sector", 6, true, tableName: "OperatingSector"),
            new Header("Business Name", 7, true),
            new Header("Tax Identification Number", 8),
            new Header("RC Number", 9),
            new Header("Website", 10),
            new Header("Billing Address Line 1", 11),
            new Header("Billing Address Line 2", 12),
            new Header("Billing State", 13),
            new Header("Billing Country", 14),
            new Header("Billing Postal Code", 15),
            new Header("Shipping Address Line 1", 16),
            new Header("Shipping Address Line 2", 17),
            new Header("Shipping State", 18),
            new Header("Shipping Country", 19),
            new Header("Shipping Postal Code", 20),
            new Header("Contact Person Name", 21),
            new Header("Contact Person Role", 22),
            new Header("Contact Person Email", 23),
            new Header("Contact Person Phone Country Code", 24),
            new Header("Contact Person Phone", 25),
            new Header("Bank Name", 26, tableName: "Bank"),
            new Header("Account Name", 27),
            new Header("Account Number", 28)
        };

        private static readonly List<Header> ProductHeaders = new()
        {
            new Header("Category", 1, true, tableName: "ProductCategory"),
            new Header("Name", 2, true),
            new Header("Description", 3),
            new Header("Serial Number", 4, true),
            new Header("Quantity", 5, true),
            new Header("Inventory Date", 6, true),
            new Header("Cost Price", 7, true),
            new Header("Sales Price", 8, true),
            new Header("Stock Keeping Unit", 9),
            new Header("Reorder Level", 10, true)
        };

        private static readonly List<Header> ServicesHeaders = new()
        {
            new Header("Name", 1, true),
            new Header("Description", 2),
            new Header("Sales Price", 3, true)
        };

        private static readonly List<Header> TransactionHeaders = new()
        {
            new Header("Transaction Date", 1, true),
            new Header("Description", 2, true),
            new Header("Reference Number", 3, true),
            new Header("Amount Spent", 4),
            new Header("Amount Received", 5),
            new Header("Payee", 6, true),
            new Header("Cheque Number", 7)
        };

        private static readonly List<Header> PurchaseOrderHeaders = new()
        {
            new Header("Vendor", 1, true, tableName: "Vendor"),
            new Header("Product", 2, true, tableName: "Inventory"),
            new Header("Quantity", 3, true),
            new Header("Rate", 4, true),
            new Header("Order Date", 5, true),
            new Header("Expected Date", 6)
        };

        private static readonly List<Header> JournalHeaders = new()
        {
            new Header("Journal Date", 1, true),
            new Header("Product Name", 2, true),
            new Header("Description", 3),
            new Header("Cash Based", 4, tableName: "YesNo"),
            new Header("Ledger Account", 5, true, tableName: "LedgerAccount"),
            new Header("Item Description ", 6),
            new Header("Debit ", 7, true),
            new Header("Credit ", 8, true)
        };
        
        private static readonly List<Header> SubcriberHeaders = new()
        {
            new Header("First Name", 1, true),
            new Header("Last Name", 2, true),
            new Header("Other Name", 3),
            new Header("Phone Number", 4, true),
            new Header("Business Name", 5, true),
            new Header("Operating Sector", 6, true, tableName: "OperatingSector"),
            new Header("Business Type", 7, true, tableName: "BusinessTypes"),
            new Header("Gender", 8, tableName: "Gender"),
            new Header("Date Of Birth", 9),
            new Header("Email", 10, true),
            new Header("TIN", 11),
            new Header("Ref_ReferralCode", 12)
            //new Header("Billing Address Line 1", 13),
            //new Header("Billing Address Line 2", 14),
            //new Header("Billing State", 15),
            //new Header("Billing Country", 16),
            //new Header("Billing Postal Code", 17),
            //new Header("Shipping Address Line 1", 18),
            //new Header("Shipping Address Line 2", 19),
            //new Header("Shipping State", 20),
            //new Header("Shipping Country", 21),
            //new Header("Shipping Postal Code", 22)
        };
    }

    public class TableModel
    {
        //public Guid Id { get; set; }
        public string Item { get; set; }
    }

    public class Header
    {
        public string Name { get; set; }

        //Not zero-indexed
        public int Index { get; set; }

        public bool IsRequired { get; set; }

        public bool HasDropdown { get; set; }

        public string TableName { get; set; }

        public Type DropDownDataType { get; set; }

        public Header(string name, int index, bool isRequired = false, Type dropDownDataType = null,
            string tableName = "")
        {
            Name = name;
            Index = index;
            IsRequired = isRequired;
            HasDropdown = dropDownDataType != null || tableName != "";
            DropDownDataType = dropDownDataType;
            TableName = tableName;
        }
    }
}