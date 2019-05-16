using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;

namespace palettehelper
{
    public enum pal_game : Int32
    {
        UNIST = 0,
        UNIEL = 1,
        Melty = 2,
        NitroPlus = 3,
    }

    public class PalFile
    {
        public List<Palette> Lpals = new List<Palette>();
        public List<Palette> Rpals = new List<Palette>();
        
        public int palcnt;

        public int getpalcnt()
        {
            return Lpals.Count + Rpals.Count;
        }

        public byte[] getdata(int head, Enum mode = null)
        {
            List<byte[]> bytelist = new List<byte[]>();

            palcnt = Lpals.Count + Rpals.Count;

            byte[] palcntbyte = BitConverter.GetBytes(palcnt);

            byte[] UNISTheader = new byte[] { 255, 255, 00, 00, 01, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };

            byte[] UNIELheader = new byte[] { 00, 00, 00, 00 };

            palcntbyte.CopyTo(UNISTheader, 12);
            palcntbyte.CopyTo(UNIELheader, 0);

            switch (mode)
            {
                case pal_game.UNIEL:
                    Console.WriteLine("UNIEL output type");


                    bytelist.Insert(0, UNIELheader);

                    Rpals.Reverse();
                    Lpals.Reverse();

                    foreach (Palette pal in Rpals)
                    {
                        //Console.WriteLine("here we go "+pal.colors[0][0]+" "+Rpals.Count);
                        byte[] paldat = pal.getdata();
                        bytelist.Insert(1, paldat);
                    }
                    foreach (Palette pal in Lpals)
                    {
                        byte[] paldat = pal.getdata();
                        bytelist.Insert(1, paldat);
                    }

                    break;
                case pal_game.Melty:
                    Console.WriteLine("Melty output type");

                    byte[] MeltyHeader = new byte[] { (byte)Lpals.Count, 00, 00, 00 };
                    bytelist.Insert(0, MeltyHeader);

                    Rpals.Reverse();
                    Lpals.Reverse();

                    foreach (Palette pal in Lpals)
                    {
                        byte[] paldat = pal.getdata();
                        bytelist.Insert(1, paldat);
                    }

                    break;
                case pal_game.NitroPlus:
                    Console.WriteLine("NitroPlus output type");

                    byte[] NitroHeader = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, (byte)palcnt, 0x00, 0x00, 0x00, 0x00 };

                    bytelist.Insert(0, NitroHeader);

                    Rpals.Reverse();
                    Lpals.Reverse();

                    foreach (Palette pal in Rpals)
                    {
                        byte[] paldat = pal.getdata();
                        bytelist.Insert(1, paldat);
                    }
                    foreach (Palette pal in Lpals)
                    {
                        byte[] paldat = pal.getdata();
                        bytelist.Insert(1, paldat);
                    }
                    break;
                default:
                    Console.WriteLine("UNIST output type");

                    bytelist.Insert(0, UNISTheader);

                    Rpals.Reverse();
                    Lpals.Reverse();

                    foreach (Palette pal in Rpals)
                    {
                        //Console.WriteLine("here we go "+pal.colors[0][0]+" "+Rpals.Count);
                        byte[] paldat = pal.getdata();
                        bytelist.Insert(1, paldat);
                    }
                    foreach (Palette pal in Lpals)
                    {
                        byte[] paldat = pal.getdata();
                        bytelist.Insert(1, paldat);
                    }
                    break;
            }

            byte[] palarray = bytelist.SelectMany(a => a).ToArray();

            Rpals.Reverse();
            Lpals.Reverse();

            return palarray;
        }
    }

    public class Palette
    {
        public byte[][] colors = new byte[256][];
        public Palette()
        {
            for(int i = 0; i < colors.Length; i++)
            {
                byte ibyte = Convert.ToByte(i);

                colors[i] = new byte[4] { ibyte, ibyte, ibyte, 255 };
            }
        }
        public byte[] getdata()
        {
            byte[] dataarray = new byte[1024];

            for (int i = 0; i < 256; i++)
            {
                colors[i].CopyTo(dataarray, i*4);
                //Console.WriteLine(i);
            }
            return dataarray;
        }
        
    }

    class PalMethods
    {
        public static void replacepalette(Palette left, Palette right, PalFile file, int index)
        {
            Console.WriteLine("replace index "+index+" cnt "+file.Lpals.Count);
            if (file.Lpals.Count > index && file.Lpals.Count > 0)
            {
                file.Lpals.RemoveAt(index);
                file.Lpals.Insert(index, left);
            }
            else
            {
                while (file.Lpals.Count < index)
                {
                    file.Lpals.Add(file.Lpals[0]);
                }

                file.Lpals.Insert(index, left);

                //Console.WriteLine(file.Lpals.Count);
            }
            
            if (file.Rpals.Count > index && file.Rpals.Count > 0)
            {
                file.Rpals.RemoveAt(index);
                file.Rpals.Insert(index, right);
            }
            else
            {
                while (file.Rpals.Count < index)
                {
                    file.Rpals.Add(file.Rpals[0]);
                }

                file.Rpals.Insert(index, right);

                //Console.WriteLine(file.Rpals.Count);
            }
        }
        public static void createfile(string outputdir, byte[] file)
        {
            new FileInfo(outputdir).Directory.Create();

            System.IO.File.WriteAllBytes(outputdir, file);

            Console.WriteLine("saved to " + outputdir);
        }

        public static int determineinput(byte[] file)
        {
            int mode = 0;

            byte[] RIFFheadercheck = new byte[] { 82, 73, 70, 70 };
            byte[] PNGPLTEcheck = new byte[] { 0x50, 0x4c, 0x54, 0x45 };
            byte[] PNGhead = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

            byte[] UNISTheader = new byte[] { 255, 255, 00, 00 };
            byte[] Nitroheader = new byte[] { 00, 00, 00, 00 };

            byte[] IMPLhead = new byte[] { 0x49, 0x4D, 0x50, 0x4C };

            using (MemoryStream stream = new MemoryStream(file))
            {
                using (BinaryReader streamread = new BinaryReader(stream))
                {

                    byte[] header = streamread.ReadBytes(4);

                    if (header.SequenceEqual(UNISTheader)) mode = 0; //mode 0 = unist

                    if (!header.SequenceEqual(UNISTheader) && !header.SequenceEqual(RIFFheadercheck)) mode = 1; //mode 1 = el (probably)

                    if (header.SequenceEqual(PNGhead)) mode = 2; //mode 2 = png

                    if (header.SequenceEqual(Nitroheader)) mode = 4; //mode 4 = nitro

                    if (header.SequenceEqual(RIFFheadercheck)) mode = 10; //mode 10 = pspal

                    if (header.SequenceEqual(IMPLhead)) mode = 20; //mode 20 = bbtag impl
                }
            }
            return mode;
        }

        public static PalFile flipindex(PalFile inpal)
        {
            foreach(Palette pal in inpal.Lpals)
            {
                for(int i = 0; i < inpal.Lpals.Count; i++)
                {
                    inpal.Lpals[i].colors.Reverse();
                }
            }

            foreach (Palette pal in inpal.Rpals)
            {
                for (int i = 0; i < inpal.Rpals.Count; i++)
                {
                    inpal.Rpals[i].colors.Reverse();
                }
            }

            return inpal;
        }

        public static PalFile processpalcfg(PalFile inpal, List<string> instrlist)
        {
            if (instrlist.Count > 0)
            {
                foreach (string line in instrlist)
                {
                    string processedline = line;
                    string paramstr = "";
                    string varstr = "";

                    Match comments = Regex.Match(line, @"\/\/(.*)");

                    processedline = line.Remove(comments.Index, comments.Length);

                    //Console.WriteLine(processedline);

                    Match input = Regex.Match(processedline, @"(^[^=]*)(\=)(.*)");
                    if (input.Success)
                    {
                        paramstr = processedline.Substring(input.Groups[1].Index, input.Groups[1].Length);
                        varstr = processedline.Substring(input.Groups[3].Index, input.Groups[3].Length);
                    }

                    int.TryParse(varstr, out int varint);
                    byte.TryParse(varstr, out byte varbyte);

                    //Console.WriteLine("parameter "+paramstr);
                    //Console.WriteLine("variable " + varstr);

                    switch (paramstr)
                    {
                        case "flipindex":
                            inpal = flipindex(inpal);
                            break;
                        case "autofixsides":
                            Console.WriteLine("fix sides");
                            inpal = fixpalsides2(varstr, inpal.Lpals[0], inpal.Rpals[0]);
                            break;
                        case "basealpha":
                            inpal = setpal_basealpha(inpal.Lpals[0], inpal.Rpals[0], varbyte);
                            break;
                        case "manualalpha":
                            Console.WriteLine("replacing alpha color");
                            byte[] file = File.ReadAllBytes(varstr);

                            inpal = setpalalpha(inpal.Lpals[0], inpal.Rpals[0], loadpalette(file));

                            break;
                        case "frompalcolor":
                            if(inpal.Lpals.Count >= varint)
                            {
                                //Console.WriteLine("this is good "+inpal.Lpals.Count);
                                PalMethods.replacepalette(inpal.Lpals[varint], inpal.Rpals[varint], inpal, 0);
                            }
                            else
                            {

                                Console.WriteLine("frompalcolor out of bounds (probably not a uni pal input)");
                            }
                            break;
                        case "rbo":
                            Console.WriteLine("reverse byte order config");

                            inpal = reverse_byte_order(inpal.Lpals[0], inpal.Rpals[0]);
                            
                            break;
                        case "test":
                            Console.WriteLine("test success");
                            Console.WriteLine("parameter " + paramstr + " help");
                            Console.WriteLine("variable " + varstr + " help");
                            //System.Environment.Exit(0);
                            //PSPALdir = Path.Combine(workingdir, @args[count + 1]);
                            break;
                        case "merge":
                            Match merge_var = Regex.Match(varstr, @"(.+?)(?:,|$)"); //several capture groups for several split vars
                            Regex match_params = new Regex(@"(.+?)(?:,|$)");
                            MatchCollection strmatches = match_params.Matches(varstr);

                            Console.WriteLine("matches count "+strmatches.Count+" varstr "+varstr);

                            string color_path = @varstr.Substring(strmatches[0].Groups[1].Index, strmatches[0].Groups[1].Length);
                            //Console.WriteLine("match 1 " + color_path);

                            int.TryParse(varstr.Substring(strmatches[1].Groups[1].Index, strmatches[1].Groups[1].Length), out int copy_from_sindex);
                            //Console.WriteLine("match 2 "+ copy_from_sindex);

                            int.TryParse(varstr.Substring(strmatches[2].Groups[1].Index, strmatches[2].Groups[1].Length), out int length);
                            int.TryParse(varstr.Substring(strmatches[3].Groups[1].Index, strmatches[3].Groups[1].Length), out int copy_to_index);

                            Palette use_palette = loadpalette(File.ReadAllBytes(color_path));

                            inpal.Lpals[0] = merge_pal(inpal.Lpals[0], use_palette, copy_from_sindex, length, copy_to_index);
                            inpal.Rpals[0] = merge_pal(inpal.Rpals[0], use_palette, copy_from_sindex, length, copy_to_index);

                            break;
                        default:
                            Console.WriteLine("unrecognized config variable: " + "'" + paramstr + "'");
                            break;
                    }
                }
            }
            return inpal;
        }

        public static Palette merge_pal(Palette base_pal, Palette pal, int copy_from_sindex, int length, int copy_to_index )
        {
            Console.WriteLine("merge palettes copy index "+ copy_from_sindex+" copy len "+length+" paste index "+copy_to_index);

            byte[][] paste_colors = new byte[256][];

            for (int i = copy_from_sindex, b = 0; i < copy_from_sindex+length; i++, b++)
            {
                //Console.WriteLine("loop");

                paste_colors[b] = pal.colors[i];
            }

            for(int i = 0; i < length; i++)
            {
                base_pal.colors[copy_to_index+i] = paste_colors[i];
            }


            return base_pal;
        }

        public static PalFile createpalfile(List<Palette> Lpalin, List<Palette> Rpalin)
        {
            PalFile outpal = new PalFile();

            outpal.Lpals = Lpalin;
            outpal.Rpals = Rpalin;
            outpal.palcnt = Lpalin.Count + Rpalin.Count;

            return outpal;
        }

        public static Palette getpngpalette(byte[] file)
        {
            //List<string> outlist = new List<string>();

            bool success = false;

            Console.WriteLine("getpngpal");

            Palette pngpal = new Palette();

            byte[] PNGPLTEcheck = new byte[] { 0x50, 0x4c, 0x54, 0x45 };

            using (MemoryStream stream = new MemoryStream(file))
            {
                using (BinaryReader streamread = new BinaryReader(stream))
                {
                    //while (streamread.BaseStream.Position != streamread.BaseStream.Length)
                    for(int i = 0; i < streamread.BaseStream.Length; i++)
                    {
                        //Console.WriteLine("reading "+streamread.BaseStream.Position+" and "+streamread.BaseStream.Length);

                        //outlist.Add("reading " + streamread.BaseStream.Position + " and " + streamread.BaseStream.Length);

                        byte[] findplte = streamread.ReadBytes(4);

                        //Console.WriteLine("finding plte chunk " + streamread.BaseStream.Position);
                        //Console.WriteLine("finding plte chunk " + System.Text.Encoding.Default.GetString(findplte));
                        if (findplte.SequenceEqual(PNGPLTEcheck))
                        {
                            Console.WriteLine("png PLTE chunk found");
                            byte[] pngpalbyte = streamread.ReadBytes(768);
                            for (int x = 0; x < 768; x += 3)
                            {

                                byte[] workingpal = new byte[4];

                                Array.Copy(pngpalbyte, x, workingpal, 0, workingpal.Length - 1);

                                //Console.WriteLine("bigoff " + (x) + " and " + workingpal[3]);

                                workingpal[3] = 255;

                                pngpal.colors[x / 3] = workingpal;

                                //Console.WriteLine("test "+workingcol.colors[x / 256][1]+" "+x);

                                //Console.WriteLine("coff " + x);
                            }
                            success = true;
                            break;
                        }

                        streamread.BaseStream.Position = i;

                        //if (streamread.BaseStream.Position > 18272) break;
                    }
                }
            }

            //System.IO.File.WriteAllLines("outlist.txt", outlist);

            if (!success)
            {
                Console.WriteLine("no png PLTE chunk found (image is not indexed color)");
                //for(int i = 0; i < 1024; i++)
            }

            

            return pngpal;
        }

        public static Palette loadpalette(byte[] sourcearray, int type = 0, int offset = 0)
        {
            //int numpal = BitConverter.ToInt32(sourcearray, offset - 4);

            //int type = determineinput(sourcearray);

            //if(type==100) type = determineinput(sourcearray);

            Console.WriteLine("load single palette");

            type = determineinput(sourcearray);

            Console.WriteLine("intype: " + type);

            Palette workingpalette = new Palette();
            switch (type)
            {
                case 20:
                    Console.WriteLine("impl load pal");
                    for (int x = 0; x < 1024; x += 4)
                    {

                        offset = 148;
                        byte[] workingpal = new byte[] {255, 0, 0, 0 } ;
                        Array.Copy(sourcearray, x + offset, workingpal, 1, 3);

                        workingpal = workingpal.Reverse().ToArray();

                        //Console.WriteLine("impertant R "+workingpal[0]+" G "+workingpal[1]+" B "+workingpal[2]+" alpha "+workingpal[3]);

                        workingpalette.colors[x / 4] = workingpal;
                    }
                    break;
                case 10:
                    Console.WriteLine("riff load pal");
                    for (int x = 0; x < 1024; x += 4)
                    {
                        offset = 24;
                        byte[] workingpal = new byte[] { 0, 0, 0, 255 };

                        Array.Copy(sourcearray, x + offset, workingpal, 0, 3);

                        workingpal[3] = 255;

                        //Console.WriteLine("bigoff " + (x) + " and " + workingpal[3]);

                        workingpalette.colors[x / 4] = workingpal;

                        //Console.WriteLine("test "+workingcol.colors[x / 256][1]+" "+x);


                        //Console.WriteLine("coff " + x);
                    }
                    break;
                case 2:
                    Console.WriteLine("png load pal");
                    workingpalette = getpngpalette(sourcearray);
                    break;
                default:
                    Console.WriteLine("unist load pal");
                    for (int x = 0; x < 1024; x += 4)
                    {

                        byte[] workingpal = new byte[4];

                        Array.Copy(sourcearray, x + offset, workingpal, 0, workingpal.Length);

                        //Console.WriteLine("bigoff " + (x) + " and " + workingpal[3]+" offset "+offset);

                        workingpalette.colors[x / 4] = workingpal;

                        //Console.WriteLine("test "+workingcol.colors[x / 256][1]+" "+x);


                        //Console.WriteLine("coff " + x);
                    }
                    break;
            }

            return workingpalette;
        }

        public static void replacepalettes(string inputdir, PalFile basepal, int index = 0, List<string> early_config = null)
        {
            bool fromuni = false;

            byte[] PSPAL = File.ReadAllBytes(inputdir);

            string configdir = inputdir + "_cfg.txt";

            PalFile newpal = new PalFile();
            if( determineinput(PSPAL) == 0 || determineinput(PSPAL)==1 )
            {
                newpal = loadpals(PSPAL,determineinput(PSPAL));
            }
            else
            {
                Palette PSPALp = PalMethods.loadpalette(PSPAL);

                Palette ltest = new Palette();
                PSPALp.colors.CopyTo(ltest.colors,0);

                newpal.Lpals.Add(ltest);
                newpal.Rpals.Add(PSPALp);
            }

            if(early_config.Count > 0)
            {
                Console.WriteLine("list config located for " + inputdir);
                newpal = PalMethods.processpalcfg(newpal, early_config);
            }

            if (File.Exists(configdir))
            {
                Console.WriteLine("config located for " + inputdir);

                List<string> configtextlist = File.ReadAllLines(configdir).ToList();

                //Console.WriteLine(configtextlist.Count);

                newpal = PalMethods.processpalcfg(newpal, configtextlist);
            }

            PalMethods.replacepalette(newpal.Lpals[0], newpal.Rpals[0], basepal, index);
        }


        public static PalFile loadpals(byte[] sourcearray, int type = 0)
        {
            bool two_sides = true;
            int col_len = 1024; //length of a 256 color table
            int offset = 0;
            switch (type)
            {
                case 0:
                    offset = 16; //st mode
                    break;
                case 1:
                    offset = 4; //el mode
                    break;
                case 2:
                    col_len = 768;
                    break;
                case 100: //melty mode
                    offset = 4;
                    two_sides = false;
                    break;
                case 3: //text list
                    break;
                case 4: //nitro+
                    offset = 16;
                    break;
                case 10:
                    //headeroff = 0;
                    break;
            }

            int numpal = (sourcearray.Length - offset) / 1024;

            Console.WriteLine("num total "+numpal);

            PalFile retpal = new PalFile();
            retpal.palcnt = numpal;

            List<Palette> LEFTpalettelist = new List<Palette>();
            List<Palette> RIGHTpalettelist = new List<Palette>();

            for (int i = offset, p = 0; p < numpal; i += 1024, p++)
            {
                Console.WriteLine("num total " + numpal+" at "+i+" num "+p);
                Palette workingcol = loadpalette(sourcearray,0,i);
                if(two_sides)
                {
                    if (p >= numpal / 2)
                    {
                        RIGHTpalettelist.Add(workingcol);

                        Console.WriteLine("side 2");
                    }
                    else
                    {
                        LEFTpalettelist.Add(workingcol);

                        Console.WriteLine("side 1");
                    }
                }
                else
                {
                    LEFTpalettelist.Add(workingcol);
                }

                //Console.WriteLine(i);
                //Console.WriteLine(workingcol.Length);
                //Console.WriteLine("loaded " + palettelist.Count + " palettes");

                byte[] paldat = workingcol.getdata();

                string workingdir = Directory.GetCurrentDirectory();

                //Console.WriteLine(paldat[0]+" and "+workingcol.getdata()[0]);

                bool debugoutput = false;

                if(debugoutput) System.IO.File.WriteAllBytes(Path.Combine(workingdir, "debug/debug_"+p+".pal"), paldat);
            }

            retpal.Lpals = LEFTpalettelist;
            retpal.Rpals = RIGHTpalettelist;

            return retpal;
        }

        public static PalFile setpal_basealpha(Palette lside, Palette rside, byte val)
        {
            PalFile fixcol = new PalFile();

            for (int i = 0; i < 255; i++)
            {
                lside.colors[i][3] = val;
                rside.colors[i][3] = val;
            }

            fixcol.Rpals.Add(rside);
            fixcol.Lpals.Add(lside);

            return fixcol;
        }

        public static PalFile reverse_byte_order(Palette lside, Palette rside)
        {
            PalFile fixcol = new PalFile();

            for(int i = 0; i < lside.colors.Length; i++)
            {
                byte[] workingpal = new byte[] { 255, 0, 0, 0 };
                Array.Copy(lside.colors[i], 0, workingpal, 1, 3);

                lside.colors[i] = workingpal.Reverse().ToArray();

                byte[] workingpal2 = new byte[] { 255, 0, 0, 0 };
                Array.Copy(rside.colors[i], 0, workingpal2, 1, 3);

                rside.colors[i] = workingpal.Reverse().ToArray();
            }

            fixcol.Rpals.Add(rside);
            fixcol.Lpals.Add(lside);

            return fixcol;
        }

        public static PalFile setpalalpha(Palette lside, Palette rside, Palette alphamap)
        {
            PalFile fixcol = new PalFile();

            for(int i = 0; i < 255; i++)
            {
                lside.colors[i][3] = alphamap.colors[i][0];
                rside.colors[i][3] = alphamap.colors[i][0];
            }

            fixcol.Rpals.Add(rside);
            fixcol.Lpals.Add(lside);

            return fixcol;
        }

        public static PalFile fixpalsides2(string character, Palette lside, Palette rside)
        {
            PalFile fixcol = new PalFile();

            int[] sethcolormapfrom = new int[] { 11, 12, 13, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153 };
            int[] sethcolormapto = new int[] { 27, 28, 29, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169 };

            int[] hildacolormapfrom = new int[] { 26, 27, 28, 64, 65, 66, 67, 68, 80, 81, 82, 83 };
            int[] hildacolormapto = new int[] { 42, 43, 44, 70, 71, 72, 73, 74, 85, 86, 87, 88 };

            int[] stmcolormapfrom = new int[] { 11, 12, 13 };
            int[] stmcolormapto = new int[] { 27, 28, 29 };

            int[] arrayfrom = new int[4];
            int[] arrayto = new int[4];

            switch (character)
            {
                case "seth":
                    arrayfrom = sethcolormapfrom;
                    arrayto = sethcolormapto;
                    break;
                case "hilda":
                    arrayfrom = hildacolormapfrom;
                    arrayto = hildacolormapto;
                    break;
                case "rentaro":
                    arrayfrom = stmcolormapfrom;
                    arrayto = stmcolormapto;
                    break;

                default:

                    break;
            }

            Console.WriteLine("fixing sides for: "+character);

            for (int i = 0; i < arrayfrom.Length; i++)
            {
                Console.WriteLine("fixing sides for loop: " + character);

                rside.colors[arrayfrom[i]] = lside.colors[arrayto[i]];
                rside.colors[arrayto[i]] = lside.colors[arrayfrom[i]];
            }

            fixcol.Rpals.Add(rside);
            fixcol.Lpals.Add(lside);
            
            return fixcol;
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

        public static byte[] editsave(byte[] save)
        {
            //byte[] overwrite20 = new byte[] { 0xff  }
            uint unlock = 0xffffffff;
            using (MemoryStream stream = new MemoryStream(save))
            {
                using (BinaryWriter writesave = new BinaryWriter(stream))
                {
                    writesave.Seek(287948, 0);

                    for (int i = 0; i < 32; i++)
                    {
                        writesave.Write(unlock);
                    }
                }
            }

            return save;
        }
    }
}
