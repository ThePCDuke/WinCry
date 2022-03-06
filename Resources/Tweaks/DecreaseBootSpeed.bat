bcdedit /deletevalue hypervisorlaunchtype
bcdedit /deletevalue tpmbootentropy
bcdedit /deletevalue timeout
bcdedit /deletevalue bootux disabled
bcdedit /deletevalue bootmenupolicy 
bcdedit /deletevalue quietboot
bcdedit /deletevalue {globalsettings} custom:16000067 true
bcdedit /deletevalue {globalsettings} custom:16000069 true
bcdedit /deletevalue {globalsettings} custom:16000068 true