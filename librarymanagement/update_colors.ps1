$srcDir = "c:\Users\jobayer\source\repos\librarymanagement\librarymanagement"

$colorMap = @{
    "BackgroundColor\s*=\s*Color\.FromArgb\([^)]+\)" = "BackgroundColor = Color.FromArgb(247, 247, 255)"
    "CardBackgroundColor\s*=\s*Color\.White" = "CardBackgroundColor = Color.White"
    "SidebarBackgroundColor\s*=\s*Color\.FromArgb\([^)]+\)" = "SidebarBackgroundColor = Color.FromArgb(41, 47, 54)"
    
    "TextPrimaryColor\s*=\s*Color\.FromArgb\([^)]+\)" = "TextPrimaryColor = Color.FromArgb(41, 47, 54)"
    "TextSecondaryColor\s*=\s*Color\.FromArgb\([^)]+\)" = "TextSecondaryColor = Color.FromArgb(73, 88, 103)"
    "TextMutedColor\s*=\s*Color\.FromArgb\([^)]+\)" = "TextMutedColor = Color.FromArgb(130, 140, 150)"
    
    "AccentColor\s*=\s*Color\.FromArgb\([^)]+\)" = "AccentColor = Color.FromArgb(87, 115, 153)"
    "AccentLightColor\s*=\s*Color\.FromArgb\([^)]+\)" = "AccentLightColor = Color.FromArgb(189, 213, 234)"
    "AccentAmber\s*=\s*Color\.FromArgb\([^)]+\)" = "AccentAmber = Color.FromArgb(219, 142, 60)"
    "AccentRed\s*=\s*Color\.FromArgb\([^)]+\)" = "AccentRed = Color.FromArgb(255, 102, 94)"
    
    "WarningColor\s*=\s*Color\.FromArgb\([^)]+\)" = "WarningColor = Color.FromArgb(255, 102, 94)"
    "WarningLightColor\s*=\s*Color\.FromArgb\([^)]+\)" = "WarningLightColor = Color.FromArgb(255, 230, 230)"
    
    "InputBackgroundColor\s*=\s*Color\.FromArgb\([^)]+\)" = "InputBackgroundColor = Color.FromArgb(255, 255, 255)"
    "BorderColor\s*=\s*Color\.FromArgb\([^)]+\)" = "BorderColor = Color.FromArgb(189, 213, 234)"
    "CardBorderColor\s*=\s*Color\.FromArgb\([^)]+\)" = "CardBorderColor = Color.FromArgb(189, 213, 234)"
    "CardBorderHoverColor\s*=\s*Color\.FromArgb\([^)]+\)" = "CardBorderHoverColor = Color.FromArgb(87, 115, 153)"
    
    "TagColor\s*=\s*Color\.FromArgb\([^)]+\)" = "TagColor = Color.FromArgb(189, 213, 234)"
    "TagTextColor\s*=\s*Color\.FromArgb\([^)]+\)" = "TagTextColor = Color.FromArgb(41, 47, 54)"
    
    "AvailableBg\s*=\s*Color\.FromArgb\([^)]+\)" = "AvailableBg = Color.FromArgb(229, 242, 201)"
    "AvailableText\s*=\s*Color\.FromArgb\([^)]+\)" = "AvailableText = Color.FromArgb(41, 47, 54)"
    "AvailableText\s*=\s*Color\.White" = "AvailableText = Color.FromArgb(41, 47, 54)"
    
    "UnavailableBg\s*=\s*Color\.FromArgb\([^)]+\)" = "UnavailableBg = Color.FromArgb(255, 230, 230)"
    "UnavailableText\s*=\s*Color\.FromArgb\([^)]+\)" = "UnavailableText = Color.FromArgb(255, 102, 94)"
    
    "RightPanelColor\s*=\s*Color\.FromArgb\([^)]+\)" = "RightPanelColor = Color.FromArgb(41, 47, 54)"
    "AccentColorDark\s*=\s*Color\.FromArgb\([^)]+\)" = "AccentColorDark = Color.FromArgb(73, 88, 103)"
    "ErrorColor\s*=\s*Color\.FromArgb\([^)]+\)" = "ErrorColor = Color.FromArgb(255, 102, 94)"
}

$dashboardSidebarFixes = @{
    "lblUserName = new Label\s*\{\s*Text = username,\s*Font = new Font\(`"Segoe UI`", 10, FontStyle\.Bold\),\s*ForeColor = TextPrimaryColor," = "lblUserName = new Label`n            {`n                Text = username,`n                Font = new Font(`"Segoe UI`", 10, FontStyle.Bold),`n                ForeColor = Color.FromArgb(247, 247, 255),"
    "lblUserRole = new Label\s*\{\s*Text = role,\s*Font = new Font\(`"Segoe UI`", 8\.5F\),\s*ForeColor = TextSecondaryColor," = "lblUserRole = new Label`n            {`n                Text = role,`n                Font = new Font(`"Segoe UI`", 8.5F),`n                ForeColor = Color.FromArgb(189, 213, 234),"
    "var textColor = isActive \? AccentColor : TextSecondaryColor;" = "var textColor = isActive ? Color.FromArgb(41, 47, 54) : Color.FromArgb(189, 213, 234);"
    "using var bgBrush = new SolidBrush\(AccentLightColor\);" = "using var bgBrush = new SolidBrush(Color.FromArgb(189, 213, 234));"
    "using var bgBrush = new SolidBrush\(InputBackgroundColor\);" = "using var bgBrush = new SolidBrush(Color.FromArgb(73, 88, 103));"
    "var divider1 = new Panel\s*\{\s*Dock = DockStyle\.Top,\s*Height = 1,\s*BackColor = BorderColor\s*\};" = "var divider1 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(73, 88, 103) };"
    "var divider2 = new Panel\s*\{\s*Dock = DockStyle\.Top,\s*Height = 1,\s*BackColor = BorderColor\s*\};" = "var divider2 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(73, 88, 103) };"
    "lblBrand\.ForeColor = AccentColor;" = "lblBrand.ForeColor = Color.FromArgb(189, 213, 234);"
    "ForeColor = AccentColor,\s*AutoSize = true,\s*Location = new Point\(58, 26\)" = "ForeColor = Color.FromArgb(189, 213, 234),`n                AutoSize = true,`n                Location = new Point(58, 26)"
    "lblSubtitle = new Label\s*\{\s*Text = `"Management System`",\s*Font = new Font\(`"Segoe UI`", 9F\),\s*ForeColor = TextSecondaryColor," = "lblSubtitle = new Label`n            {`n                Text = `"Management System`",`n                Font = new Font(`"Segoe UI`", 9F),`n                ForeColor = Color.FromArgb(189, 213, 234),"
}

Get-ChildItem -Path $srcDir -Filter "*.cs" -Recurse | ForEach-Object {
    $filePath = $_.FullName
    $content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
    $originalContent = $content
    
    foreach ($key in $colorMap.Keys) {
        $content = [System.Text.RegularExpressions.Regex]::Replace($content, $key, $colorMap[$key])
    }
    
    if ($_.Name -eq "DashboardForm.cs") {
        foreach ($key in $dashboardSidebarFixes.Keys) {
            $content = [System.Text.RegularExpressions.Regex]::Replace($content, $key, $dashboardSidebarFixes[$key])
        }
    }
    
    if ($content -cne $originalContent) {
        [System.IO.File]::WriteAllText($filePath, $content, [System.Text.Encoding]::UTF8)
        Write-Host "Updated colors in $($_.Name)"
    }
}
Write-Host "Color update complete!"
