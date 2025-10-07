# GeeksCoreLibrary.Modules.GclConverters.EvoPdf

## Repository Metadata
- **Type**: Library / Extension Module
- **Language(s)**: C#
- **Framework(s)**: .NET 9.0
- **Database**: None (uses parent GCL database connection)
- **Status**: Production / Active Maintenance
- **Critical Level**: High (PDF generation is critical for document workflows)
- **Last Updated**: October 2025 (Version 5.3.2508.1)
- **Repository**: https://github.com/happy-geeks/geeks-core-library-evopdf
- **License**: GNU GPL v3
- **NuGet Package**: GeeksCoreLibrary.Modules.GclConverters.EvoPdf

## Purpose & Context

### What This Repository Does
This is a plugin library for the GeeksCoreLibrary (GCL) that implements HTML-to-PDF conversion functionality using the EvoPdf.Chromium library. It provides a production-ready implementation of the `IHtmlToPdfConverterService` interface, enabling Wiser CMS applications to generate PDF documents from HTML content with full support for modern web standards, CSS, and JavaScript rendering.

The library handles complex PDF generation scenarios including:
- Multi-page document generation with configurable page sizes and orientations
- Custom headers and footers with dynamic content
- Background images and watermarks
- Security controls (password protection, edit/copy permissions)
- Configurable margins, page breaks, and layout options
- Cross-platform support (Windows and Linux)

### Business Value
PDF generation is a critical feature for Wiser CMS customers who need to:
- Generate invoices, quotes, and order confirmations
- Create product catalogs and specification sheets
- Export configurator designs as PDF documents
- Produce legal documents with specific formatting requirements
- Generate reports with complex layouts and styling

This library provides a high-quality, commercially-supported PDF rendering engine that produces professional documents matching the exact appearance of web pages rendered in Chrome/Chromium browsers.

### Wiser Ecosystem Role
This library extends the GeeksCoreLibrary with concrete PDF generation functionality:

**Dependencies:**
- **GeeksCoreLibrary** (Core platform): Provides the `IHtmlToPdfConverterService` interface, database connections, object service, and string replacements
- **EvoPdf.Chromium.Windows/Linux**: Commercial PDF rendering engine (not included, must be added by consuming applications)

**Integration Pattern:**
1. GCL defines the `IHtmlToPdfConverterService` interface with a default null implementation
2. This library provides a production implementation using EvoPdf
3. Applications add this library via NuGet and register it during dependency injection setup
4. Applications must separately license and include the appropriate EvoPdf.Chromium package for their target OS

**Configuration Source:**
All PDF generation settings are loaded from the Wiser database via the `IObjectsService` (system objects pattern), allowing runtime configuration without code changes.

## Architecture & Design

### High-Level Architecture
```
┌─────────────────────────────────────────────────────────┐
│           Wiser Application / API                        │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Controllers / Services                          │   │
│  │  - Inject IHtmlToPdfConverterService             │   │
│  └──────────────────┬───────────────────────────────┘   │
│                     │                                    │
│  ┌──────────────────▼───────────────────────────────┐   │
│  │  EvoPdfHtmlToPdfConverterService                 │   │
│  │  (This Library)                                  │   │
│  │                                                   │   │
│  │  • Loads config from database                    │   │
│  │  • Builds EvoPdf converter instance              │   │
│  │  • Applies settings (margins, size, security)    │   │
│  │  • Handles platform differences (Win/Linux)      │   │
│  └──────┬────────────────────────────┬──────────────┘   │
│         │                            │                   │
│  ┌──────▼────────┐          ┌────────▼────────────┐     │
│  │ GeeksCoreLib  │          │  EvoPdf.Chromium    │     │
│  │ Services      │          │  (Commercial Lib)   │     │
│  │               │          │                     │     │
│  │ • IObjects    │          │  • HTML Rendering   │     │
│  │ • IStrReplace │          │  • PDF Generation   │     │
│  │ • IDatabase   │          │  • Chromium Engine  │     │
│  └───────────────┘          └─────────────────────┘     │
└─────────────────────────────────────────────────────────┘
```

### Key Design Patterns

**1. Interface Implementation Pattern**
The library implements the `IHtmlToPdfConverterService` interface defined in GeeksCoreLibrary, following the Interface Segregation and Dependency Inversion principles. This allows applications to swap PDF converters without code changes.

**2. Extension Method Registration**
Uses the standard ASP.NET Core service collection extension pattern for clean dependency injection:
```csharp
builder.Services.AddEvoPdfHtmlToPdfConverterService();
```

**3. Configuration-as-Data Pattern**
All conversion settings are retrieved from the database via `IObjectsService` rather than hardcoded or stored in config files. This enables:
- Per-tenant/per-environment configuration
- Runtime changes without deployment
- Customer-specific PDF styling
- Dynamic configuration via Wiser UI

**4. Template Method Pattern**
Inherits from the base `HtmlToPdfConverterService` class which provides common functionality like background image retrieval and filename sanitization, while overriding the core conversion method.

**5. Private Assets Pattern**
Uses NuGet's `PrivateAssets="all"` feature to avoid forcing downstream dependencies on specific EvoPdf packages, giving consuming applications control over which platform-specific package to use.

### Technology Decisions

**Why EvoPdf?**
- Uses Chromium rendering engine for maximum HTML5/CSS3 compatibility
- Produces pixel-perfect PDFs matching browser rendering
- Supports modern web features (flexbox, grid, custom fonts, SVG)
- Commercial support and regular updates
- Better rendering quality than alternatives like wkhtmltopdf or PdfSharp

**Why .NET 9.0?**
- Matches GeeksCoreLibrary target framework
- Latest performance improvements and language features
- Long-term support (LTS release)

**Why Separate Windows/Linux Packages?**
EvoPdf.Chromium has platform-specific native dependencies. The library uses MSBuild conditions and version ranges to handle this complexity, allowing the same source code to work on both platforms.

**Technical Trade-offs:**
- **Pro**: High-quality PDF output with complex HTML/CSS support
- **Pro**: Chromium-based rendering is predictable and well-documented
- **Con**: Commercial license required for EvoPdf (not free)
- **Con**: Larger deployment footprint due to Chromium binaries
- **Con**: Requires platform-specific executable permissions on Linux

## Installation & Setup

### Prerequisites
- **.NET 9.0 SDK** or later
- **GeeksCoreLibrary** v5.3.2508.1 or compatible version
- **EvoPdf.Chromium License** - Commercial license required (contact EvoPdf)
- **EvoPdf.Chromium.Windows** (for Windows deployment) or **EvoPdf.Chromium.Linux** (for Linux deployment)
- **Wiser Database** with required system objects (see Configuration section)

### Installation

**Step 1: Install the NuGet package**
```bash
dotnet add package GeeksCoreLibrary.Modules.GclConverters.EvoPdf
```

**Step 2: Install platform-specific EvoPdf package**

For Windows:
```bash
dotnet add package EvoPdf.Chromium.Windows
```

For Linux:
```bash
dotnet add package EvoPdf.Chromium.Linux
```

For cross-platform builds (recommended), add this to your `.csproj`:
```xml
<!-- Default PackageReference when no RuntimeIdentifier is specified -->
<ItemGroup Condition="'$(RuntimeIdentifier)' == '' AND $([MSBuild]::IsOsPlatform('Windows'))">
    <PackageReference Include="EvoPdf.Chromium.Windows" Version="11.4.5" />
</ItemGroup>

<ItemGroup Condition="'$(RuntimeIdentifier)' == '' AND $([MSBuild]::IsOsPlatform('Linux'))">
    <PackageReference Include="EvoPdf.Chromium.Linux" Version="11.4.1" />
</ItemGroup>

<!-- Windows-specific dependency -->
<ItemGroup Condition="'$(RuntimeIdentifier)' == 'win-x64'">
    <PackageReference Include="EvoPdf.Chromium.Windows" Version="11.4.5" />
</ItemGroup>

<!-- Linux-specific dependency -->
<ItemGroup Condition="'$(RuntimeIdentifier)' == 'linux-x64'">
    <PackageReference Include="EvoPdf.Chromium.Linux" Version="11.4.1" />
</ItemGroup>
```

**Step 3: Register the service**

In your application's `Program.cs` or `Startup.cs`, add the EvoPdf service **after** adding GCL services:

```csharp
// Add GeeksCoreLibrary services first
builder.Services.AddGclServices(builder.Configuration);

// Then add EvoPdf implementation
builder.Services.AddEvoPdfHtmlToPdfConverterService();
```

### Configuration

#### Required Configuration: EvoPdf License Key
Add your EvoPdf license key to `appsettings.json`:

```json
{
  "GclSettings": {
    "EvoPdfLicenseKey": "your-evopdf-license-key-here"
  }
}
```

#### Database Configuration (System Objects)
The service loads PDF settings from the Wiser database via system objects. The following system objects (by domain name) control PDF generation behavior:

| System Object Key | Purpose | Default Value | Valid Values |
|------------------|---------|---------------|--------------|
| `pdf_orientation` | Page orientation | `portrait` | `portrait`, `landscape` |
| `pdf_html_viewer_width` | HTML viewport width (px) | *(none)* | Integer (e.g., `1024`) |
| `pdf_html_viewer_height` | HTML viewport height (px) | *(none)* | Integer (e.g., `768`) |
| `pdf_margins` | Margin size (all sides, px) | `0` | Integer (e.g., `40`) |
| `pdf_avoid_text_break` | Prevent text from breaking across pages | `false` | `true`, `false` |
| `pdf_avoid_image_break` | Prevent images from breaking across pages | `true` | `true`, `false` |
| `pdf_pagesize` | Page size | `A4` | `A0`-`A10`, `CUSTOM` |
| `pdf_pagesize_width` | Custom page width (if `pdf_pagesize=CUSTOM`) | *(none)* | Integer (points) |
| `pdf_pagesize_height` | Custom page height (if `pdf_pagesize=CUSTOM`) | *(none)* | Integer (points) |
| `pdf_header_show` | Enable header | `false` | `true`, `false` |
| `pdf_header_text` | Header HTML template | *(none)* | HTML string |
| `pdf_footer_show` | Enable footer | `false` | `true`, `false` |
| `pdf_footer_text` | Footer HTML template | *(none)* | HTML string |
| `pdf_can_edit_content` | Allow PDF editing | `false` | `true`, `false` |
| `pdf_can_copy_content` | Allow text copying | `true` | `true`, `false` |
| `pdf_password` | Owner/edit password | *(auto-generated)* | String (supports replacements) |
| `pdf_baseurl_override` | Override base URL for resources | *(current URL)* | Full URL (e.g., `https://example.com`) |

**Example: Setting up basic PDF configuration via SQL:**
```sql
-- Set default orientation to landscape
INSERT INTO wiser_system_objects (domain_name, value)
VALUES ('pdf_orientation', 'landscape')
ON DUPLICATE KEY UPDATE value = 'landscape';

-- Set margins to 40px
INSERT INTO wiser_system_objects (domain_name, value)
VALUES ('pdf_margins', '40')
ON DUPLICATE KEY UPDATE value = '40';

-- Enable footer with page numbers
INSERT INTO wiser_system_objects (domain_name, value)
VALUES ('pdf_footer_show', 'true')
ON DUPLICATE KEY UPDATE value = 'true';

INSERT INTO wiser_system_objects (domain_name, value)
VALUES ('pdf_footer_text', '<div style="text-align: center; font-size: 10pt;">Page <span class="pageNumber"></span> of <span class="totalPages"></span></div>')
ON DUPLICATE KEY UPDATE value = '<div style="text-align: center; font-size: 10pt;">Page <span class="pageNumber"></span> of <span class="totalPages"></span></div>';
```

### Platform-Specific Setup

#### Linux Deployment (Docker/Ubuntu)
On Linux, the EvoPdf loader executable requires execute permissions. Add this to your Dockerfile or deployment script:

```dockerfile
# In your Dockerfile
RUN chmod +x /app/evopdf_loadhtml
```

Or manually:
```bash
chmod +x /app/evopdf_loadhtml
```

The service automatically detects Linux and sets the correct path:
```csharp
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    converter.HtmlLoaderFilePath = "/app/evopdf_loadhtml";
}
```

#### macOS M1/ARM Development
For local development on Apple Silicon:
- EvoPdf does not provide native ARM packages
- Use Rosetta 2 emulation for x64 .NET runtime
- Or develop against a Linux Docker container

```bash
# Run in x64 mode on M1 Mac
dotnet run --arch x64
```

### Usage Example

#### Basic Usage
```csharp
public class InvoiceController : Controller
{
    private readonly IHtmlToPdfConverterService _pdfConverter;

    public InvoiceController(IHtmlToPdfConverterService pdfConverter)
    {
        _pdfConverter = pdfConverter;
    }

    public async Task<IActionResult> DownloadInvoice(int invoiceId)
    {
        // Generate your HTML content
        var htmlContent = await GenerateInvoiceHtmlAsync(invoiceId);

        // Create PDF request
        var request = new HtmlToPdfRequestModel
        {
            Html = htmlContent,
            FileName = $"Invoice-{invoiceId}.pdf",
            Orientation = PageOrientation.Portrait
        };

        // Convert to PDF
        var pdfResult = await _pdfConverter.ConvertHtmlStringToPdfAsync(request);

        return pdfResult; // Returns FileContentResult
    }
}
```

#### Advanced Usage with Custom Settings
```csharp
public async Task<IActionResult> GenerateQuote(int quoteId)
{
    var htmlContent = await GenerateQuoteHtmlAsync(quoteId);

    var request = new HtmlToPdfRequestModel
    {
        Html = htmlContent,
        FileName = $"Quote-{quoteId}.pdf",
        Orientation = PageOrientation.Landscape,

        // Custom header with dynamic content
        Header = @"<div style='font-size: 10pt; text-align: right;'>
                      Quote #{quoteId} - <span class='date'></span>
                   </div>",

        // Custom footer with page numbers
        Footer = @"<div style='font-size: 9pt; text-align: center;'>
                      Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                   </div>",

        // Background image for watermark/letterhead
        ItemId = quoteId,
        BackgroundPropertyName = "company_letterhead",

        // Override EvoPdf converter properties
        DocumentOptions = "ConversionDelay:3;HtmlViewerWidth:1200"
    };

    var pdfResult = await _pdfConverter.ConvertHtmlStringToPdfAsync(request);
    return pdfResult;
}
```

## Core Functionality

### Main Components

#### EvoPdfHtmlToPdfConverterService
- **Purpose**: Implements HTML-to-PDF conversion using EvoPdf.Chromium engine
- **Location**: `/Services/EvoPdfHtmlToPdfConverterService.cs`
- **Base Class**: Inherits from `HtmlToPdfConverterService` (GCL)
- **Key Method**: `ConvertHtmlStringToPdfAsync(HtmlToPdfRequestModel settings)`

**Key Responsibilities:**
1. Validates EvoPdf library is loaded (throws exception if missing)
2. Loads all configuration from database via `IObjectsService`
3. Builds and configures `HtmlToPdfConverter` instance
4. Applies platform-specific settings (Linux path override)
5. Injects CSS for page break control and background images
6. Processes custom document options from request
7. Converts HTML to PDF bytes
8. Returns `FileContentResult` ready for download

**Constructor Dependencies:**
- `IDatabaseConnection` - Database access
- `IObjectsService` - Loads configuration from database
- `IStringReplacementsService` - Processes dynamic content in settings
- `IOptions<GclSettings>` - Access to EvoPdf license key
- `IHttpContextAccessor` - Gets base URL for resource loading (optional)
- `IWebHostEnvironment` - Environment information (optional)

### Conversion Process Flow

1. **Initialization**
   - Constructor checks EvoPdf library is loaded via reflection
   - Throws `InvalidOperationException` if EvoPdf.Chromium not found
   - Initializes dependencies

2. **Configuration Loading** (from database)
   - Page orientation (portrait/landscape)
   - Page size (A4, A5, custom dimensions, etc.)
   - Margins (top, right, bottom, left)
   - HTML viewer dimensions
   - Header/footer templates
   - Security settings
   - Page break behavior

3. **HTML Preprocessing**
   - Inject CSS for page break control (avoid text/image breaks)
   - Add background image CSS if specified
   - Process string replacements in content

4. **Converter Configuration**
   - Create `HtmlToPdfConverter` instance
   - Set license key
   - Configure Linux-specific path if needed
   - Apply all settings from database
   - Override with request-specific settings

5. **Document Options Processing**
   - Parse `DocumentOptions` string (semicolon-separated key:value pairs)
   - Use reflection to set properties on `HtmlToPdfConverter` and `PdfDocumentOptions`
   - Example: `"ConversionDelay:3;HtmlViewerWidth:1200"`

6. **PDF Generation**
   - Determine base URL for resource loading
   - Call `converter.ConvertHtml(html, baseUri)`
   - Returns byte array of PDF content

7. **Result Packaging**
   - Wrap PDF bytes in `FileContentResult`
   - Set content type: `application/pdf`
   - Set download filename (sanitized)
   - Return for streaming to client

## Troubleshooting

### Common Issues

#### Issue: "EvoPdf is not loaded" Exception
- **Symptoms**: `InvalidOperationException` thrown when first PDF is requested
- **Cause**: EvoPdf.Chromium.Windows or .Linux package not installed in consuming application
- **Solution**: Add the appropriate EvoPdf package reference to your project (see Installation section)

#### Issue: "Permission denied" on Linux
- **Symptoms**: PDF generation fails with permission error on Linux
- **Cause**: EvoPdf loader executable `/app/evopdf_loadhtml` doesn't have execute permissions
- **Solution**: Run `chmod +x /app/evopdf_loadhtml` in your Docker container or deployment script

#### Issue: Invalid License Key Error
- **Symptoms**: PDF generation fails with license validation error
- **Cause**: License key missing or incorrect in `appsettings.json`
- **Solution**: Verify `GclSettings:EvoPdfLicenseKey` is set correctly; contact EvoPdf for license issues

## Known Issues & Technical Debt

### Current Limitations

**1. No Unit Tests**
- The repository has a test workflow but no actual test files
- **Impact**: Changes require manual testing
- **Recommendation**: Add integration tests with mock EvoPdf converter

**2. Configuration Loading Performance**
- 15+ database queries per PDF conversion
- **Impact**: Adds latency for high-volume generation
- **Recommendation**: Implement configuration caching layer

**3. Linux Path Hardcoded**
- `/app/evopdf_loadhtml` path is hardcoded
- **Impact**: May not work with non-standard deployments
- **Recommendation**: Make path configurable via system object

---

*This documentation is part of the Wiser Platform knowledge base maintained by Happy Horizon B.V.*