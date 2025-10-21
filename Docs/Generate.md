# Command to generate P/Invoke code
Example
```powershell

ClangSharpPInvokeGenerator -c generate-macro-bindings -l mdk -n MDK.SDK.NET.Gen -f .\global.h -o ..\..\..\csharp_gen\global.cs
ClangSharpPInvokeGenerator -l mdk -n MDK.SDK.NET.Gen -f .\MediaInfo.h -o ..\..\..\csharp_gen\MediaInfo.cs
ClangSharpPInvokeGenerator -l mdk -n MDK.SDK.NET.Gen -f .\VideoFrame.h -o ..\..\..\csharp_gen\VideoFrame.cs
ClangSharpPInvokeGenerator -c generate-macro-bindings -l mdk -n MDK.SDK.NET.Gen -f .\Player.h -o ..\..\..\csharp_gen\Player.cs
ClangSharpPInvokeGenerator -l mdk -n MDK.SDK.NET.Gen -f .\AudioFrame.h -o ..\..\..\csharp_gen\AudioFrame.cs

```
