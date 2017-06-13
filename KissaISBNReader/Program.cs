using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace KissaISBNReader {
  class Program {
    static void Main(string[] args) {
      string filePath = @"C:\Users\marco\Downloads\ConLog.txt";
      string resultPath = @"C:\Users\marco\Downloads\Conreads.csv";
      Regex IsbnRegex = new Regex(@"(?<ISBN>\d+) (?<date>\d{4}-\d{2}-\d{2}) (?<time>\d{2}:\d{2}:\d{2})");

      if (args.Length > 0) {
        // Filepath has been provided as argument
        filePath = args[0];
      }

      if (args.Length > 1) {
        // another regex??
      }

      using (StreamReader fileReader = new StreamReader(filePath)) {
        using (StreamWriter resultWriter = new StreamWriter(resultPath, false)) {
          string contents = fileReader.ReadToEnd();
          resultWriter.WriteLine("Title");
          var matches = IsbnRegex.Matches(contents);
          HtmlWeb web = new HtmlWeb();

          foreach (Match match in matches) {
            var isbn = match.Groups["ISBN"].Value;
            HtmlDocument document = web.Load($"http://isbndb.com/search/all?query={isbn}");
            var result = document.DocumentNode.SelectSingleNode("//div[@class='bookSnippetBasic']/h1[@itemprop='name']");

            if (result != null) {
              var booktitle = result.InnerText.Trim();
              resultWriter.WriteLine(booktitle);
            }
            else {
              resultWriter.WriteLine($"{isbn} not found!");
            }
          }
        }
      }
      Console.WriteLine("Press any key to continue...");
      Console.ReadKey();
    }
  }
}
