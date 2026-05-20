#!/bin/bash

# Lista de views que precisam ser atualizadas (excluindo as já feitas)
views=(
    "ATLAS ERP/Views/Produto/Create.cshtml"
    "ATLAS ERP/Views/Cliente/Index.cshtml"
    "ATLAS ERP/Views/Cliente/Create.cshtml"
    "ATLAS ERP/Views/Fornecedor/Index.cshtml"
    "ATLAS ERP/Views/Fornecedor/Create.cshtml"
    "ATLAS ERP/Views/Usuario/Index.cshtml"
    "ATLAS ERP/Views/Usuario/Create.cshtml"
    "ATLAS ERP/Views/Venda/Index.cshtml"
    "ATLAS ERP/Views/Venda/Financeiro.cshtml"
    "ATLAS ERP/Views/Cargo/Index.cshtml"
    "ATLAS ERP/Views/Cargo/Create.cshtml"
    "ATLAS ERP/Views/Cargo/Edit.cshtml"
    "ATLAS ERP/Views/Cargo/Permissoes.cshtml"
)

for file in "${views[@]}"; do
    if [ -f "$file" ]; then
        # Verificar se tem Layout = null e @using
        if grep -q "Layout = null" "$file" && ! grep -q "@using ATLAS_ERP.Helpers" "$file"; then
            # Adicionar @using após @model (se existir) ou no início do bloco @{
            if grep -q "@model" "$file"; then
                sed -i '1s/^/@using ATLAS_ERP.Helpers\n/' "$file"
            fi
            echo "✓ Added @using to $file"
        fi
    fi
done

echo "Done!"
