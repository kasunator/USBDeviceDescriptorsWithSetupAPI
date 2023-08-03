#Get USB Device name example


##We have tow examples at hand 

[The first example we had was in the following stack overflow link]
(https://stackoverflow.com/questions/26732291/how-to-get-bus-reported-device-description-using-c-sharp)

	This is in C# and made to get all the COM ports, it does get all the com ports  but the GetDEviceBusDescription() fails.

[The second example I found was in the following stack overflow link]  

(https://stackoverflow.com/questions/3438366/setupdigetdeviceproperty-usage-example)

	This code is in C++. I was able to execute this code and I was able to sucessfully get the "Bus Reported Device Description" aka USB device name we were looking for.

_NOTE: The USB device name aka "Bus reported Device description" we were looking for is populated by the structure member  
struct  custom_hid_desc.string and I think this is fetched by the "USB String Descriptor"( i.e bDescriptorType String Descriptor(0x03) )_