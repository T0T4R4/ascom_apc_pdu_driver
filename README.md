# ASCOM Driver for APC's Power Distribution Units

*Disclaimer: This program is not distributed by APC Enterprises. It is provided to the users community for free, under the Creative Common 3 license.*

## Summary 

This is my implementation of an **[ASCOM](https://ascom-standards.org/) Driver** which can interact with 
your **[Schneider Electric/APC Switched Rack PDU](https://astrohaven.com/)** via its SSH interface.

The driver is built in C# using the open-source *ASCOM Driver Visual Studio template*  , provided by the *ASCOM Initiative*.
![screenshot](https://user-images.githubusercontent.com/1294511/54508504-da1e1180-4991-11e9-8bdb-8db12b207d3b.png)

## Requirements

In order to **use** this driver you will need to install the [ASCOM Platform](https://github.com/ASCOMInitiative/ASCOMPlatform/releases) and the [.NET Framework 4](https://www.microsoft.com/en-au/download/details.aspx?id=17851) (which may already come with the ASCOM Installer).

If you want to **modify** this driver, you will  also need to install the *ASCOM Platform 64bit Developer package* available on the same release page (ASCOMPlatform64Developer.exe).

I aimed at building a drivor specifically for the [AP7921B model](https://www.apc.com/shop/au/en/products/Rack-PDU-Switched-1U-16A-208-230V-8-C13/P-AP7921B), but you can adapt it for your APC PDU model.

By default, SSH is not enabled on these PDU devices, so be sure to enabled it (and disable telnet, by the way, as it's unsecure) and restart your PDU device.

## Compiling and building

Just a reminder that in order to successfully build the project, it is preferrable to run Visual Studio as an administrator.

In order to build the installer, you will need to install [**Inno Setup**](http://www.jrsoftware.org/isdl.php#stable), the free installer for Windows programs by *Jordan Russell* and *Martijn Laan*. Open the file `AstroHaven.Dome Setup.iss` with *Inno Setup* and press the *run* button to build and launch the installer.

For more information, please refer to the guidelines provided as part of the [ASCOM Driver development guide](https://ascom-standards.org/Developer/DriverImpl.htm).

## Installation

Pre-built binaries for windows can be downloaded from the [releases]() page.

As for all ASCOM Switch drivers, it will be installed under the following directory :

`C:\Program Files (x86)\Common Files\ASCOM\Swich`

The installer will create the following elements :

- ASCOM.APCPDU.Switch.dll, the main driver library, which is automatically registered in the ASCOM registry upon installation,

- APCPDUTest folder, which contains the Test app (APCPDUTest.exe). I strongly recommend to try this app to control your PDU in the first place.

## How does it works ?

tbd

## Driver setup

As for every ASCOM Driver, this driver has a few parameters that must be setup the first time you use it either programmatically or via the Test application. 

Be sure to fill in the PDU's IP Address, SSH port, and Username and Password to be used for the SSH connection.


## Feedback

Please give me your feedback, positive or negative, raise any issue that occurs with your dome when controlling it via this driver, so we can together enhance this program as a community.



