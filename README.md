# OrderPreservingEncryptionDotNet

OrderPreservingEncryptionDotNet is a dotnet core implementation of Boldyreva symmetric order-preserving encryption scheme [Boldyreva's paper](https://www.cc.gatech.edu/~aboldyre/papers/bclo.pdf).

## Installation

[Nuget package url](
https://www.nuget.org/packages/OrderPreservingEncryptionDotNet/?fbclid=IwAR1ntEun5zaZ_BH4JSEKBA37uUDXwHLmXXR0r7goQlzW-2zxplIsAW4hnSE)

Package-Manager Console  
```
Install-Package OrderPreservingEncryptionDotNet -Version 1.0.0  
```
.net CLI  
```
dotnet add package OrderPreservingEncryptionDotNet --version 1.0.0
```

## Usage

```csharp
using OrderPreservingEncryptionDotNet;

{...}

private byte[] key = OPE.CreateKey(32);
private OPE ope = new OPE(key);
var n1 = 1000;
var n2 = 1100;
var n1Encrypted = ope.Encrypt(n1);
var n2Encrypted = ope.Encrypt(n2);

if(n1Encrypted < n2Encrypted)
{
  Console.WriteLine("It works");
}
else
{
  throw new Exception(":-( Write me up an issue please.");
}

var n1Decrypted = ope.Decrypt(n1Encrypted);
var n2Decrypted = ope.Decrypt(n2Encrypted);

if( n1Decrypted != n1 || n2Decrypted != n2)
{
  throw new Exception(":-( Write me up an issue please.");
}

```

## Contributing
Issues are welcome. Pull requests are more than welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[GNU GENERAL PUBLIC LICENSE](LICENSE)

## Dependencies
[Bouncy Castle](https://www.bouncycastle.org/csharp/index.html)
