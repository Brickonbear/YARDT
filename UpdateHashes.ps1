$languages = 'de_de', 'en_us', 'es_es', 'fr_fr', 'it_it', 'ja_jp', 'ko_kr'
$numOfSets = 2

function DownloadDataSets {
	Import-Module BitsTransfer

	For ($i = 1; $i -le $numOfSets; $i++) {
		ForEach ($language in $languages) {
			$url = "https://dd.b.pvp.net/latest/set{0}-lite-{1}.zip" -f $i, $language
			$outDir = "{0}{1}.zip" -f $language, $i
			Start-BitsTransfer -Source $url -Destination $outDir 
			Write-Output "$(split-path -path "$url" -leaf)"
		}
	}
}

function UnzipFiles {
	For ($i = 1; $i -le $numOfSets; $i++) {
		ForEach ($language in $languages) {
			$inDir = "{0}{1}.zip" -f $language, $i
			$outDir = "{0}{1}" -f $language, $i
			Expand-Archive $inDir -DestinationPath $outDir
		}
	}
}

function MoveFiles {
	For ($i = 1; $i -le $numOfSets; $i++) {
		ForEach ($language in $languages) {
			$outDir = ".\{0}{1}\{0}\data\*.json" -f $language, $i
			Move-Item -Path $outDir -Destination .\dataSets
		}
	}
}
function DeleteFiles {
	For ($i = 1; $i -le $numOfSets; $i++) {
		ForEach ($language in $languages) {
			$dir = ".\{0}{1}\" -f $language, $i
			$zip = "{0}{1}.zip" -f $language, $i
			Remove-Item $dir -Recurse
			Remove-Item $zip
		}
	}
}

function PrintResult {
	$de_de_set1 = Get-FileHash .\dataSets\set1-de_de.json -Algorithm MD5
	$en_us_set1 = Get-FileHash .\dataSets\set1-en_us.json -Algorithm MD5
	$es_es_set1 = Get-FileHash .\dataSets\set1-es_es.json -Algorithm MD5
	$fr_fr_set1 = Get-FileHash .\dataSets\set1-fr_fr.json -Algorithm MD5
	$it_it_set1 = Get-FileHash .\dataSets\set1-it_it.json -Algorithm MD5
	$ja_jp_set1 = Get-FileHash .\dataSets\set1-ja_jp.json -Algorithm MD5
	$ko_kr_set1 = Get-FileHash .\dataSets\set1-ko_kr.json -Algorithm MD5
	$de_de_set2 = Get-FileHash .\dataSets\set2-de_de.json -Algorithm MD5
	$en_us_set2 = Get-FileHash .\dataSets\set2-en_us.json -Algorithm MD5
	$es_es_set2 = Get-FileHash .\dataSets\set2-es_es.json -Algorithm MD5
	$fr_fr_set2 = Get-FileHash .\dataSets\set2-fr_fr.json -Algorithm MD5
	$it_it_set2 = Get-FileHash .\dataSets\set2-it_it.json -Algorithm MD5
	$ja_jp_set2 = Get-FileHash .\dataSets\set2-ja_jp.json -Algorithm MD5
	$ko_kr_set2 = Get-FileHash .\dataSets\set2-ko_kr.json -Algorithm MD5

	"Set 1"

	'{{de_de, "{0}"}}, `
{{en_us, "{1}"}}, `
{{es_es, "{2}"}}, `
{{fr_fr, "{3}"}}, `
{{it_it, "{4}"}}, `
{{ja_jp, "{5}"}}, `
{{ko_kr, "{6}"}}' -f $de_de_set1.hash, $en_us_set1.hash, $es_es_set1.hash, $fr_fr_set1.hash, $it_it_set1.hash, $ja_jp_set1.hash, $ko_kr_set1.hash

	"Set 2"

	'{{de_de, "{0}"}}, `
{{en_us, "{1}"}}, `
{{es_es, "{2}"}}, `
{{fr_fr, "{3}"}}, `
{{it_it, "{4}"}}, `
{{ja_jp, "{5}"}}, `
{{ko_kr, "{6}"}}' -f $de_de_set2.hash, $en_us_set2.hash, $es_es_set2.hash, $fr_fr_set2.hash, $it_it_set2.hash, $ja_jp_set2.hash, $ko_kr_set2.hash
}
 
DownloadDataSets
UnzipFiles
New-Item -Name "dataSets" -ItemType "directory" | Out-Null
MoveFiles
DeleteFiles
PrintResult


