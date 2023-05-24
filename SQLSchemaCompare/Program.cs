using System;
using System.Data;
using System.Data.SqlClient;
using SQLSchemaCompare.Classes;

Console.Write("Enter in data source for master: ");
string dataSourceOne = Console.ReadLine();
Console.Write("Enter in database name for master: ");
string dataBaseNameOne = Console.ReadLine();
Console.Write("Enter in user id for master: ");
string userIdOne = Console.ReadLine();
Console.Write("Enter in password for master: ");
string passwordOne = Console.ReadLine();
string connectionStringMaster = "Data Source="+ dataSourceOne + ";Initial Catalog=" + dataBaseNameOne + ";User Id=" + userIdOne + ";Password=" + passwordOne;


Console.Write("Enter in data source for second database: ");
string dataSourceTwo = Console.ReadLine();
Console.Write("Enter in database name for second database: ");
string dataBaseNameTwo = Console.ReadLine();
Console.Write("Enter in user id for second database: ");
string userIdTwo = Console.ReadLine();
Console.Write("Enter in password for second database: ");
string passwordTwo = Console.ReadLine();
string connectionStringSecond = "Data Source=" + dataSourceTwo + ";Initial Catalog=" + dataBaseNameTwo + ";User Id=" + userIdTwo + ";Password=" + passwordTwo;

List<TableSchema> prodTableNames = new List<TableSchema>();
List<TableSchema> devTableNames = new List<TableSchema>();
string colFileName = DateTime.Now.ToString("yyyy-dd-mm-hh-mm") + "-" + dataBaseNameOne + "_COMPARE_Columns.csv";
string tableFileName = DateTime.Now.ToString("yyyy-dd-mm-hh-mm") +"-" + dataBaseNameOne + "_COMPARE_Tables.csv";
if (File.Exists(colFileName))
{
    using (var sw = new StreamWriter(colFileName, true))
    {
        sw.WriteLine("TableName,ColumnName,Exists");
    }
}
else
{
    File.Create(colFileName).Close();
    using (var sw = new StreamWriter(colFileName, true))
    {
        sw.WriteLine("TableName,ColumnName,Exists");
    }
}
if (File.Exists(tableFileName))
{
    using (var sw = new StreamWriter(tableFileName, true))
    {
        sw.WriteLine("TableName,Exists");
    }
}
else
{
    File.Create(tableFileName).Close();
    using (var sw = new StreamWriter(tableFileName, true))
    {
        sw.WriteLine("TableName,Exists");
    }
}

using (SqlConnection connProd = new SqlConnection(connectionStringMaster))
{
    Console.WriteLine("Getting Columns from Database #1");
    connProd.Open();
    DataTable tables = connProd.GetSchema("Tables");
    
    foreach (DataRow table in tables.Rows)
    {
        List<Column> columnNames = new List<Column>();
        DataTable dtCols = connProd.GetSchema("Columns", new[] { dataBaseNameOne, null, table[2].ToString() });
        foreach (DataRow row in dtCols.Rows)
        {
            columnNames.Add(new Column
            {
                Name = row[3].ToString()
            });
        }
        prodTableNames.Add(new TableSchema
        {
            TableName = table[2].ToString(),
            Columns = columnNames
        }) ;
        
    }
    Console.Clear();
    connProd.Close();
}

using (SqlConnection connDev = new SqlConnection(connectionStringSecond))
{
    Console.WriteLine("Getting Columns from Database #2");
    connDev.Open();
    DataTable tables = connDev.GetSchema("Tables");

    foreach (DataRow table in tables.Rows)
    {
        List<Column> columnNames = new List<Column>();
        DataTable dtCols = connDev.GetSchema("Columns", new[] { dataBaseNameTwo, null, table[2].ToString() });
        foreach (DataRow row in dtCols.Rows)
        {
            columnNames.Add(new Column
            {
                Name = row[3].ToString()
            });
        }
        devTableNames.Add(new TableSchema
        {
            TableName = table[2].ToString(),
            Columns = columnNames
        });

    }
    Console.Clear();
    connDev.Close();


    

}
foreach (var prodTable in prodTableNames)
{
    Console.Write("Comparing " + prodTable.TableName);
    //Step 1: Verify the table exists in dev
    var devTable = from a in devTableNames where a.TableName == prodTable.TableName select a;
    bool tableExists = false;
    if (devTable.Count() > 0)
    {
        tableExists = true;
        //Step 2: Compare the columns in each table
        foreach (var prodTableCol in prodTable.Columns)
        {
            bool colExists = false;
            var devCol = from a in devTable.FirstOrDefault().Columns where a.Name == prodTableCol.Name select a;
            if (devCol.Count() > 0)
                colExists = true;
            using (var sw = new StreamWriter(colFileName, true))
            {
                sw.WriteLine(prodTable.TableName + "," + prodTableCol.Name + "," + colExists);
            }
        }
    }
    using (var sw = new StreamWriter(tableFileName, true))
    {
        sw.WriteLine(prodTable.TableName + "," + tableExists);
    }
    Console.Clear();
}
Console.WriteLine("Complete! Press any key to quit. Check output file for results.");
Console.ReadKey();