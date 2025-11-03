#!/usr/bin/env python3
"""
Convert markdown file to PDF using markdown and xhtml2pdf
"""
import sys
import os
from pathlib import Path

try:
    import markdown
    from xhtml2pdf import pisa
    from io import BytesIO
except ImportError as e:
    print(f"Error: Required packages not installed. Please run:")
    print(f"  pip3 install markdown xhtml2pdf")
    sys.exit(1)

def markdown_to_pdf(md_file_path, pdf_file_path):
    """Convert markdown file to PDF"""
    
    # Read markdown file
    with open(md_file_path, 'r', encoding='utf-8') as f:
        md_content = f.read()
    
    # Convert markdown to HTML
    html_content = markdown.markdown(
        md_content,
        extensions=['extra', 'tables', 'codehilite', 'toc']
    )
    
    # Add CSS styling for better PDF appearance
    css_style = """
    @page {
        size: A4;
        margin: 2cm;
    }
    body {
        font-family: Helvetica, Arial, sans-serif;
        font-size: 11pt;
        line-height: 1.6;
        color: #333;
    }
    h1 {
        font-size: 24pt;
        margin-top: 0.5em;
        margin-bottom: 0.5em;
        page-break-after: avoid;
        border-bottom: 2px solid #333;
        padding-bottom: 0.3em;
    }
    h2 {
        font-size: 18pt;
        margin-top: 1em;
        margin-bottom: 0.5em;
        page-break-after: avoid;
        border-bottom: 1px solid #666;
        padding-bottom: 0.2em;
    }
    h3 {
        font-size: 14pt;
        margin-top: 0.8em;
        margin-bottom: 0.4em;
        page-break-after: avoid;
    }
    h4 {
        font-size: 12pt;
        margin-top: 0.6em;
        margin-bottom: 0.3em;
    }
    table {
        border-collapse: collapse;
        width: 100%;
        margin: 1em 0;
        page-break-inside: avoid;
    }
    table th, table td {
        border: 1px solid #ddd;
        padding: 8px;
        text-align: left;
    }
    table th {
        background-color: #f2f2f2;
        font-weight: bold;
    }
    code {
        background-color: #f4f4f4;
        padding: 2px 4px;
        font-family: "Courier New", monospace;
        font-size: 10pt;
    }
    pre {
        background-color: #f4f4f4;
        padding: 1em;
        overflow-x: auto;
        page-break-inside: avoid;
    }
    pre code {
        background-color: transparent;
        padding: 0;
    }
    ul, ol {
        margin: 0.5em 0;
        padding-left: 2em;
    }
    li {
        margin: 0.3em 0;
    }
    blockquote {
        border-left: 4px solid #ddd;
        padding-left: 1em;
        margin: 1em 0;
        color: #666;
    }
    hr {
        border: none;
        border-top: 1px solid #ddd;
        margin: 2em 0;
    }
    """
    
    # Wrap HTML content with CSS
    full_html = f"""
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset="utf-8">
        <title>Enterprise Maturity Assessment</title>
        <style>
        {css_style}
        </style>
    </head>
    <body>
        {html_content}
    </body>
    </html>
    """
    
    # Convert HTML to PDF using xhtml2pdf
    try:
        with open(pdf_file_path, 'w+b') as pdf_file:
            pisa_status = pisa.CreatePDF(
                BytesIO(full_html.encode('utf-8')),
                dest=pdf_file
            )
            if pisa_status.err:
                print(f"✗ Error converting to PDF: {pisa_status.err}")
                return False
        print(f"✓ Successfully converted {md_file_path} to {pdf_file_path}")
        return True
    except Exception as e:
        print(f"✗ Error converting to PDF: {e}")
        return False

if __name__ == "__main__":
    md_file = "docs/ENTERPRISE_MATURITY_ASSESSMENT.md"
    pdf_file = "docs/ENTERPRISE_MATURITY_ASSESSMENT.pdf"
    
    if not os.path.exists(md_file):
        print(f"Error: {md_file} not found")
        sys.exit(1)
    
    success = markdown_to_pdf(md_file, pdf_file)
    sys.exit(0 if success else 1)

