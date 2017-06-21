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
      readmode mode = readmode.conmode;

      if (args.Length > 0) {
        // Filepath has been provided as argument
        filePath = args[0];
      }

      if (args.Length > 1) {
        mode = readmode.conmode;

        if (args[1].ToLower().Contains("sparql")) {
          mode = readmode.sparqlmode;
        }
        else if (args[1].ToLower().Contains("literal")) {
          mode = readmode.literal;
        }
      }

      if (args.Length > 2) {
        resultPath = args[2];
      }

      using (StreamWriter resultWriter = new StreamWriter(resultPath, false)) {
        using (StreamReader fileReader = new StreamReader(filePath)) {
          string contents = fileReader.ReadToEnd();
          if (mode == readmode.sparqlmode) {
            IsbnRegex = new Regex(@"\>http://opendata.mangakissa.nl/collection/nodes#LOC=(?<location>.*?)&amp;DATTIME=(?<date>\d+?)&amp;ISBN=(?<ISBN>\d+)\<");
          }
          else if (mode == readmode.literal) {
            IsbnRegex = new Regex(@"\>(?<ISBN>\d+)\<");
          }
          else {
            IsbnRegex = new Regex(@"(?<ISBN>\d+) (?<date>\d{4}-\d{2}-\d{2}) (?<time>\d{2}:\d{2}:\d{2})");
          }

          resultWriter.WriteLine("Title|Count");
          var matches = IsbnRegex.Matches(contents);

          int lineCounter = 0;
          var previousIsbn = "";
          foreach (Match match in matches) {
            var isbn = match.Groups["ISBN"].Value;

            // Con users are idios and keep scanning the same book? 
            if (isbn == previousIsbn && mode == readmode.conmode) {
              Console.WriteLine($"line {lineCounter++}: Doubles previous entry");
            }
            else {
              var bookTitle = getBookTitle(countdictionary, titleDictionary, lineCounter, isbn);
              previousIsbn = isbn;
              Console.WriteLine($"line {lineCounter++}: {bookTitle}");
            }
          }
        }

        foreach (var isbn in titleDictionary.Keys) {
          resultWriter.WriteLine($"{titleDictionary[isbn]}|{countdictionary[isbn]}");
        }
      }

      Console.WriteLine("Press any key to continue...");
      Console.ReadKey();
    }

    private static string getBookTitle(Dictionary<string, int> countdictionary, Dictionary<string, string> titleDictionary, int lineCounter, string isbn) {
      var bookTitle = $"{isbn} not found!";
      if (!titleDictionary.ContainsKey(isbn)) {
        try {
          HtmlWeb web = new HtmlWeb();
          //HtmlDocument document = web.Load($"http://isbndb.com/search/all?query={isbn}");
          //var result = document.DocumentNode.SelectSingleNode("//div[@class='bookSnippetBasic']/h1[@itemprop='name']");

          //HtmlDocument document = web.Load($"https://isbnsearch.org/isbn/{isbn}");
          //var result = document.DocumentNode.SelectSingleNode("//div[@class='bookinfo']/h1");

          //HtmlDocument document = web.Load($"https://www.bookfinder.com/search/?isbn={isbn}&mode=advanced&st=sr&ac=qr");
          //var result = document.DocumentNode.SelectSingleNode("//div[@class='attributes']/div/a//span[@itemprop='name']");

          HtmlDocument document = web.Load(
            $@"https://collectie.mangakissa.nl/sparql?default-graph-uri=&query=SELECT+distinct+%3Fz%0D%0AWHERE%0D%0A%7B+%3Fx+%3Chttp%3A%2F%2Fopendata.mangakissa.nl%2Fcollection%2Fproperties%23hasISBN%3E+%22{isbn}%22+.%0D%0A++%3Fx+%3Chttp%3A%2F%2Fopendata.mangakissa.nl%2Fcollection%2Fproperties%23hasTitle%3E+%3Fz%0D%0A+%7D&format=text%2Fhtml&timeout=0&debug=on");
          var result = document.DocumentNode.SelectSingleNode("//td");
          if (result != null) {
            bookTitle = WebUtility.HtmlDecode(result.InnerText.Trim().Trim('"').Trim());
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

      return bookTitle;
    }
  }

  enum readmode {
    conmode,
    sparqlmode,
    literal
  }
}
