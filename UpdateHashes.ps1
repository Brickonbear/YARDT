$languages = 'de_de', 'en_us', 'es_es', 'fr_fr', 'it_it', 'ja_jp', 'ko_kr'

function DownloadDataSets {
    Import-Module BitsTransfer

    ForEach ($language in $languages) {
        $url = "https://dd.b.pvp.net/latest/set1-lite-{0}.zip" -f $language
        $outDir = "{0}.zip" -f $language
        Start-BitsTransfer -Source $url -Destination $outDir 
        Write-Output "$(split-path -path "$url" -leaf)"
    }
}

function UnzipFiles {
    ForEach ($language in $languages) {
        $inDir = "{0}.zip" -f $language
        Expand-Archive $inDir -DestinationPath $language
    }
}

function MoveFiles {
    ForEach ($language in $languages) {
        $outDir = ".\{0}\{0}\data\*.json" -f $language
        Move-Item -Path $outDir -Destination .\dataSets
    }
}
function DeleteFiles {
    ForEach ($language in $languages) {
        $dir = ".\{0}\" -f $language
        $zip = "{0}.zip" -f $language
        Remove-Item $dir -Recurse
        Remove-Item $zip
    }
}
 
DownloadDataSets
UnzipFiles
New-Item -Name "dataSets" -ItemType "directory" | Out-Null
MoveFiles
DeleteFiles


$de_de = Get-FileHash .\dataSets\set1-de_de.json -Algorithm MD5
$en_us = Get-FileHash .\dataSets\set1-en_us.json -Algorithm MD5
$es_es = Get-FileHash .\dataSets\set1-es_es.json -Algorithm MD5
$fr_fr = Get-FileHash .\dataSets\set1-fr_fr.json -Algorithm MD5
$it_it = Get-FileHash .\dataSets\set1-it_it.json -Algorithm MD5
$ja_jp = Get-FileHash .\dataSets\set1-ja_jp.json -Algorithm MD5
$ko_kr = Get-FileHash .\dataSets\set1-ko_kr.json -Algorithm MD5

"{{de_de, {0}}}, `
{{en_us, {1}}}, `
{{es_es, {2}}}, `
{{fr_fr, {3}}}, `
{{it_it, {4}}}, `
{{ja_jp, {5}}}, `
{{ko_kr, {6}}}" -f $de_de.hash, $en_us.hash, $es_es.hash, $fr_fr.hash, $it_it.hash, $ja_jp.hash, $ko_kr.hash