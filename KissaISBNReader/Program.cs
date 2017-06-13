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
      Dictionary<string, int> countdictionary = new Dictionary<string, int>();
      Dictionary<string, string> titleDictionary = new Dictionary<string, string>();


      if (args.Length > 0) {
        // Filepath has been provided as argument
        filePath = args[0];
      }

      if (args.Length > 1) {
        // another regex??
      }

      using (StreamWriter resultWriter = new StreamWriter(resultPath, false)) {
        using (StreamReader fileReader = new StreamReader(filePath)) {
          string contents = fileReader.ReadToEnd();
          resultWriter.WriteLine("Title|Count");
          var matches = IsbnRegex.Matches(contents);
        
          int lineCounter = 0;

          foreach (Match match in matches) {
            var isbn = match.Groups["ISBN"].Value;
            var bookTitle = $"{isbn} not found!";

            if (!titleDictionary.ContainsKey(isbn)) {
              try {

                //HtmlDocument document = web.Load($"http://isbndb.com/search/all?query={isbn}");
                //var result = document.DocumentNode.SelectSingleNode("//div[@class='bookSnippetBasic']/h1[@itemprop='name']");

                //HtmlDocument document = web.Load($"https://isbnsearch.org/isbn/{isbn}");
                //var result = document.DocumentNode.SelectSingleNode("//div[@class='bookinfo']/h1");
                HtmlWeb web = new HtmlWeb();
                HtmlDocument document = web.Load($"https://www.bookfinder.com/search/?isbn={isbn}&mode=advanced&st=sr&ac=qr");
                var result = document.DocumentNode.SelectSingleNode("//div[@class='attributes']/div/a//span[@itemprop='name']");


                if (result != null) {
                  bookTitle = result.InnerText.Trim();
                }
              }
              catch (Exception) {
                Console.WriteLine($"Error ocurred on line {lineCounter}");
              }
              titleDictionary.Add(isbn, bookTitle);
              countdictionary.Add(isbn, 1);
            }
            else {
              countdictionary[isbn] = countdictionary[isbn] + 1;
              bookTitle = titleDictionary[isbn];
            }

            Console.WriteLine($"line {lineCounter++}: {bookTitle}");
          }
        }

        foreach (var isbn in titleDictionary.Keys) {
          resultWriter.WriteLine($"{titleDictionary[isbn]}|{countdictionary[isbn]}");
        }
      }

      Console.WriteLine("Press any key to continue...");
      Console.ReadKey();
    }
  }
}
