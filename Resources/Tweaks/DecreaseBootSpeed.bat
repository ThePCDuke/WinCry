bcdedit /set hypervisorlaunchtype auto
bcdedit /deletevalue tpmbootentropy
bcdedit /timeout 30
bcdedit /deletevalue bootux
bcdedit /set bootmenupolicy standard
bcdedit /deletevalue {globalsettings} custom:16000067
bcdedit /deletevalue {globalsettings} custom:16000069
bcdedit /deletevalue {globalsettings} custom:16000068