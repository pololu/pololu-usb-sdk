UsbWrapper_lib := $(UsbWrapper)/UsbWrapper.dll
Targets += $(UsbWrapper_lib)

UsbWrapper_csfiles := $(wildcard $(UsbWrapper)/*.cs)

$(UsbWrapper)/UsbWrapper.dll: $(UsbWrapper_csfiles)
	$(CS) -target:library $(UsbWrapper_csfiles) -out:$(UsbWrapper)/UsbWrapper.dll -lib:/usr/lib/mono/2.0/ -r:System.dll -r:System.Core.dll
