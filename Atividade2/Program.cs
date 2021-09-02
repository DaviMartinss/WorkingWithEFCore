using System;
using System.Xml; 
using System.IO; 
using System.Linq; 
using System.Text.Json; 
using Microsoft.EntityFrameworkCore;

using static System.Console;
using static System.IO.Path;
using static System.Environment;

namespace Atividade2
{
    class Program
    {
      static void Main(string[] args)
      {

        WriteLine("Criando três arquivos com todas as categorias e produtos serializados");

        using (var db = new Northwind())
        {

          IQueryable<Category> cats = db.Categories.Include(c => c.Products);

          GenerateXmlFile(cats, useAttributes: false);
          GenerateCsvFile(cats);
          GenerateJsonFile(cats);
          }
      }


      private delegate void WriteDataDelegate(string name, string value);

      private static void GenerateXmlFile(IQueryable<Category> cats, bool useAttributes = true)
      {

        string which = useAttributes ? "attibutes" : "elements";

        string xmlFile = $"categories-and-products-using-{which}.xml";

        using (FileStream xmlStream = File.Create(
          Combine(CurrentDirectory, xmlFile)))
        {
          using (XmlWriter xml = XmlWriter.Create(xmlStream,
            new XmlWriterSettings { Indent = true }))
          {

            WriteDataDelegate writeMethod;

            if (useAttributes)
            {
              writeMethod = xml.WriteAttributeString;
            }
            else
            {
              writeMethod = xml.WriteElementString;
            }

            xml.WriteStartDocument();

            xml.WriteStartElement("categories");

            foreach (Category c in cats)
            {
              xml.WriteStartElement("category");
              writeMethod("id", c.CategoryID.ToString());
              writeMethod("name", c.CategoryName);
              writeMethod("desc", c.Description);
              writeMethod("product_count", c.Products.Count.ToString());
              xml.WriteStartElement("products");

              foreach (Product p in c.Products)
              {
                xml.WriteStartElement("product");

                writeMethod("id", p.ProductID.ToString());
                writeMethod("name", p.ProductName);
                writeMethod("cost", p.Cost.Value.ToString());
                writeMethod("stock", p.Stock.ToString());
                writeMethod("discontinued", p.Discontinued.ToString());

                xml.WriteEndElement(); 
              }
              xml.WriteEndElement(); 
              xml.WriteEndElement(); 
            }
            xml.WriteEndElement(); 
          }
      }

      WriteLine("{0} contains {1:N0} bytes.",
        arg0: xmlFile,
        arg1: new FileInfo(xmlFile).Length);
      }

      private static void GenerateCsvFile(IQueryable<Category> cats)
      {
        string csvFile = "categories-and-products.csv";

        using (FileStream csvStream = File.Create(Combine(CurrentDirectory, csvFile)))
        {
          using (var csv = new StreamWriter(csvStream)) 
          {

            csv.WriteLine("CategoryID,CategoryName,Description,ProductID,ProductName,Cost,Stock,Discontinued");
          
            foreach (Category c in cats)
            {
              foreach (Product p in c.Products)
              {
                csv.Write("{0},\"{1}\",\"{2}\",",
                  arg0: c.CategoryID.ToString(),
                  arg1: c.CategoryName,
                  arg2: c.Description);

                csv.Write("{0},\"{1}\",{2},",
                  arg0: p.ProductID.ToString(),
                  arg1: p.ProductName,
                  arg2: p.Cost.Value.ToString());

                csv.WriteLine("{0},{1}",
                  arg0: p.Stock.ToString(),
                  arg1: p.Discontinued.ToString());
              }
            }
          }
        }

        WriteLine("{0} contains {1:N0} bytes.",
          arg0: csvFile,
          arg1: new FileInfo(csvFile).Length);
      }
    

      private static void GenerateJsonFile(IQueryable<Category> cats)
      {
        string jsonFile = "categories-and-products.json";

        using (FileStream jsonStream = File.Create(Combine(CurrentDirectory, jsonFile))) 
        {
          using (var json = new Utf8JsonWriter(jsonStream,
            new JsonWriterOptions { Indented = true })) 
          {
            json.WriteStartObject();
            json.WriteStartArray("categories"); 

            foreach (Category c in cats)
            {
              json.WriteStartObject(); 

              json.WriteNumber("id", c.CategoryID);
              json.WriteString("name", c.CategoryName);
              json.WriteString("desc", c.Description);
              json.WriteNumber("product_count", c.Products.Count);

              json.WriteStartArray("products");

              foreach (Product p in c.Products)
              {
                json.WriteStartObject();

                json.WriteNumber("id", p.ProductID);
                json.WriteString("name", p.ProductName);
                json.WriteNumber("cost", p.Cost.Value);
                json.WriteNumber("stock", p.Stock.Value);
                json.WriteBoolean("discontinued", p.Discontinued);

                json.WriteEndObject();
              }
              json.WriteEndArray(); 

              json.WriteEndObject();
            }
            json.WriteEndArray(); 
            json.WriteEndObject();
          }
        }

        WriteLine("{0} contains {1:N0} bytes.",
          arg0: jsonFile, 
          arg1: new FileInfo(jsonFile).Length); 

      }

    } 
}
