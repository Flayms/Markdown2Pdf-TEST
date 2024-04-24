# Run this to generate the documentation files for the Github_Wiki

function Get-AllDuplicateIds($markdownContent) {
    $idPattern = '(?<=<a id=").+?(?=">)'
    $idTable = @{}
    # Write-Warning "markdown-content: $markdownContent"

    # Find all matches of the pattern in the content
    $idMatches = [regex]::Matches($markdownContent, $idPattern)

    # Add each match to the hashtable, incrementing the count if it already exists
    foreach ($match in $idMatches) {
        if ($idTable.ContainsKey($match.Value)) {
            $idTable[$match.Value]++
        } else {
            $idTable[$match.Value] = 1
        }
    }

    $duplicateIds = $idTable.GetEnumerator() | Where-Object { $_.Value -gt 1 } | ForEach-Object { $_.Key }
    return $duplicateIds
}

# Cleanup markdown files for Github Wiki
function CleanMarkdown($markdownContent) {
    [regex]$removeLinkExtensionRegex = "(?<=\[.*\]\(.*).md(?=\))"
    [regex]$removeStartOfXrefRegex = "<xref href=""(\w+\.)*(?=\w+)"
    [regex]$removeEndOfXrefRegex = "(?<=\w+)"" .*></xref>"

    $cleanedContent = $markdownContent -replace $removeLinkExtensionRegex, "" -replace $removeStartOfXrefRegex, "" -replace $removeEndOfXrefRegex, ""
    
    # docfx sometimes duplicates some sections -> Remove duplicates
    $duplicateIds = Get-AllDuplicateIds $cleanedContent
    foreach ($id in $duplicateIds) {
        Write-Host "Removing duplicate Section for id: $id"
        [regex]$removeSectionRegex = "(?s)### <a id=""$id""></a>(.)+?(?=\n### )"
        $cleanedContent = $removeSectionRegex.Replace($cleanedContent, "", 1)
        # Write-Host "Cleaned markdown: $cleanedMarkdown"
    }
    
    return $cleanedContent
}

dotnet tool update -g docfx # TODO: install in pipeline instead
docfx metadata docfx.json

$markdownFiles = Get-ChildItem -Path "../Markdown2Pdf.wiki" -Filter "*.md" -Recurse -File

foreach ($file in $markdownFiles) {
    $markdownContent = Get-Content $file.FullName -Raw
    $cleanedMarkdown = CleanMarkdown $markdownContent
    Set-Content -Path $file.FullName -Value $cleanedMarkdown
}

Write-Host "Updated markdown files for the Github Wiki."