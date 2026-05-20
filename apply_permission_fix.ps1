# Script para corrigir validação de permissão no menu lateral

$viewsPath = "ATLAS ERP\Views"
$files = Get-ChildItem -Path $viewsPath -Recurse -Filter "*.cshtml"

$counter = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw

    # Skip if doesn't have Layout = null
    if (-not ($content -match "Layout = null")) {
        continue
    }

    $originalContent = $content

    # Add using directive
    if (-not ($content -match "@using ATLAS_ERP.Helpers")) {
        $content = $content -replace "(@\{)", "@using ATLAS_ERP.Helpers`r`n`$1"
        Write-Host "✓ Added @using to $($file.Name)"
    }

    # Update Cargo menu item with permission check
    if ($content -match 'href="/Cargo"' -and -not ($content -match '@if.*PermissaoHelper')) {
        # Find and replace the Cargo link pattern
        $cargoPattern = '        <a href="/Cargo" class="menu-item">([\s\S]*?)</a>'
        if ($content -match $cargoPattern) {
            $cargoContent = $matches[1]
            $replacement = "        @if (PermissaoHelper.UsuarioTemPermissao(`"cargos_manage`"))`r`n        {`r`n        <a href=`"/Cargo`" class=`"menu-item`">$cargoContent</a>`r`n        }"
            $content = $content -replace $cargoPattern, $replacement
            Write-Host "✓ Updated Cargo menu in $($file.Name)"
        }
    }

    # Save if changed
    if ($content -ne $originalContent) {
        Set-Content $file.FullName $content -Encoding UTF8
        $counter++
    }
}

Write-Host "`n✓ Updated $counter files"
