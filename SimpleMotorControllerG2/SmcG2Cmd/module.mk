# Generate a unique list of files that need to be in the same
# directory as this program at runtime (runtime dependencies).
SmcG2Cmd_runtime := $(sort $(UsbWrapper_lib) $(SmcG2_lib))

# Compile-time dependencies.
SmcG2Cmd_dlls := $(UsbWrapper)/UsbWrapper.dll $(SmcG2)/Smc.dll
SmcG2Cmd_csfiles := $(SmcCmd)/Program.cs $(SmcG2Cmd)/Properties/AssemblyInfo.cs

# Required module variables
Targets += $(SmcG2Cmd)/smcg2cmd
Byproducts += $(foreach dll, $(SmcCmd_runtime), $(SmcCmd)/$(notdir $(dll)))

$(SmcG2Cmd)/smcg2cmd: $(SmcG2Cmd_csfiles) $(SmcCmd_dlls)
	cp $(SmcCmd_runtime) $(SmcG2Cmd)
	$(CS) -target:exe -out:$@.exe $(SmcG2Cmd_csfiles) $(foreach dll, $(SmcG2Cmd_dlls),-r:$(SmcG2Cmd)/$(notdir $(dll)))
	mv $@.exe $@

smcg2cmd: $(SmcG2Cmd)/smcg2cmd
