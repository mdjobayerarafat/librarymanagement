import os
import re

src_dir = r"c:\Users\jobayer\source\repos\librarymanagement\librarymanagement"

color_map = {
    r"BackgroundColor\s*=\s*Color\.FromArgb\([^)]+\)": "BackgroundColor = Color.FromArgb(247, 247, 255)",
    r"CardBackgroundColor\s*=\s*Color\.White": "CardBackgroundColor = Color.White",
    r"SidebarBackgroundColor\s*=\s*Color\.FromArgb\([^)]+\)": "SidebarBackgroundColor = Color.FromArgb(41, 47, 54)",
    
    r"TextPrimaryColor\s*=\s*Color\.FromArgb\([^)]+\)": "TextPrimaryColor = Color.FromArgb(41, 47, 54)",
    r"TextSecondaryColor\s*=\s*Color\.FromArgb\([^)]+\)": "TextSecondaryColor = Color.FromArgb(73, 88, 103)",
    r"TextMutedColor\s*=\s*Color\.FromArgb\([^)]+\)": "TextMutedColor = Color.FromArgb(130, 140, 150)", 
    
    r"AccentColor\s*=\s*Color\.FromArgb\([^)]+\)": "AccentColor = Color.FromArgb(87, 115, 153)",
    r"AccentLightColor\s*=\s*Color\.FromArgb\([^)]+\)": "AccentLightColor = Color.FromArgb(189, 213, 234)",
    r"AccentAmber\s*=\s*Color\.FromArgb\([^)]+\)": "AccentAmber = Color.FromArgb(219, 142, 60)",
    r"AccentRed\s*=\s*Color\.FromArgb\([^)]+\)": "AccentRed = Color.FromArgb(255, 102, 94)",
    
    r"WarningColor\s*=\s*Color\.FromArgb\([^)]+\)": "WarningColor = Color.FromArgb(255, 102, 94)",
    r"WarningLightColor\s*=\s*Color\.FromArgb\([^)]+\)": "WarningLightColor = Color.FromArgb(255, 230, 230)",
    
    r"InputBackgroundColor\s*=\s*Color\.FromArgb\([^)]+\)": "InputBackgroundColor = Color.FromArgb(255, 255, 255)",
    r"BorderColor\s*=\s*Color\.FromArgb\([^)]+\)": "BorderColor = Color.FromArgb(189, 213, 234)",
    r"CardBorderColor\s*=\s*Color\.FromArgb\([^)]+\)": "CardBorderColor = Color.FromArgb(189, 213, 234)",
    r"CardBorderHoverColor\s*=\s*Color\.FromArgb\([^)]+\)": "CardBorderHoverColor = Color.FromArgb(87, 115, 153)",
    
    r"TagColor\s*=\s*Color\.FromArgb\([^)]+\)": "TagColor = Color.FromArgb(189, 213, 234)",
    r"TagTextColor\s*=\s*Color\.FromArgb\([^)]+\)": "TagTextColor = Color.FromArgb(41, 47, 54)",
    
    r"AvailableBg\s*=\s*Color\.FromArgb\([^)]+\)": "AvailableBg = Color.FromArgb(229, 242, 201)",
    r"AvailableText\s*=\s*Color\.FromArgb\([^)]+\)": "AvailableText = Color.FromArgb(41, 47, 54)",
    r"AvailableText\s*=\s*Color\.White": "AvailableText = Color.FromArgb(41, 47, 54)",
    
    r"UnavailableBg\s*=\s*Color\.FromArgb\([^)]+\)": "UnavailableBg = Color.FromArgb(255, 230, 230)",
    r"UnavailableText\s*=\s*Color\.FromArgb\([^)]+\)": "UnavailableText = Color.FromArgb(255, 102, 94)",
    
    r"RightPanelColor\s*=\s*Color\.FromArgb\([^)]+\)": "RightPanelColor = Color.FromArgb(41, 47, 54)",
    r"AccentColorDark\s*=\s*Color\.FromArgb\([^)]+\)": "AccentColorDark = Color.FromArgb(73, 88, 103)",
    r"ErrorColor\s*=\s*Color\.FromArgb\([^)]+\)": "ErrorColor = Color.FromArgb(255, 102, 94)",
}

# Sidebar specific text colors inside DashboardForm.cs need to be inverted
dashboard_sidebar_fixes = {
    # In CreateSidebar()
    r"ForeColor = TextPrimaryColor,(\s*// For UserName)": r"ForeColor = Color.FromArgb(247, 247, 255),\1",
    r"lblUserName = new Label\s*\{\s*Text = username,\s*Font = new Font\(\"Segoe UI\", 10, FontStyle\.Bold\),\s*ForeColor = TextPrimaryColor,": "lblUserName = new Label\n            {\n                Text = username,\n                Font = new Font(\"Segoe UI\", 10, FontStyle.Bold),\n                ForeColor = Color.FromArgb(247, 247, 255),",
    r"lblUserRole = new Label\s*\{\s*Text = role,\s*Font = new Font\(\"Segoe UI\", 8\.5F\),\s*ForeColor = TextSecondaryColor,": "lblUserRole = new Label\n            {\n                Text = role,\n                Font = new Font(\"Segoe UI\", 8.5F),\n                ForeColor = Color.FromArgb(189, 213, 234),",
    r"var textColor = isActive \? AccentColor : TextSecondaryColor;": "var textColor = isActive ? Color.FromArgb(41, 47, 54) : Color.FromArgb(189, 213, 234);",
    r"using var bgBrush = new SolidBrush\(AccentLightColor\);": "using var bgBrush = new SolidBrush(Color.FromArgb(189, 213, 234));",
    r"using var bgBrush = new SolidBrush\(InputBackgroundColor\);": "using var bgBrush = new SolidBrush(Color.FromArgb(73, 88, 103));", # hover background on sidebar
    r"var divider1 = new Panel\s*\{\s*Dock = DockStyle\.Top,\s*Height = 1,\s*BackColor = BorderColor\s*\};": "var divider1 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(73, 88, 103) };",
    r"var divider2 = new Panel\s*\{\s*Dock = DockStyle\.Top,\s*Height = 1,\s*BackColor = BorderColor\s*\};": "var divider2 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(73, 88, 103) };",
    r"lblBrand\.ForeColor = AccentColor;": "lblBrand.ForeColor = Color.FromArgb(189, 213, 234);",
    r"ForeColor = AccentColor,\s*AutoSize = true,\s*Location = new Point\(58, 26\)": "ForeColor = Color.FromArgb(189, 213, 234),\n                AutoSize = true,\n                Location = new Point(58, 26)",
    r"lblSubtitle = new Label\s*\{\s*Text = \"Management System\",\s*Font = new Font\(\"Segoe UI\", 9F\),\s*ForeColor = TextSecondaryColor,": "lblSubtitle = new Label\n            {\n                Text = \"Management System\",\n                Font = new Font(\"Segoe UI\", 9F),\n                ForeColor = Color.FromArgb(189, 213, 234),"
}

for root, _, files in os.walk(src_dir):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8-sig') as f:
                content = f.read()
            
            new_content = content
            for pattern, replacement in color_map.items():
                new_content = re.sub(pattern, replacement, new_content)
            
            if file == "DashboardForm.cs":
                for pattern, replacement in dashboard_sidebar_fixes.items():
                    new_content = re.sub(pattern, replacement, new_content)
            
            if new_content != content:
                with open(filepath, 'w', encoding='utf-8-sig') as f:
                    f.write(new_content)
                print(f"Updated colors in {file}")

print("Color update complete!")
