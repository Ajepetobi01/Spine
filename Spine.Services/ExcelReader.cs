using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Spine.Common.Converters;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Spine.Services
{
    public interface IExcelReader
    {
        Task<DataTable> ReadExcelFile(Stream fileStream);
        string DataTableToJSONWithJSONNet(DataTable table);
        string SerializeDataTableToJSON(DataTable table);
        Task<Stream> GenerateExcel(DataTable data, string templateName);
    }

    public class ExcelReader : IExcelReader
    {
        public async Task<Stream> GenerateExcel(DataTable data, string templateName)
        {
            try
            {
                var outputStream = new MemoryStream();
                var workbook = new XLWorkbook();

                var worksheet = workbook.Worksheets.Add(data, templateName);
                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(outputStream);

                return outputStream;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DataTable> ReadExcelFile(Stream fileStream)
        {
            try
            {
                var dt = new DataTable();
                using (var workbook = new XLWorkbook(fileStream))
                {
                    var worksheet = workbook.Worksheet(1); //Worksheets.ElementAt(0);
                    string readRange = "1:1";

                    var rowsUsed = worksheet.RowsUsed();
                    var headerRowPosition = 1; //1;
                    for (int i = headerRowPosition; i <= rowsUsed.Count(); i++) // column header is starting from row 3
                    {
                        var row = rowsUsed.ElementAtOrDefault(i);
                        if (row != null)
                        {
                            if (i == headerRowPosition)
                            {
                                readRange = $"{headerRowPosition}:{row.LastCellUsed().Address.ColumnNumber}";
                                foreach (var cell in row.Cells(readRange))
                                {
                                    // stripping the spaces off to get the property name
                                    var columnName = new string(cell.Value.ToString().Where(c => c != '*' && !char.IsWhiteSpace(c)).ToArray());
                                    dt.Columns.Add(columnName);
                                }
                            }
                            else
                            {
                                dt.Rows.Add();
                                int cellIndex = 0;
                                foreach (var cell in row.Cells(readRange))
                                {
                                    dt.Rows[dt.Rows.Count - 1][cellIndex] = cell.Value.ToString();
                                    cellIndex++;
                                }
                            }
                        }
                    }
                }
                return dt;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public string SerializeDataTableToJSON(DataTable table)
        {
            var options = new JsonSerializerOptions
            {
                //  IgnoreNullValues = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new DateTimeConverterFactory());
            options.Converters.Add(new DataTableJsonConverter());
            
            return System.Text.Json.JsonSerializer.Serialize(table, options); // JsonConvert.SerializeObject(table);
        }
        
        public string DataTableToJSONWithJSONNet(DataTable table)
        {
            return JsonConvert.SerializeObject(table);
        }
    }

    public class MyCustomDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime?>
    {
        /// <summary>
        /// DateTime format
        /// </summary>
        private readonly string Format = "dd/MM/yyyy hh:mm";

        public override DateTime? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            DateTime result;
            if (DateTime.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return null;
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime? value, System.Text.Json.JsonSerializerOptions options)
        {
            if (value == null) return;

            writer.WriteStringValue(((DateTime)value).ToString(Format));
        }
    }

    public class NewCustomDateTimeConverter : DateTimeConverterBase
    {
        public NewCustomDateTimeConverter()
        {

        }
        /// <summary>
        /// DateTime format
        /// </summary>
        private readonly string Format = "dd/MM/yyyy hh:mm";

        /// <summary>
        /// Writes value to JSON
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="value">Value to be written</param>
        /// <param name="serializer">JSON serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;

            writer.WriteValue(((DateTime)value).ToString(Format));
        }

        /// <summary>
        /// Reads value from JSON
        /// </summary>
        /// <param name="reader">JSON reader</param>
        /// <param name="objectType">Target type</param>
        /// <param name="existingValue">Existing value</param>
        /// <param name="serializer">JSON serialized</param>
        /// <returns>Deserialized DateTime</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            var s = reader.Value.ToString();
            DateTime result;
            if (DateTime.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return null;
        }
    }
}
