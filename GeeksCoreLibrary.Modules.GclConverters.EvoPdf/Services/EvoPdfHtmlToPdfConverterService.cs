using System.Runtime.InteropServices;
using System.Text;
using EvoPdf.Chromium;
using GeeksCoreLibrary.Core.Helpers;
using GeeksCoreLibrary.Core.Models;
using GeeksCoreLibrary.Modules.Databases.Interfaces;
using GeeksCoreLibrary.Modules.GclConverters.Enums;
using GeeksCoreLibrary.Modules.GclConverters.Models;
using GeeksCoreLibrary.Modules.GclConverters.Services;
using GeeksCoreLibrary.Modules.GclReplacements.Interfaces;
using GeeksCoreLibrary.Modules.Objects.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GeeksCoreLibrary.Modules.GclConverters.EvoPdf.Services;

public class EvoPdfHtmlToPdfConverterService : HtmlToPdfConverterService
{
    private readonly GclSettings gclSettings;
    private readonly IObjectsService objectsService;
    private readonly IStringReplacementsService stringReplacementsService;
    private readonly IHttpContextAccessor? httpContextAccessor;

    public EvoPdfHtmlToPdfConverterService(IDatabaseConnection databaseConnection,
        IObjectsService objectsService,
        IStringReplacementsService stringReplacementsService,
        IOptions<GclSettings> gclSettings,
        IHttpContextAccessor? httpContextAccessor = null,
        IWebHostEnvironment? webHostEnvironment = null)
        : base(objectsService, databaseConnection, webHostEnvironment)
    {
        this.objectsService = objectsService;
        this.stringReplacementsService = stringReplacementsService;
        this.httpContextAccessor = httpContextAccessor;
        this.gclSettings = gclSettings.Value;

        // Check if EvoPdf is loaded, otherwise throw an exception.
        // We load Evo PDF with PrivateAssets = true, so it won't be automatically loaded in projects that use the GCL.
        // This is because Evo PDF has separate packages for Windows and Linux, and we can't properly detect which one they need from here.
        var evoPdfType = Type.GetType("EvoPdf.Chromium.HtmlToPdfConverter, EvoPdf.Chromium");
        if (evoPdfType == null)
        {
            throw new InvalidOperationException("EvoPdf is not loaded. Please ensure you have added the correct NuGet package: Either 'EvoPdf.Chromium.Windows' for Windows or 'EvoPdf.Chromium.Linux' for Linux.");
        }
    }

    /// <inheritdoc />
    public override async Task<FileContentResult> ConvertHtmlStringToPdfAsync(HtmlToPdfRequestModel settings)
    {
         var htmlToConvert = new StringBuilder(settings.Html);
         var httpContext = httpContextAccessor?.HttpContext;
         var converter = new HtmlToPdfConverter
         {
             LicenseKey = gclSettings.EvoPdfLicenseKey,
             ConversionDelay = 2
         };

         if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
         {
             // For some reason the default path doesn't work on Linux.
             // Note: This also requires the command "chmod +x /app/evopdf_loadhtml" to be run on the server.
             converter.HtmlLoaderFilePath = "/app/evopdf_loadhtml";
         }

         if (!settings.Orientation.HasValue)
         {
             var orientationValue = await objectsService.FindSystemObjectByDomainNameAsync("pdf_orientation");
             settings.Orientation = orientationValue.Equals("landscape", StringComparison.OrdinalIgnoreCase) ? PageOrientation.Landscape : PageOrientation.Portrait;
         }

         converter.PdfDocumentOptions.PdfPageOrientation = settings.Orientation.Value switch
         {
             PageOrientation.Portrait => PdfPageOrientation.Portrait,
             PageOrientation.Landscape => PdfPageOrientation.Landscape,
             _ => throw new ArgumentOutOfRangeException(nameof(settings.Orientation), settings.Orientation.Value.ToString(), null)
         };

         _ = Int32.TryParse(await objectsService.FindSystemObjectByDomainNameAsync("pdf_html_viewer_width"), out var htmlViewerWidth);
         _ = Int32.TryParse(await objectsService.FindSystemObjectByDomainNameAsync("pdf_html_viewer_height"), out var htmlViewerHeight);
         _ = Int32.TryParse(await objectsService.FindSystemObjectByDomainNameAsync("pdf_margins"), out var margins);

         // Main document options.
         var avoidTextBreak = (await objectsService.FindSystemObjectByDomainNameAsync("pdf_avoid_text_break")).Equals("true", StringComparison.OrdinalIgnoreCase);
         var avoidImageBreak = (await objectsService.FindSystemObjectByDomainNameAsync("pdf_avoid_image_break", "true")).Equals("true", StringComparison.OrdinalIgnoreCase);
         htmlToConvert.Insert(0, $$"""
                                   <style>
                                   	* {
                                   		break-inside: {{(avoidTextBreak ? "avoid" : "auto")}};
                                   	}

                                   	img {
                                           break-inside: {{(avoidImageBreak ? "avoid" : "auto")}};
                                   	}
                                   </style>
                                   """);

         converter.PdfDocumentOptions.BottomMargin = margins;
         converter.PdfDocumentOptions.LeftMargin = margins;
         converter.PdfDocumentOptions.RightMargin = margins;
         converter.PdfDocumentOptions.TopMargin = margins;

         // Page size.
         var pageSize = await objectsService.FindSystemObjectByDomainNameAsync("pdf_pagesize", "A4");
         if (pageSize == "CUSTOM")
         {
             _ = Int32.TryParse(await objectsService.FindSystemObjectByDomainNameAsync("pdf_pagesize_width"), out var pageSizeWidth);
             _ = Int32.TryParse(await objectsService.FindSystemObjectByDomainNameAsync("pdf_pagesize_height"), out var pageSizeHeight);

             converter.PdfDocumentOptions.PdfPageSize = new PdfPageSize(pageSizeWidth, pageSizeHeight);
         }
         else
         {
             converter.PdfDocumentOptions.AutoResizePdfPageWidth = false;
             converter.PdfDocumentOptions.PdfPageSize = pageSize switch
             {
                 "A0" => PdfPageSize.A0,
                 "A1" => PdfPageSize.A1,
                 "A2" => PdfPageSize.A2,
                 "A3" => PdfPageSize.A3,
                 "A4" => PdfPageSize.A4,
                 "A5" => PdfPageSize.A5,
                 "A6" => PdfPageSize.A6,
                 "A7" => PdfPageSize.A7,
                 "A8" => PdfPageSize.A8,
                 "A9" => PdfPageSize.A9,
                 "A10" => PdfPageSize.A10,
                 _ => PdfPageSize.A4
             };
         }

         if (htmlViewerWidth > 0) converter.HtmlViewerWidth = htmlViewerWidth;
         if (htmlViewerHeight > 0) converter.HtmlViewerHeight = htmlViewerHeight;

         // Header settings.
         converter.PdfDocumentOptions.EnableHeaderFooter = (await objectsService.FindSystemObjectByDomainNameAsync("pdf_header_show")).Equals("true", StringComparison.OrdinalIgnoreCase) || (await objectsService.FindSystemObjectByDomainNameAsync("pdf_footer_show")).Equals("true", StringComparison.OrdinalIgnoreCase);
         if (String.IsNullOrWhiteSpace(settings.Header))
         {
             settings.Header = await objectsService.FindSystemObjectByDomainNameAsync("pdf_header_text");
         }

         if (!String.IsNullOrWhiteSpace(settings.Header))
         {
             converter.PdfDocumentOptions.HeaderTemplate = settings.Header;
         }

         // Footer settings.
         if (String.IsNullOrWhiteSpace(settings.Footer))
         {
             settings.Footer = await objectsService.FindSystemObjectByDomainNameAsync("pdf_footer_text");
         }

         if (!String.IsNullOrWhiteSpace(settings.Footer))
         {
             converter.PdfDocumentOptions.FooterTemplate = settings.Footer;
         }

         // Security settings.
         converter.PdfSecurityOptions.CanEditContent = (await objectsService.FindSystemObjectByDomainNameAsync("pdf_can_edit_content", "false")).Equals("true", StringComparison.OrdinalIgnoreCase);
         converter.PdfSecurityOptions.CanCopyContent = (await objectsService.FindSystemObjectByDomainNameAsync("pdf_can_copy_content", "true")).Equals("true", StringComparison.OrdinalIgnoreCase);
         converter.PdfSecurityOptions.OwnerPassword = await objectsService.FindSystemObjectByDomainNameAsync("pdf_password");

         if (String.IsNullOrWhiteSpace(converter.PdfSecurityOptions.OwnerPassword))
         {
             converter.PdfSecurityOptions.OwnerPassword = SecurityHelpers.GenerateRandomPassword();
         }
         else
         {
             converter.PdfSecurityOptions.OwnerPassword = await stringReplacementsService.DoAllReplacementsAsync(converter.PdfSecurityOptions.OwnerPassword);
         }

         if (settings.ItemId > 0UL && !String.IsNullOrWhiteSpace(settings.BackgroundPropertyName))
         {
             var backgroundImage = await RetrieveBackgroundImageAsync(settings.ItemId, settings.BackgroundPropertyName);

             if (!String.IsNullOrWhiteSpace(backgroundImage))
             {
                 htmlToConvert.Insert(0, $$"""
                                           <style>
                                             html {
                                                 width: 100%;
                                                 height: 100%;
                                                 margin: 0;
                                                 padding: 0;
                                             }

                                           	body {
                                                 width: 100%;
                                                 height: 100%;
                                           		margin: 0;
                                           		padding: 0;
                                           		background-image: url('{{backgroundImage}}');
                                           		background-size: cover; /* Ensure the image covers the full page */
                                           		background-repeat: repeat-y;
                                           		background-position: top left;
                                           	}
                                           </style>
                                           """);
             }
         }

         // Set additional document options.
         var options = String.IsNullOrWhiteSpace(settings.DocumentOptions) ? new Dictionary<string, string>() : settings.DocumentOptions.Split(';').Where(o => o.Contains(":")).ToDictionary(o => o.Split(':')[0], o => o.Split(':')[1], StringComparer.OrdinalIgnoreCase);

         foreach (var p in typeof(HtmlToPdfConverter).GetProperties())
         {
             if (!options.ContainsKey(p.Name))
             {
                 continue;
             }

             p.SetValue(converter, Convert.ChangeType(options[p.Name], p.PropertyType), null);
         }

         foreach (var p in typeof(PdfDocumentOptions).GetProperties())
         {
             if (!options.ContainsKey(p.Name))
             {
                 continue;
             }

             p.SetValue(converter.PdfDocumentOptions, Convert.ChangeType(options[p.Name], p.PropertyType), null);
         }

         var baseUri = httpContext == null ? null : HttpContextHelpers.GetBaseUri(httpContext).ToString();
         baseUri = await objectsService.FindSystemObjectByDomainNameAsync("pdf_baseurl_override", baseUri);
         
         var output = converter.ConvertHtml(htmlToConvert.ToString(), baseUri);
         var fileResult = new FileContentResult(output, "application/pdf")
         {
             FileDownloadName = EnsureCorrectFileName(settings.FileName)
         };

         return fileResult;
    }
}