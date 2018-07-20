using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace palettehelper
{
    class PalMethods
    {
        public static List<byte[]> loadpals(int start, int numpal, byte[] sourcearray)
        {
            List<byte[]> palettelist = new List<byte[]>();
            for (int i = start; i < numpal * 1024; i += 1024)
            {
                byte[] workingpal = new byte[1024];

                Array.Copy(sourcearray, i, workingpal, 0, workingpal.Length);
                palettelist.Add(workingpal);
                Console.WriteLine(i);
                Console.WriteLine(workingpal.Length);
                Console.WriteLine("loaded " + palettelist.Count + " palettes");
            }
            return palettelist;
        }
        public static byte[] fixpalsides(string character,byte[] input,byte[] input2)
        {
            List<byte[]> twosides = new List<byte[]>();

            byte[] bufferpal = new byte[1024];
            byte[] outcolors = new byte[2048];

            int[] sethcolormapfrom = new int[] { 11, 12, 13, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153 };
            int[] sethcolormapto = new int[] { 27, 28, 29, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169 };

            int[] hildacolormapfrom = new int[] { 26, 27, 28, 64, 65, 66, 67, 68, 80, 81, 82, 83 };
            int[] hildacolormapto = new int[] { 42, 43, 44, 70, 71, 72, 73, 74, 85, 86, 87, 88 };

            int[] stmcolormapfrom = new int[] { 11, 12, 13 };
            int[] stmcolormapto = new int[] { 27, 28, 29 };

            int numcol = 0;

            switch (character)
            {
                case "seth":
                    numcol = sethcolormapfrom.Length;
                    break;
                case "hilda":
                    numcol = hildacolormapfrom.Length;
                    break;
                case "rentaro":
                    numcol = stmcolormapfrom.Length;
                    break;
            }
            for (int i = 0; i < numcol; i++)
            {
                int fromval = 0;
                int toval = 0;

                switch (character)
                {
                    case "seth":
                        fromval = sethcolormapfrom[i] * 4;
                        toval = sethcolormapto[i] * 4;
                        break;
                    case "hilda":
                        fromval = hildacolormapfrom[i] * 4;
                        toval = hildacolormapto[i] * 4;
                        break;
                    case "rentaro":
                        fromval = stmcolormapfrom[i] * 4;
                        toval = stmcolormapto[i] * 4;
                        break;
                }

                Array.Copy(input2, fromval, bufferpal, fromval, 4);
                Array.Copy(input, toval, input2, fromval, 4);
                Array.Copy(bufferpal, fromval, input2, toval, 4);
            }
            Array.Copy(input, outcolors, 1024);
            Array.Copy(input2, 0, outcolors, 1023, 1024);

            return outcolors;
        }
    }
}
