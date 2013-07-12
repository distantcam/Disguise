
function Edit-DisguiseConfig() {
    $project = Get-Project

	$fodyWeaversPath = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($project.FullName), "FodyWeavers.xml")

	echo "Path $fodyWeaversPath"
}

Export-ModuleMember Edit-DisguiseConfig