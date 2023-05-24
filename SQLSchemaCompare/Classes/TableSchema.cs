using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLSchemaCompare.Classes
{
    internal class TableSchema
    {
        public string TableName { get; set; }
        public List<Column> Columns { get; set; }
    }
    internal class Column
    {
        public string Name { get; set; }
    }
}
