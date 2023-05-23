using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.Text.Msdfgen;

//public static class MsdfFontLoader
//{
//    public static string CachePath

//    private static char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

//    public Font GenerateFromTtf(string fontName, string pathToTtf)
//    {
//        for (int i = 0; i < fontName.Length; i++)
//        {
//            if (Array.IndexOf(invalidFileNameChars, fontName[i]) != -1)
//                throw new ArgumentException("fontName contains invalid characters");
//        }
//        Process.Start(
//            "msdf-atlas-gen", $"-font \"{pathToTtf}\" -charset charset.txt -format png -pots -imageout \"{fontName}.png\" -json \"{fontName}.json\""); 
//    }
//}
