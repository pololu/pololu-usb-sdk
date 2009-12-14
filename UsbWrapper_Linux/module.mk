UsbWrapper_lib := $(UsbWrapper)/UsbWrapper.dll
Targets += $(UsbWrapper_lib)

$(UsbWrapper)/UsbWrapper.dll: $(UsbWrapper)/UsbDevice.cs
	$(CS) -target:library $(UsbWrapper)/UsbDevice.cs -out:$(UsbWrapper)/UsbWrapper.dll -lib:/usr/lib/mono/2.0/ -r:System.dll -r:System.Core.dll
