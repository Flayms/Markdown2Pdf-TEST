using PuppeteerSharp.Media;

namespace Markdown2Pdf.Options;

/// <summary>
/// All the options for the conversion.
/// </summary>
public class Markdown2PdfOptions {

  /// <summary>
  /// Options that decide from where to load additional modules.
  /// </summary>
  /// <value>Default: <see cref="ModuleOptions.Remote"/>.</value>
  public ModuleOptions ModuleOptions { get; set; } = ModuleOptions.Remote;

  /// <summary>
  /// The styling to apply to the document. Default: <see cref="Theme.Github"/>.
  /// </summary>
  public Theme Theme { get; set; } = Theme.Github;

  /// <summary>
  /// The theme to use for highlighting code blocks.
  /// <value>Default: <see cref="CodeHighlightTheme.Github"/>.</value>
  /// </summary>
  public CodeHighlightTheme CodeHighlightTheme { get; set; } = CodeHighlightTheme.Github;

  /// <summary>
  /// Auto detect the language for code blocks without specfied language.
  /// <value>Default: <see langword="false"/>.</value>
  /// </summary>
  public bool EnableAutoLanguageDetection { get; set; }

  /// <summary>
  /// An HTML string to use as the document-header.
  /// </summary>
  /// <value>Default: <see langword="null"/>.</value>
  public string? HeaderHtml { get; set; }

  /// <summary>
  /// An HTML string to use as the document-footer.
  /// </summary>
  /// <value>Default: <see langword="null"/>.</value>
  public string? FooterHtml { get; set; }

  /// <summary>
  /// The title of this document. Can be injected into the header / footer by adding the class <c>document-title</c> to the element.
  /// </summary>
  /// <value>Default: <see langword="null"/>.</value>
  public string? DocumentTitle { get; set; }

  /// <summary>
  /// A <see langword="string"/> containing any content valid inside a HTML <c>&lt;head&gt;</c> 
  /// to apply extra scripting / styling to the document.
  /// </summary>
  /// <value>Default: <see langword="null"/>.</value>
  public string? CustomHeadContent { get; set; }

  /// <summary>
  /// Path to chrome or chromium executable. If set to <see langword="null"/> downloads chromium by itself.
  /// </summary>
  /// <value>Default: <see langword="null"/>.</value>
  public string? ChromePath { get; set; }

  /// <summary>
  /// Doesn't delete the HTML-file used for generating the PDF if set to <see langword="true"/>.
  /// </summary>
  /// <value>Default: <see langword="false"/>.</value>
  public bool KeepHtml { get; set; }

  /// <summary>
  /// Css-margins for the sides of the document.
  /// </summary>
  /// <value>Default: <see langword="null"/>.</value>
  public MarginOptions? MarginOptions { get; set; }

  /// <summary>
  /// Paper orientation.
  /// </summary>
  /// <value>Default: <see langword="false"/>.</value>
  public bool IsLandscape { get; set; }

  /// <summary>
  /// The paper format for the PDF.
  /// </summary>
  /// <value>Default: <see cref="PaperFormat.A4"/>.</value>
  public PaperFormat Format { get; set; } = PaperFormat.A4;

  /// <inheritdoc cref="PuppeteerSharp.PdfOptions.Scale"/>
  public decimal Scale { get; set; } = 1;

  /// <inheritdoc cref="TableOfContentsOptions"/>
  /// <value>Default: <see langword="null"/>.</value>
  public TableOfContentsOptions? TableOfContents { get; set; } = null;
}
