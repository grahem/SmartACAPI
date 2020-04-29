Set-Location $PSScriptRoot
Set-Location ..
# Authenticate into AWS's ECR. 
Invoke-Expression -Command (Get-ECRLoginCommand -Region us-west-1 -ProfileName smartac-user).Command
# Display a list of the envrionments
eb list
# Get the version number
$VersionNumber = Read-Host -Prompt 'Provide a Version Number'
git tag -a v$VersionNumber -m "Build Version $VersionNumber"
# Build Docker
docker build --pull -t smartacdeviceapi .
# Tag the versions
docker tag smartacdeviceapi:latest 009952781627.dkr.ecr.us-west-1.amazonaws.com/smartacdeviceapi:latest
docker tag smartacdeviceapi:latest 009952781627.dkr.ecr.us-west-1.amazonaws.com/smartacdeviceapi:$VersionNumber
# Push the versions up
docker push 009952781627.dkr.ecr.us-west-1.amazonaws.com/smartacdeviceapi:latest
docker push 009952781627.dkr.ecr.us-west-1.amazonaws.com/smartacdeviceapi:$VersionNumber
# Replace the string.
(Get-Content scripts\Dockerrun.aws.json).replace('[VersionNumber]', $VersionNumber) | Set-Content Dockerrun.aws.json
# Deploy
eb deploy --label smartacdeviceapi.$VersionNumber
# Reset the file
Remove-Item Dockerrun.aws.json 
