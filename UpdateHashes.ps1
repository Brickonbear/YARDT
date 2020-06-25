$languages = 'de_de', 'en_us', 'es_es', 'es_mx', 'fr_fr', 'it_it', 'ja_jp', 'ko_kr', 'pl_pl', 'pt_br', 'tr_tr', 'ru_ru', 'zh_tw'
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
			$outDir = ".\{0}{1}\set{1}-{0}\$language\data\*.json" -f $language, $i
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
	For ($i = 1; $i -le $numOfSets; $i++) {
		'Set {0}' -f $i
		ForEach ($language in $languages) {
			$file = ".\dataSets\set{0}-{1}.json" -f $i, $language
			$hash = Get-FileHash $file -Algorithm MD5
			'{{"{0}", "{1}"}},' -f $language, $hash.hash
		}
	}

}
 
DownloadDataSets
UnzipFiles
New-Item -Name "dataSets" -ItemType "directory" | Out-Null
MoveFiles
DeleteFiles
PrintResult


