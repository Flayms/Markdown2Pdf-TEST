﻿using Markdig;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Reflection;
using System.Linq;
using Markdown2Pdf.Options;
using System.Collections.Generic;

namespace Markdown2Pdf;

/// <summary>
/// Use this to parse markdown to PDF.
/// </summary>
public class Markdown2PdfConverter {

  /// <summary>
  /// All the options this converter uses for generating the PDF.
  /// </summary>
  public Markdown2PdfOptions Options { get; }

  //todo: one instead of 2 dics
  //todo: better way to keep versions in sync
  //todo: implement
  private readonly IReadOnlyDictionary<string, string> _packageLocationsWeb = new Dictionary<string, string>() {
    {"mathjaxPath",  "https://cdn.jsdelivr.net/npm/mathjax@3" },
    {"mermaidPath",  "https://cdn.jsdelivr.net/npm/mermaid@10.2.3" }
  };

  //the first half of the path gets added in the constructor, depending on the user-settings
  private readonly IReadOnlyDictionary<string, string> _packageLocationsLocal = new Dictionary<string, string>() {
    {"mathjaxPath",  "mathjax" },
    {"mermaidPath",  "mermaid" }
  };

  /// <summary>
  /// Instantiate a new <see cref="Markdown2PdfConverter"/>.
  /// </summary>
  /// <param name="options">Optional options to specify how to convert the markdown.</param>
  public Markdown2PdfConverter(Markdown2PdfOptions? options = default) {
    this.Options = options ?? new Markdown2PdfOptions();

    var moduleOptions = this.Options.ModuleOptions;

    //adjust local dictionary paths
    if (moduleOptions.ModuleLocation != ModuleLocation.Remote) {
      var path = moduleOptions.ModulePath!;

      var updatedDic = new Dictionary<string, string>();

      foreach (var kvp in this._packageLocationsLocal) {
        var key = kvp.Key;
        var value = Path.Combine(path, kvp.Value);
        updatedDic[key] = value;
      }

      this._packageLocationsLocal = updatedDic;
    }
  }

  /// <inheritdoc cref="Convert(FileInfo, FileInfo)"/>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  /// <returns>The newly created PDF-file.</returns>
  public FileInfo Convert(FileInfo markdownFile) => new FileInfo(this.Convert(markdownFile.FullName));

  /// <summary>
  /// Converts the given markdown-file to PDF.
  /// </summary>
  /// <param name="markdownFile"><see cref="FileInfo"/> containing the markdown.</param>
  /// <param name="outputFile"><see cref="FileInfo"/> for saving the generated PDF.</param>
  public void Convert(FileInfo markdownFile, FileInfo outputFile) => this.Convert(markdownFile.FullName, outputFile.FullName);

  /// <inheritdoc cref="Convert(string, string)"/>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  /// <returns>Filepath to the generated pdf.</returns>
  public string Convert(string markdownFilePath) {
    var markdownDir = Path.GetDirectoryName(markdownFilePath);
    var outputFileName = Path.GetFileNameWithoutExtension(markdownFilePath) + ".pdf";
    var outputFilePath = Path.Combine(markdownDir, outputFileName);
    this.Convert(markdownFilePath, outputFilePath);

    return outputFilePath;
  }

  /// <summary>
  /// Converts the given markdown-file to PDF.
  /// </summary>
  /// <param name="markdownFilePath">Path to the markdown file.</param>
  /// <param name="outputFilePath">File path for saving the PDF to.</param>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  public void Convert(string markdownFilePath, string outputFilePath) {
    markdownFilePath = Path.GetFullPath(markdownFilePath);
    outputFilePath = Path.GetFullPath(outputFilePath);

    var markdownContent = File.ReadAllText(markdownFilePath);

    var html = this._GenerateHtml(markdownContent);

    //todo: make temp-file
    var markdownDir = Path.GetDirectoryName(markdownFilePath);
    var htmlPath = Path.Combine(markdownDir, "converted.html");
    File.WriteAllText(htmlPath, html);

    var task = this._GeneratePdfAsync(htmlPath, outputFilePath, Path.GetFileNameWithoutExtension(markdownFilePath));
    task.Wait();

    if (!this.Options.KeepHtml)
      File.Delete(htmlPath);
  }

  private string _GenerateHtml(string markdownContent) {
    //todo: decide on how to handle pipeline better
    var pipeline = new MarkdownPipelineBuilder()
      .UseAdvancedExtensions()
      .UseDiagrams()
      .Build();
    //.UseSyntaxHighlighting();
    var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

    //todo: support more plugins
    //todo: code-color markup

    var assembly = Assembly.GetAssembly(typeof(Markdown2PdfConverter));
    var currentLocation = Path.GetDirectoryName(assembly.Location);
    var templateHtmlResource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("ContentTemplate.html"));

    string templateHtml;

    using (Stream stream = assembly.GetManifestResourceStream(templateHtmlResource))
    using (StreamReader reader = new StreamReader(stream)) {
      templateHtml = reader.ReadToEnd();
    }

    //create model for templating html
    var templateModel = new Dictionary<string, string>();

    //load correct module paths
    if (this.Options.ModuleOptions.ModuleLocation == ModuleLocation.Remote) {
      foreach (var kvp in this._packageLocationsWeb)
        templateModel.Add(kvp.Key, kvp.Value);
    }
    else {
      foreach (var kvp in this._packageLocationsLocal)
        templateModel.Add(kvp.Key, kvp.Value);
    }

    templateModel.Add("body", htmlContent);

    return TemplateFiller.FillTemplate(templateHtml, templateModel);
  }

  private async Task _GeneratePdfAsync(string htmlFilePath, string outputFilePath, string title) {
    using var browser = await this._CreateBrowserAsync();
    var page = await browser.NewPageAsync();

    await page.GoToAsync(htmlFilePath, WaitUntilNavigation.Networkidle2);

    var marginOptions = new PuppeteerSharp.Media.MarginOptions();
    if (this.Options.MarginOptions != null) {
      //todo: remove double initialization
      marginOptions = new PuppeteerSharp.Media.MarginOptions {
        Top = this.Options.MarginOptions.Top,
        Bottom = this.Options.MarginOptions.Bottom,
        Left = this.Options.MarginOptions.Left,
        Right = this.Options.MarginOptions.Right,
      };
    }

    var pdfOptions = new PdfOptions {
      //todo: make this settable
      Format = PaperFormat.A4,
      PrintBackground = true,
      MarginOptions = marginOptions
    };

    //todo: error handling
    //todo: default header is super small
    if (this.Options.HeaderUrl != null) {
      var headerContent = File.ReadAllText(this.Options.HeaderUrl);

      //todo: super hacky, rather replace class content
      //todo: create setting and only use fileName as fallback
      headerContent = headerContent.Replace("title", title);
      pdfOptions.HeaderTemplate = headerContent;
      pdfOptions.DisplayHeaderFooter = true;
    }

    if (this.Options.FooterUrl != null) {
      var footerContent = File.ReadAllText(this.Options.FooterUrl);
      footerContent = footerContent.Replace("title", title);
      pdfOptions.FooterTemplate = footerContent;
      pdfOptions.DisplayHeaderFooter = true;
    }

    await page.EmulateMediaTypeAsync(MediaType.Screen);
    await page.PdfAsync(outputFilePath, pdfOptions);
  }

  private async Task<IBrowser> _CreateBrowserAsync() {
    var launchOptions = new LaunchOptions {
      Headless = true,
      Args = new[] {
        "--no-sandbox" //todo: check why this is needed
      },
    };

    if (this.Options.ChromePath == null) {
      using var browserFetcher = new BrowserFetcher();
      Console.WriteLine("Downloading chromium...");
      await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
    } else
      launchOptions.ExecutablePath = this.Options.ChromePath;

    return await Puppeteer.LaunchAsync(launchOptions);
  }

}