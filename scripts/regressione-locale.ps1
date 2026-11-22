<#
.SYNOPSIS
    Regressione locale dell'estrazione su un set: riproduce le Registrazioni, scrive il rapporto e stampa il comando di pubblicazione.

.DESCRIPTION
    Esegue "dbe eval extraction --replay" sul set indicato (nessuna chiamata AI, nessuna rete) e copia il rapporto
    in <Uscita>\<versione del prompt>\<nome del set>. Il comando "gh release upload" viene solo stampato: non si esegue.
    Esce con un codice diverso da 0 se la regressione non riesce.

.PARAMETER Set
    Cartella del set (contiene set.json).

.PARAMETER Uscita
    Cartella radice dei rapporti. Predefinita: rapporti-regressione nella cartella corrente.

.PARAMETER Dbe
    Eseguibile dbe. Se omesso si usa "dbe" dal PATH, altrimenti "dotnet run" sul progetto Console del repository.

.PARAMETER Tag
    Tag della release a cui allegare il rapporto, solo per comporre il comando stampato.

.EXAMPLE
    .\scripts\regressione-locale.ps1 -Set tests\DBEnigmistica.Tests\Fixtures\SetSintetico
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Set,
    [string]$Uscita = 'rapporti-regressione',
    [string]$Dbe,
    [string]$Fornitore = 'finto',
    [string]$Modello = 'finto-1',
    [int]$Dpi = 200,
    [string]$Tag = '<tag-della-release>'
)

$ErrorActionPreference = 'Stop'

function Invoke-Dbe {
    param([string[]]$Argomenti)
    if ($Dbe) {
        & $Dbe @Argomenti
    }
    elseif (Get-Command dbe -ErrorAction SilentlyContinue) {
        & dbe @Argomenti
    }
    else {
        $radice = Split-Path -Parent $PSScriptRoot
        & dotnet run --project (Join-Path $radice 'src\DBEnigmistica.Console') --configuration Release --verbosity quiet -- @Argomenti
    }
}

try {
    if (-not (Test-Path -LiteralPath (Join-Path $Set 'set.json'))) {
        throw "Nel set '$Set' manca set.json."
    }

    $temporanea = Join-Path ([System.IO.Path]::GetTempPath()) ("dbe-regressione-" + [guid]::NewGuid().ToString('N'))
    try {
        Invoke-Dbe @('eval', 'extraction', '--replay', '--set', $Set, '--fornitore', $Fornitore, '--modello', $Modello, '--dpi', $Dpi, '--output', $temporanea)
        if ($LASTEXITCODE -ne 0) {
            throw "La regressione non e' riuscita (codice $LASTEXITCODE)."
        }

        $rapporto = Get-Content -LiteralPath (Join-Path $temporanea 'rapporto.json') -Raw -Encoding UTF8 | ConvertFrom-Json
        $versione = $rapporto.condizioni.versione_prompt
        $nome = $rapporto.set
        $destinazione = Join-Path (Join-Path $Uscita $versione) $nome
        New-Item -ItemType Directory -Force -Path $destinazione | Out-Null
        Copy-Item -LiteralPath (Join-Path $temporanea 'rapporto.json'), (Join-Path $temporanea 'rapporto.md') -Destination $destinazione -Force
    }
    finally {
        if (Test-Path -LiteralPath $temporanea) {
            Remove-Item -LiteralPath $temporanea -Recurse -Force
        }
    }

    $json = Join-Path $destinazione 'rapporto.json'
    $md = Join-Path $destinazione 'rapporto.md'
    Write-Host ''
    Write-Host "Rapporto salvato in: $destinazione"
    Write-Host 'Per allegarlo alla release, esegui tu questo comando (non viene eseguito):'
    Write-Host "gh release upload $Tag `"$json`" `"$md`" --clobber"
    exit 0
}
catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}
