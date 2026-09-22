Get-PnpDevice -FriendlyName "*Tesla*" | Disable-PnpDevice -Confirm:$false
# Get-PnpDevice -FriendlyName "*Tesla*" | Select-Object FriendlyName, InstanceId
pnputil /remove-device "PCI\VEN_10DE&DEV_1DB5&SUBSYS_124910DE&REV_A1\4&8BD6E8D&0&0008"