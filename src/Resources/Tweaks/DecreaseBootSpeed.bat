bcdedit /set hypervisorlaunchtype auto
bcdedit /deletevalue tpmbootentropy
bcdedit /timeout 30
bcdedit /deletevalue bootux
bcdedit /deletevalue quietboot
bcdedit /deletevalue {globalsettings} custom:16000067
bcdedit /deletevalue {globalsettings} custom:16000068
bcdedit /deletevalue {globalsettings} custom:16000069