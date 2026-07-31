using System.Collections.Generic;

namespace MyNavicat.Api.Models.DTOs
{
    public class ColumnSchemaDto
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string? DefaultValue { get; set; }
        public string? Comment { get; set; }
    }

    public class IndexSchemaDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsUnique { get; set; }
        public List<string> Columns { get; set; } = new();
    }

    public class TableSchemaDto
    {
        public string TableName { get; set; } = string.Empty;
        public List<ColumnSchemaDto> Columns { get; set; } = new();
        public List<IndexSchemaDto> Indexes { get; set; } = new();
    }
}
